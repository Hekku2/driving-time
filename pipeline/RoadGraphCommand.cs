using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Xml;

namespace DrivingTime.Pipeline;

public sealed record RoadGraphConfiguration(int SchemaVersion, string InputPath, string ProvenancePath,
    string OutputPath, string DebugPath, Bounds Region, bool AllowApproximateDatum);

public static class RoadGraphCommand
{
    public const string Usage = "road-graph --config <file.json> (offline only; paths relative to config)";
    public static int Run(string[] args)
    {
        try
        {
            if (args is not ["--config", _]) throw new ArgumentException(Usage);
            var path = Path.GetFullPath(args[1]);
            var config = JsonSerializer.Deserialize<RoadGraphConfiguration>(File.ReadAllBytes(path), RoadSample.Json)
                ?? throw new ArgumentException("Graph config is null.");
            if (config.SchemaVersion != 1) throw new ArgumentException("Graph config requires schemaVersion=1.");
            string Resolve(string p) => !string.IsNullOrWhiteSpace(p) ? Path.GetFullPath(p, Path.GetDirectoryName(path)!) : throw new ArgumentException("Graph paths must be nonempty.");
            var input = Resolve(config.InputPath); var provenancePath = Resolve(config.ProvenancePath);
            var output = Resolve(config.OutputPath); var debug = Resolve(config.DebugPath);
            if (new[] { input, provenancePath, output, debug, path }.Distinct().Count() != 5)
                throw new ArgumentException("Config, input, provenance, output and debug paths must all differ.");
            var bytes = RoadSampleCommand.ReadBounded(File.OpenRead(input)).GetAwaiter().GetResult();
            using var metadata = JsonDocument.Parse(RoadSampleCommand.ReadBounded(File.OpenRead(provenancePath)).GetAwaiter().GetResult());
            var p = metadata.RootElement;
            if (p.GetProperty("schemaVersion").GetInt32() != 1 || p.GetProperty("payloadSha256").GetString() != RoadSample.Hash(bytes) ||
                p.GetProperty("byteCount").GetInt32() != bytes.Length)
                throw new ArgumentException("Raw input provenance checksum/size/schema mismatch; restore the P10 cache.");
            foreach (var key in new[] { "license", "attribution", "horizontalCrs", "verticalDatum", "importedOrRetrievedUtc", "source" })
                if (string.IsNullOrWhiteSpace(p.GetProperty(key).GetString())) throw new ArgumentException($"Provenance lacks {key}; use P10 acquisition/import.");
            var extract = RoadSample.Parse(bytes);
            if (p.GetProperty("osmBase").GetString() != extract.OsmBase || p.GetProperty("generator").GetString() != extract.Generator)
                throw new ArgumentException("Provenance source timestamp/generator mismatch.");
            using var raw = JsonDocument.Parse(bytes);
            var graph = RoadGraph.Convert(extract, config.Region, config.AllowApproximateDatum, raw.RootElement, p);
            Directory.CreateDirectory(Path.GetDirectoryName(output)!);
            Directory.CreateDirectory(Path.GetDirectoryName(debug)!);
            File.WriteAllText(output, JsonSerializer.Serialize(graph, RoadSample.Json) + "\n", new UTF8Encoding(false));
            WriteSvg(graph, debug);
            Console.WriteLine($"Graph v1: {graph.SourceWays.Length} source ways, {graph.Nodes.Length} nodes, {graph.Segments.Length} segments, {graph.Arcs.Length} directed arcs, {graph.Components.Length} components, {graph.IsolatedSegments.Length} isolated segments, {graph.Diagnostics.Length} diagnostics.");
            Console.WriteLine($"Wrote {output}\nDebug plot: {debug}");
            return 0;
        }
        catch (Exception e) when (e is ArgumentException or IOException or UnauthorizedAccessException or JsonException or KeyNotFoundException or InvalidOperationException)
        {
            Console.Error.WriteLine("Road graph error: " + e.Message);
            Console.Error.WriteLine("Check graph config and P10 raw/provenance paths; no network retrieval is attempted.");
            return 2;
        }
    }

    public static void WriteSvg(IntermediateRoadGraph graph, string path)
    {
        var b = graph.ClipRegion;
        var scale = Math.Min(880 / (b.MaxEast - b.MinEast), 650 / (b.MaxNorth - b.MinNorth));
        string X(double e) => (60 + (e - b.MinEast) * scale).ToString("0.###", CultureInfo.InvariantCulture);
        string Y(double n) => (720 - (n - b.MinNorth) * scale).ToString("0.###", CultureInfo.InvariantCulture);
        using var w = XmlWriter.Create(path, new XmlWriterSettings { Indent = true, Encoding = new UTF8Encoding(false) });
        w.WriteStartElement("svg", "http://www.w3.org/2000/svg"); w.WriteAttributeString("viewBox", "0 0 1040 850");
        void Element(string name, params (string Key, string Value)[] attributes)
        { w.WriteStartElement(name); foreach (var a in attributes) w.WriteAttributeString(a.Key, a.Value); w.WriteEndElement(); }
        void Text(double e, double n, string label)
        { w.WriteStartElement("text"); w.WriteAttributeString("x", X(e)); w.WriteAttributeString("y", Y(n)); w.WriteAttributeString("font-size", "10"); w.WriteString(label); w.WriteEndElement(); }
        Element("rect", ("width", "1040"), ("height", "850"), ("fill", "white"));
        Text(b.MinEast, b.MaxNorth + 12 / scale, "P11 EPSG:3067 metres | north up, east right | arrows: base direction | orange: cuts | red: junctions");
        Element("rect", ("x", X(b.MinEast)), ("y", Y(b.MaxNorth)), ("width", ((b.MaxEast-b.MinEast)*scale).ToString("R", CultureInfo.InvariantCulture)),
            ("height", ((b.MaxNorth-b.MinNorth)*scale).ToString("R", CultureInfo.InvariantCulture)), ("fill", "none"), ("stroke", "#999"));
        foreach (var s in graph.Segments)
        {
            w.WriteStartElement("polyline"); w.WriteAttributeString("points", string.Join(" ", s.Geometry.Select(p => X(p.East) + "," + Y(p.North))));
            w.WriteAttributeString("fill", "none"); w.WriteAttributeString("stroke", s.Tags.GetValueOrDefault("layer", "0") == "0" ? "#236a9d" : "#9260b0"); w.WriteAttributeString("stroke-width", "2");
            w.WriteStartElement("title"); w.WriteString(s.Id + " " + s.Direction + " " + JsonSerializer.Serialize(s.Tags)); w.WriteEndElement(); w.WriteEndElement();
            var a = s.Geometry[0]; var c = s.Geometry[^1];
            Text((a.East+c.East)/2, (a.North+c.North)/2, $"{s.OsmWayId} ({s.Direction})");
            foreach (var (arc, index) in graph.Arcs.Where(a => a.SegmentId == s.Id).Select((arc, index) => (arc, index)))
            {
                var pair = s.Geometry.Length / 2; var start = s.Geometry[pair-1]; var end = s.Geometry[pair];
                var dx = end.East-start.East; var dy = end.North-start.North;
                var len = double.Hypot(dx,dy); var sign = s.Direction == "reverse" || (s.Direction == "both" && index == 1) ? -1 : 1;
                var mx = (start.East+end.East)/2; var my = (start.North+end.North)/2;
                var ux = dx/len*sign*4/scale; var uy = dy/len*sign*4/scale;
                Element("polyline", ("points", $"{X(mx-ux-uy)},{Y(my-uy+ux)} {X(mx+ux)},{Y(my+uy)} {X(mx-ux+uy)},{Y(my-uy-ux)}"), ("fill", "none"), ("stroke", "#111"));
            }
        }
        foreach (var node in graph.Nodes)
        {
            Element("circle", ("cx", X(node.East)), ("cy", Y(node.North)), ("r", "3"), ("fill", node.Boundary ? "#db7a00" : node.Junction ? "#cc2222" : "#333"));
            Text(node.East, node.North + 4/scale, node.Id);
        }
        Text(b.MinEast, b.MinNorth - 20/scale, $"E [{b.MinEast:R}, {b.MaxEast:R}] N [{b.MinNorth:R}, {b.MaxNorth:R}]; no height data");
        Text(b.MinEast, b.MinNorth - 36/scale, $"{graph.Components.Length} undirected components; {graph.IsolatedSegments.Length} isolated segments; {graph.Diagnostics.Length} diagnostics; © OpenStreetMap contributors (ODbL)");
        w.WriteEndElement();
    }
}
