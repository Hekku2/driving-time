using System.Globalization;
using System.Xml;
using System.Xml.Linq;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;

namespace DrivingTime.Pipeline;

public sealed record BuildingBounds(double MinEast, double MinNorth, double MinHeight, double MaxEast, double MaxNorth, double MaxHeight);
public sealed record BuildingReference(string BuildingId, string Element, string Href, string Status);
public sealed record BuildingInspection(string Id, string Crs, BuildingBounds Envelope, BuildingBounds GeometryBounds,
    Dictionary<string, int> GeometryCounts, int CoordinateTuples, int InteriorRings, int Lod1Polygons,
    Bounds ProjectedBounds, bool OverlapsRoadRegion, ProjectedPoint[] AddressPoints, string[] CreationDates, string[] ExternalMetadata, string[] Diagnostics);
public sealed record BuildingReport(int SchemaVersion, string Format, string SchemaLocation, string? SourceTimestamp,
    string? NumberMatched, int NumberReturned, string? NextPage, string[] Namespaces,
    BuildingInspection[] Buildings, BuildingReference[] References, string[] Diagnostics);

/// <summary>Observed WFS 2 / CityGML building 2 / GML 3.1 inspection subset, not mesh conversion or an XSD validator.</summary>
public static class BuildingXml
{
    public const int MaxBytes = 4 * 1024 * 1024;
    public const string CompoundCrs = "urn:ogc:def:crs,crs:EPSG::3879,crs:EPSG::3900";
    public static readonly XNamespace Wfs = "http://www.opengis.net/wfs/2.0", Gml = "http://www.opengis.net/gml",
        Bldg = "http://www.opengis.net/citygml/building/2.0", Core = "http://www.opengis.net/citygml/2.0",
        Xlink = "http://www.w3.org/1999/xlink", Xsi = "http://www.w3.org/2001/XMLSchema-instance";

    public static BuildingReport Inspect(byte[] bytes, Bounds roadRegion, int maxBuildings = 5)
    {
        if (bytes.Length is < 1 or > MaxBytes) throw new ArgumentException("Building XML must contain 1 byte–4 MiB.");
        if (maxBuildings is < 1 or > 5) throw new ArgumentException("Building cap must be 1–5.");
        using var input = new MemoryStream(bytes);
        using var reader = XmlReader.Create(input, new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null,
            MaxCharactersInDocument = MaxBytes, IgnoreComments = true
        });
        reader.MoveToContent();
        if (reader.LocalName != "FeatureCollection" || reader.NamespaceURI != Wfs.NamespaceName)
            throw new ArgumentException("Expected WFS 2.0 FeatureCollection; exception reports, CityModel and other versions are unsupported.");
        var schema = reader.GetAttribute("schemaLocation", Xsi.NamespaceName) ?? "";
        var timestamp = reader.GetAttribute("timeStamp");
        var matched = reader.GetAttribute("numberMatched");
        var returned = reader.GetAttribute("numberReturned") ?? throw new ArgumentException("Missing WFS numberReturned.");
        var nextPage = reader.GetAttribute("next");
        var buildings = new List<BuildingInspection>();
        var ids = new Dictionary<string, (string BuildingId, XName Name)>();
        var references = new List<(string Id, string Element, string Href)>();
        var namespaces = new HashSet<string> { Wfs.NamespaceName };
        var diagnostics = new List<string>();
        while (reader.Read())
        {
            if (reader.NodeType != XmlNodeType.Element) continue;
            if (reader.Depth == 1 && (reader.LocalName != "member" || reader.NamespaceURI != Wfs.NamespaceName))
                throw new ArgumentException($"Unsupported FeatureCollection child {{{reader.NamespaceURI}}}{reader.LocalName}.");
            if (reader.Name == "" || reader.NamespaceURI != Wfs.NamespaceName || reader.LocalName != "member") continue;
            using var subtree = reader.ReadSubtree();
            var member = XElement.Load(subtree);
            var feature = member.Elements().SingleOrDefault();
            if (feature == null || feature.Name != Bldg + "Building")
                throw new ArgumentException("Each WFS member must contain one CityGML 2.0 Building.");
            if (buildings.Count >= maxBuildings) throw new ArgumentException("Response exceeds requested building cap; narrow the sample.");
            var id = (string?)feature.Attribute(Gml + "id") ?? throw new ArgumentException("Building lacks gml:id.");
            foreach (var e in feature.DescendantsAndSelf())
            {
                namespaces.Add(e.Name.NamespaceName);
                if (e.Name.LocalName is "Polygon" or "LinearRing" or "Solid" or "posList" or "pos" && e.Name.Namespace != Gml)
                    throw new ArgumentException($"{id}: geometry uses an unsupported namespace: {e.Name}.");
                if (e.Attribute(Gml + "id") is { } a && !ids.TryAdd(a.Value, (id, e.Name)))
                    throw new ArgumentException($"Duplicate gml:id {a.Value}.");
                if (e.Attribute(Xlink + "href") is { } href) references.Add((id, e.Name.ToString(), href.Value));
            }
            buildings.Add(InspectBuilding(feature, id, roadRegion));
        }
        if (!int.TryParse(returned, NumberStyles.None, CultureInfo.InvariantCulture, out var count) || count != buildings.Count || count == 0)
            throw new ArgumentException("Empty or inconsistent WFS numberReturned/member count.");
        var resolved = references.Select(r =>
        {
            var status = !r.Href.StartsWith('#') ? "external-not-fetched" :
                !ids.TryGetValue(r.Href[1..], out var target) ? "unresolved-local" :
                r.Element != (Gml + "surfaceMember").ToString() ? "unsupported-reference-property" :
                target.Name != Gml + "Polygon" ? "unsupported-target-type" :
                target.BuildingId != r.Id ? "cross-building-not-supported" : "resolved-in-building";
            return new BuildingReference(r.Id, r.Element, r.Href, status);
        }).ToArray();
        foreach (var r in resolved.Where(r => r.Status != "resolved-in-building")) diagnostics.Add($"{r.BuildingId}: {r.Status}: {r.Href}");
        if (nextPage != null || matched != returned) diagnostics.Add("Bounded partial WFS response; no paging performed and no complete inventory claimed.");
        if (!buildings.Any(b => b.OverlapsRoadRegion)) diagnostics.Add("No building bounds overlap the configured road region; select a different bounded sample before conversion.");
        return new(1, "WFS 2.0 / CityGML building/core 2.0 / GML 3.1 namespace", schema, timestamp,
            matched, count, nextPage, namespaces.Order().ToArray(), buildings.ToArray(), resolved, diagnostics.ToArray());
    }

    private static BuildingInspection InspectBuilding(XElement feature, string id, Bounds roadRegion)
    {
        var envelope = feature.Element(Gml + "boundedBy")?.Element(Gml + "Envelope")
            ?? throw new ArgumentException($"{id}: missing building envelope.");
        var crs = (string?)envelope.Attribute("srsName") ?? "";
        if (crs != CompoundCrs || (string?)envelope.Attribute("srsDimension") != "3")
            throw new ArgumentException($"{id}: require observed GK25/N2000 compound CRS and three dimensions; other CRS/axes unsupported.");
        var lower = Values(envelope.Element(Gml + "lowerCorner")?.Value ?? "");
        var upper = Values(envelope.Element(Gml + "upperCorner")?.Value ?? "");
        if (lower.Length != 3 || upper.Length != 3) throw new ArgumentException($"{id}: invalid envelope corners.");
        var bounds = new BuildingBounds(lower[0], lower[1], lower[2], upper[0], upper[1], upper[2]);
        if (lower.Where((n, i) => n > upper[i]).Any() || lower[0] is < 25487000 or > 25516000 || lower[1] is < 6655000 or > 6690000)
            throw new ArgumentException($"{id}: envelope bounds/physical E,N axes outside Helsinki; do not use EPSG authority N,E order for this observed response.");
        var notes = new List<string>();
        var known = new HashSet<string> { "boundedBy", "Envelope", "lowerCorner", "upperCorner", "Solid", "exterior", "interior", "CompositeSurface", "surfaceMember", "Polygon", "LinearRing", "posList", "pos", "MultiSurface", "MultiPoint", "pointMember", "Point" };
        foreach (var group in feature.Descendants().Where(e => e.Name.Namespace == Gml && !known.Contains(e.Name.LocalName)).GroupBy(e => e.Name))
            notes.Add($"{id}: unsupported GML construct {group.Key}, count={group.Count()}.");
        foreach (var group in feature.Descendants().Where(e => e.Name.Namespace == Bldg &&
            (e.Name.LocalName.StartsWith("lod") && e.Name.LocalName is not ("lod1Solid" or "lod2Solid" or "lod2MultiSurface") || e.Name.LocalName == "BuildingPart")).GroupBy(e => e.Name))
            notes.Add($"{id}: unsupported building construct {group.Key}, count={group.Count()}.");
        foreach (var declared in feature.Descendants().Attributes("srsName"))
            if (declared.Value != CompoundCrs) throw new ArgumentException($"{id}: conflicting geometry CRS {declared.Value}.");
        var coordinates = new List<ProjectedPoint>();
        var addressPoints = new List<ProjectedPoint>();
        foreach (var element in feature.Descendants().Where(e => e.Name == Gml + "posList" || e.Name == Gml + "pos"))
        {
            // This observed service omits CRS on positions: explicit building envelope is the checked fallback.
            var dimension = element.AncestorsAndSelf().Attributes("srsDimension").FirstOrDefault()?.Value ?? "3";
            if (dimension != "3") throw new ArgumentException($"{id}: only E,N,H triples supported.");
            var values = Values(element.Value);
            if (values.Length == 0 || values.Length % 3 != 0 || element.Name == Gml + "pos" && values.Length != 3)
                throw new ArgumentException($"{id}: incomplete position tuples.");
            var positions = element.Ancestors(Core + "Address").Any() ? addressPoints : coordinates;
            for (int i = 0; i < values.Length; i += 3) positions.Add(new(values[i], values[i + 1], values[i + 2]));
            if (coordinates.Count + addressPoints.Count > 100000) throw new ArgumentException("Coordinate tuple cap exceeded.");
        }
        if (coordinates.Count == 0) throw new ArgumentException($"{id}: no inline geometry positions.");
        foreach (var ring in feature.Descendants(Gml + "LinearRing"))
        {
            var lists = ring.Elements(Gml + "posList").ToArray();
            var positions = ring.Elements(Gml + "pos").ToArray();
            if (lists.Length != 1 || positions.Length != 0) { notes.Add($"{id}: unsupported LinearRing encoding; require one inline posList."); continue; }
            var values = Values(lists[0].Value);
            if (values.Length < 12 || values.Length % 3 != 0 || !values.Take(3).SequenceEqual(values.TakeLast(3)))
                throw new ArgumentException($"{id}: LinearRing requires at least four closed triples.");
        }
        var actual = new BuildingBounds(coordinates.Min(p => p.East), coordinates.Min(p => p.North), coordinates.Min(p => p.Height),
            coordinates.Max(p => p.East), coordinates.Max(p => p.North), coordinates.Max(p => p.Height));
        if (actual.MinEast < bounds.MinEast - 0.01 || actual.MaxEast > bounds.MaxEast + 0.01 || actual.MinNorth < bounds.MinNorth - 0.01 ||
            actual.MaxNorth > bounds.MaxNorth + 0.01 || actual.MinHeight < bounds.MinHeight - 0.01 || actual.MaxHeight > bounds.MaxHeight + 0.01)
            notes.Add($"{id}: positions exceed declared envelope; inspect metadata/geometry before conversion.");
        var transformed = new[] { ToTm35(bounds.MinEast, bounds.MinNorth), ToTm35(bounds.MinEast, bounds.MaxNorth),
            ToTm35(bounds.MaxEast, bounds.MinNorth), ToTm35(bounds.MaxEast, bounds.MaxNorth) };
        var projected = new Bounds { MinEast = transformed.Min(p => p.East), MinNorth = transformed.Min(p => p.North),
            MaxEast = transformed.Max(p => p.East), MaxNorth = transformed.Max(p => p.North) };
        var overlap = projected.MaxEast >= roadRegion.MinEast && projected.MinEast <= roadRegion.MaxEast &&
            projected.MaxNorth >= roadRegion.MinNorth && projected.MinNorth <= roadRegion.MaxNorth;
        var counts = feature.Descendants().Where(e => e.Name.Namespace == Gml || e.Name.Namespace == Bldg && e.Name.LocalName.StartsWith("lod"))
            .GroupBy(e => e.Name.ToString()).OrderBy(g => g.Key).ToDictionary(g => g.Key, g => g.Count());
        if (addressPoints.Count > 0) notes.Add($"{id}: {addressPoints.Count} address point(s) excluded from building geometry bounds; source address heights are not roof/base controls.");
        return new(id, crs, bounds, actual, counts, coordinates.Count, feature.Descendants(Gml + "interior").Count(),
            feature.Element(Bldg + "lod1Solid")?.Descendants(Gml + "Polygon").Count() ?? 0, projected, overlap,
            addressPoints.ToArray(),
            feature.Descendants(Core + "creationDate").Select(e => e.Value).Distinct().Order().ToArray(),
            feature.Descendants(Core + "externalReference").Select(e => e.ToString(SaveOptions.DisableFormatting)).ToArray(), notes.ToArray());
    }
    private static double[] Values(string text)
    {
        var result = text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Select(s => double.Parse(s, CultureInfo.InvariantCulture)).ToArray();
        if (result.Any(n => !double.IsFinite(n))) throw new ArgumentException("Coordinates must be finite.");
        return result;
    }
    public static (double East, double North) ToTm35(double east, double north)
    {
        CoordinateFrame.Finite(east, north);
        var gk25 = $"PROJCS[\"EUREF-FIN / GK25FIN (physical E,N)\",{Tm35FinProjection.GeographicWkt},PROJECTION[\"Transverse_Mercator\"],PARAMETER[\"latitude_of_origin\",0],PARAMETER[\"central_meridian\",25],PARAMETER[\"scale_factor\",1],PARAMETER[\"false_easting\",25500000],PARAMETER[\"false_northing\",0],UNIT[\"metre\",1],AXIS[\"Easting\",EAST],AXIS[\"Northing\",NORTH]]";
        var factory = new CoordinateSystemFactory();
        var transform = new CoordinateTransformationFactory().CreateFromCoordinateSystems(factory.CreateFromWkt(gk25), factory.CreateFromWkt(Tm35FinProjection.ProjectedWkt));
        var p = transform.MathTransform.Transform(new[] { east, north });
        CoordinateFrame.Finite(p[0], p[1]);
        return (p[0], p[1]);
    }
}
