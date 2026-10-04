using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DrivingTime.Pipeline;

public sealed record RoadBounds(double South, double West, double North, double East);
public sealed record RoadWay(long Id, string? Timestamp, long[] Nodes, GeographicPoint[] Geometry, Dictionary<string, string> Tags);
public sealed record RoadExtract(string Generator, string OsmBase, RoadWay[] Ways);
public sealed record RoadRequest(int SchemaVersion, string StreetName, Bounds Region, string CachePath)
{
    public RoadBounds Bounds()
    {
        if (SchemaVersion != 1 || string.IsNullOrWhiteSpace(StreetName) || StreetName.Length > 100 ||
            StreetName.Any(char.IsControl) || string.IsNullOrWhiteSpace(CachePath))
            throw new ArgumentException("Road config needs schemaVersion=1, streetName and cachePath.");
        CoordinateFrame.Finite(Region.MinEast, Region.MinNorth, Region.MaxEast, Region.MaxNorth);
        if (Region.MinEast < 383736.0361374 || Region.MaxEast > 384736.0361374 ||
            Region.MinNorth < 6673710.89320437 || Region.MaxNorth > 6674710.89320437 ||
            Region.MaxEast <= Region.MinEast || Region.MaxNorth <= Region.MinNorth)
            throw new ArgumentException("Road region must have positive extents inside P09's selected 1 km square.");
        var projection = new Tm35FinProjection();
        var corners = new[] { projection.ToEurefFin(Region.MinEast, Region.MinNorth), projection.ToEurefFin(Region.MinEast, Region.MaxNorth),
            projection.ToEurefFin(Region.MaxEast, Region.MinNorth), projection.ToEurefFin(Region.MaxEast, Region.MaxNorth) };
        return new(corners.Min(p => p.LatitudeDegrees), corners.Min(p => p.LongitudeDegrees),
            corners.Max(p => p.LatitudeDegrees), corners.Max(p => p.LongitudeDegrees));
    }
    public string Query()
    {
        var b = Bounds();
        return FormattableString.Invariant($"[out:json][timeout:20][maxsize:16777216];way[\"highway\"][\"name\"={JsonSerializer.Serialize(StreetName, RoadSample.Json)}]({b.South:R},{b.West:R},{b.North:R},{b.East:R});out meta geom;");
    }
}

public static class RoadSample
{
    public const int MaxBytes = 10 * 1024 * 1024;
    public const string Endpoint = "https://overpass-api.de/api/interpreter";
    public static readonly JsonSerializerOptions Json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true,
        AllowDuplicateProperties = false, RespectRequiredConstructorParameters = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };
    public static string Hash(byte[] bytes) => Convert.ToHexStringLower(SHA256.HashData(bytes));

    public static RoadExtract Parse(byte[] bytes)
    {
        if (bytes.Length == 0 || bytes.Length > MaxBytes) throw new ArgumentException("OSM input must contain 1 byte–10 MiB of raw Overpass JSON.");
        try
        {
            using var doc = JsonDocument.Parse(bytes);
            var root = doc.RootElement;
            RejectDuplicates(root);
            if (root.TryGetProperty("remark", out var remark)) throw new ArgumentException($"Overpass reported incomplete/error output: {remark}");
            if (root.GetProperty("version").GetDouble() != 0.6) throw new ArgumentException("Expected OSM version 0.6.");
            var generator = Text(root, "generator");
            var osmBase = Text(root.GetProperty("osm3s"), "timestamp_osm_base");
            Timestamp(osmBase);
            var elements = root.GetProperty("elements");
            if (elements.GetArrayLength() is < 1 or > 1000) throw new ArgumentException("Expected 1–1000 highway ways; empty output is not a road sample.");
            var ways = new List<RoadWay>();
            var ids = new HashSet<long>();
            var locations = new Dictionary<long, GeographicPoint>();
            var vertexCount = 0;
            foreach (var e in elements.EnumerateArray())
            {
                if (Text(e, "type") != "way") throw new ArgumentException("Use the documented way-only out meta geom JSON query; nodes/relations/GeoJSON unsupported.");
                var id = e.GetProperty("id").GetInt64();
                if (id <= 0 || !ids.Add(id)) throw new ArgumentException("Way IDs must be positive and unique.");
                var nodes = e.GetProperty("nodes").EnumerateArray().Select(n => n.GetInt64()).ToArray();
                var geometry = e.GetProperty("geometry").EnumerateArray().Select(p => new GeographicPoint(p.GetProperty("lat").GetDouble(), p.GetProperty("lon").GetDouble())).ToArray();
                if (nodes.Length < 2 || nodes.Length != geometry.Length || (vertexCount += nodes.Length) > 100000)
                    throw new ArgumentException($"Way {id} has incomplete geometry or exceeds the 100000 vertex cap.");
                for (var i = 0; i < nodes.Length; i++)
                {
                    var p = geometry[i];
                    CoordinateFrame.Finite(p.LatitudeDegrees, p.LongitudeDegrees);
                    if (nodes[i] <= 0 || p.LatitudeDegrees is < 59 or > 71 || p.LongitudeDegrees is < 19 or > 32)
                        throw new ArgumentException($"Way {id} has invalid node ID or coordinates outside Finland.");
                    if (locations.TryGetValue(nodes[i], out var previous) && previous != p)
                        throw new ArgumentException($"Node {nodes[i]} has conflicting coordinates.");
                    locations[nodes[i]] = p;
                }
                var tags = e.GetProperty("tags").EnumerateObject().ToDictionary(t => t.Name, t => t.Value.GetString() ?? throw new ArgumentException("Null tag value."));
                if (!tags.TryGetValue("highway", out var highway) || string.IsNullOrWhiteSpace(highway)) throw new ArgumentException($"Way {id} lacks highway.");
                string? timestamp = e.TryGetProperty("timestamp", out var t) ? t.GetString() : null;
                if (timestamp != null) Timestamp(timestamp);
                ways.Add(new(id, timestamp, nodes, geometry, tags));
            }
            return new(generator, osmBase, ways.ToArray());
        }
        catch (Exception e) when (e is JsonException or KeyNotFoundException or InvalidOperationException or FormatException or OverflowException)
        { throw new ArgumentException("Malformed/incomplete Overpass JSON: require version, generator, osm3s timestamp, highway ways, node IDs and matching full geometry. " + e.Message, e); }
    }
    private static string Text(JsonElement e, string key) => !string.IsNullOrWhiteSpace(e.GetProperty(key).GetString()) ? e.GetProperty(key).GetString()! : throw new ArgumentException($"Missing {key}.");
    private static void Timestamp(string s)
    {
        if (!DateTimeOffset.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None, out _) || !s.EndsWith('Z')) throw new ArgumentException("Expected UTC OSM timestamp ending in Z.");
    }
    private static void RejectDuplicates(JsonElement e)
    {
        if (e.ValueKind == JsonValueKind.Object)
        {
            var keys = new HashSet<string>();
            foreach (var p in e.EnumerateObject()) { if (!keys.Add(p.Name)) throw new ArgumentException($"Duplicate JSON field: {p.Name}."); RejectDuplicates(p.Value); }
        }
        else if (e.ValueKind == JsonValueKind.Array) foreach (var item in e.EnumerateArray()) RejectDuplicates(item);
    }
    public static object Summary(RoadExtract extract, RoadRequest request)
    {
        var b = request.Bounds();
        var points = extract.Ways.SelectMany(w => w.Geometry).ToArray();
        return new
        {
            wayCount = extract.Ways.Length, uniqueNodeCount = extract.Ways.SelectMany(w => w.Nodes).Distinct().Count(), vertexCount = points.Length,
            requestBounds = b, actualBounds = new RoadBounds(points.Min(p => p.LatitudeDegrees), points.Min(p => p.LongitudeDegrees), points.Max(p => p.LatitudeDegrees), points.Max(p => p.LongitudeDegrees)),
            verticesOutsideRequest = points.Count(p => p.LatitudeDegrees < b.South || p.LatitudeDegrees > b.North || p.LongitudeDegrees < b.West || p.LongitudeDegrees > b.East),
            extract.OsmBase, extract.Generator,
            tags = new[] { "highway", "oneway", "lanes", "maxspeed", "layer", "bridge", "tunnel", "access", "motor_vehicle", "construction" }.ToDictionary(key => key,
                key => new { present = extract.Ways.Count(w => w.Tags.ContainsKey(key)), missing = extract.Ways.Count(w => !w.Tags.ContainsKey(key)),
                    values = extract.Ways.Where(w => w.Tags.ContainsKey(key)).GroupBy(w => w.Tags[key]).OrderBy(g => g.Key, StringComparer.Ordinal).ToDictionary(g => g.Key, g => g.Count()) }),
            ways = extract.Ways.Select(w => new { w.Id, w.Timestamp, w.Tags }),
            limitations = "Complete ways may extend beyond request bounds; no clipping, graph, roads, heights, route suitability or missing-tag defaults. WGS84 bounds use P07 approximate EUREF-FIN policy."
        };
    }
}
