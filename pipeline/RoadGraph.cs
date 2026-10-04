using System.Globalization;
using System.Text.Json;

namespace DrivingTime.Pipeline;

public sealed record GraphPoint(double East, double North, long? OsmNodeId, double SourceIndex);
public sealed record GraphNode(string Id, long? OsmNodeId, double East, double North, bool Boundary, bool Junction);
public sealed record GraphSegment(string Id, long OsmWayId, string From, string To, string Direction,
    string DirectionBasis, GraphPoint[] Geometry, Dictionary<string, string> Tags);
public sealed record GraphArc(string SegmentId, string From, string To);
public sealed record GraphDiagnostic(string Code, long? OsmWayId, string Detail);
public sealed record ProjectedWay(RoadWay Source, GraphPoint[] Geometry);
public sealed record IntermediateRoadGraph(int SchemaVersion, string HorizontalCrs, string Units, string VerticalDatum,
    string DatumPolicy, Bounds ClipRegion, JsonElement SourceDocument, JsonElement Provenance,
    ProjectedWay[] SourceWays, GraphNode[] Nodes, GraphSegment[] Segments, GraphArc[] Arcs,
    string[][] Components, string[] IsolatedSegments, GraphDiagnostic[] Diagnostics);

public static class RoadGraph
{
    public static IntermediateRoadGraph Convert(RoadExtract extract, Bounds bounds, bool allowApproximateDatum,
        JsonElement sourceDocument, JsonElement provenance)
    {
        ArgumentNullException.ThrowIfNull(bounds);
        CoordinateFrame.Finite(bounds.MinEast, bounds.MinNorth, bounds.MaxEast, bounds.MaxNorth);
        if (bounds.MaxEast <= bounds.MinEast || bounds.MaxNorth <= bounds.MinNorth)
            throw new ArgumentException("Graph clipping bounds must have positive extents.");
        var projection = new Tm35FinProjection();
        var ways = extract.Ways.OrderBy(w => w.Id).Select(w => new ProjectedWay(w,
            w.Geometry.Select((p, i) => { var en = projection.FromWgs84(p, allowApproximateDatum); return new GraphPoint(en.East, en.North, w.Nodes[i], i); }).ToArray())).ToArray();
        var uses = ways.SelectMany(w => w.Source.Nodes.Select(n => (Node: n, Way: w.Source.Id)))
            .GroupBy(x => x.Node).ToDictionary(g => g.Key, g => g.ToArray());
        var splitNodes = uses.Where(kv => kv.Value.Length > 1).Select(kv => kv.Key).ToHashSet();
        var nodes = new Dictionary<string, GraphNode>();
        var segments = new List<GraphSegment>();
        var diagnostics = new List<GraphDiagnostic>();
        foreach (var use in uses.Where(kv => kv.Value.Select(x => x.Way).Distinct().Count() > 1))
        {
            var grades = ways.Where(w => use.Value.Any(u => u.Way == w.Source.Id))
                .Select(w => string.Join("/", new[] { "layer", "bridge", "tunnel" }.Select(k => w.Source.Tags.GetValueOrDefault(k, "<missing>")))).Distinct().ToArray();
            if (grades.Length > 1) diagnostics.Add(new("shared-node-grade-transition", null,
                $"OSM node {use.Key} has differing grade tags; explicit node connectivity retained, transition/elevation needs review."));
        }
        foreach (var way in ways)
        {
            var w = way.Source;
            var (direction, basis) = Direction(w.Tags);
            if (direction == "unknown") diagnostics.Add(new("unsupported-oneway", w.Id, "No arcs emitted for unrecognized/reversible/alternating direction."));
            if (basis == "default-two-way") diagnostics.Add(new("assumed-two-way", w.Id, "Missing oneway; ordinary-highway default, not physical validation."));
            foreach (var key in w.Tags.Keys.Where(k => k.Contains("conditional", StringComparison.Ordinal) || k.StartsWith("oneway:", StringComparison.Ordinal) || k.StartsWith("restriction", StringComparison.Ordinal)))
                diagnostics.Add(new("unsupported-restriction", w.Id, $"Preserved {key}; arcs do not resolve conditional, modal or turn restrictions."));
            if (w.Nodes.Distinct().Count() != w.Nodes.Length)
                diagnostics.Add(new("repeated-node", w.Id, "Repeated/closed-way nodes are split; no roundabout maneuver or turn model."));
            if (w.Tags.ContainsKey("bridge") || w.Tags.ContainsKey("tunnel") || w.Tags.ContainsKey("layer"))
                diagnostics.Add(new("grade-without-height", w.Id, "Grade tags preserved; no elevation inferred from layer/bridge/tunnel."));
            if (w.Tags.Keys.Any(k => k is "access" or "motor_vehicle" or "vehicle" or "construction") || w.Tags["highway"] is "construction" or "proposed")
                diagnostics.Add(new("access-unresolved", w.Id, "Access/construction preserved; arcs indicate base direction only, not permission or suitability."));
            var part = new List<GraphPoint>();
            var number = 0;
            void Flush()
            {
                if (part.Count < 2) { part.Clear(); return; }
                var id = $"way/{w.Id}/segment/{number++}";
                string Endpoint(GraphPoint p)
                {
                    var boundary = OnBoundary(p, bounds);
                    var nodeId = p.OsmNodeId is { } osm ? $"osm/{osm}" : $"cut/{w.Id}/{p.SourceIndex.ToString("R", CultureInfo.InvariantCulture)}";
                    nodes.TryAdd(nodeId, new(nodeId, p.OsmNodeId, p.East, p.North, boundary,
                        !boundary && p.OsmNodeId is { } n && uses[n].Select(x => x.Way).Distinct().Count() > 1));
                    return nodeId;
                }
                segments.Add(new(id, w.Id, Endpoint(part[0]), Endpoint(part[^1]), direction, basis,
                    part.ToArray(), w.Tags.OrderBy(k => k.Key, StringComparer.Ordinal).ToDictionary(k => k.Key, k => k.Value)));
                part.Clear();
            }
            for (var i = 0; i < way.Geometry.Length - 1; i++)
            {
                var a = way.Geometry[i]; var b = way.Geometry[i + 1];
                if (double.Hypot(b.East - a.East, b.North - a.North) < 1e-6)
                { diagnostics.Add(new("degenerate-edge", w.Id, $"Zero/near-zero edge at source index {i}; excluded and continuity broken.")); Flush(); continue; }
                if (!Clip(a, b, bounds, out var start, out var end)) { Flush(); continue; }
                if (part.Count > 0 && part[^1].SourceIndex != start.SourceIndex) Flush();
                if (part.Count == 0) part.Add(start);
                part.Add(end);
                if (end.OsmNodeId == null || (end.OsmNodeId is { } n && splitNodes.Contains(n))) Flush();
            }
            Flush();
            if (!segments.Any(s => s.OsmWayId == w.Id)) diagnostics.Add(new("outside-or-degenerate", w.Id, "No nonzero geometry retained inside clip bounds."));
        }
        var arcs = segments.SelectMany(s => s.Direction switch
        {
            "forward" => new[] { new GraphArc(s.Id, s.From, s.To) },
            "reverse" => new[] { new GraphArc(s.Id, s.To, s.From) },
            "both" => new[] { new GraphArc(s.Id, s.From, s.To), new GraphArc(s.Id, s.To, s.From) },
            _ => Array.Empty<GraphArc>()
        }).ToArray();
        var incident = nodes.Keys.ToDictionary(n => n, n => new List<GraphSegment>());
        foreach (var s in segments) { incident[s.From].Add(s); if (s.To != s.From) incident[s.To].Add(s); }
        var visited = new HashSet<string>(); var components = new List<string[]>();
        foreach (var segment in segments)
        {
            if (!visited.Add(segment.Id)) continue;
            var component = new List<string>(); var queue = new Queue<GraphSegment>(); queue.Enqueue(segment);
            while (queue.TryDequeue(out var current))
            {
                component.Add(current.Id);
                foreach (var next in incident[current.From].Concat(incident[current.To])) if (visited.Add(next.Id)) queue.Enqueue(next);
            }
            components.Add(component.Order(StringComparer.Ordinal).ToArray());
        }
        diagnostics.Add(new("sample-context", null, "Way-only sample: node tags, restriction relations and neighboring streets unavailable; geometric crossings are never welded. Components are undirected, not routability."));
        return new(1, "EPSG:3067", "metre", "none; horizontal graph only", "WGS84 angles treated as EUREF-FIN (P07 approximation)",
            bounds, sourceDocument.Clone(), provenance.Clone(), ways, nodes.Values.OrderBy(n => n.Id, StringComparer.Ordinal).ToArray(),
            segments.ToArray(), arcs, components.ToArray(), components.Where(c => c.Length == 1).Select(c => c[0]).ToArray(), diagnostics.ToArray());
    }

    private static (string, string) Direction(Dictionary<string, string> tags)
    {
        if (tags.TryGetValue("oneway", out var value)) return value switch
        {
            "yes" or "true" or "1" => ("forward", "explicit-oneway"),
            "-1" => ("reverse", "explicit-oneway"),
            "no" or "false" or "0" => ("both", "explicit-oneway"),
            _ => ("unknown", "unsupported-oneway")
        };
        if (tags.GetValueOrDefault("junction") == "roundabout" || tags.GetValueOrDefault("highway") == "motorway")
            return ("forward", "implied-oneway");
        return ("both", "default-two-way");
    }
    private static bool OnBoundary(GraphPoint p, Bounds b) => Math.Abs(p.East - b.MinEast) < 1e-7 || Math.Abs(p.East - b.MaxEast) < 1e-7 || Math.Abs(p.North - b.MinNorth) < 1e-7 || Math.Abs(p.North - b.MaxNorth) < 1e-7;
    private static bool Clip(GraphPoint a, GraphPoint b, Bounds bounds, out GraphPoint start, out GraphPoint end)
    {
        var lo = 0d; var hi = 1d;
        var dx = b.East - a.East; var dy = b.North - a.North;
        bool Test(double p, double q)
        {
            if (p == 0) return q >= 0;
            var r = q / p;
            if (p < 0) lo = Math.Max(lo, r); else hi = Math.Min(hi, r);
            return lo < hi;
        }
        GraphPoint At(double t) => t == 0 ? a : t == 1 ? b : new(a.East + t * dx, a.North + t * dy, null, a.SourceIndex + t);
        var retained = Test(-dx, a.East - bounds.MinEast) && Test(dx, bounds.MaxEast - a.East) &&
            Test(-dy, a.North - bounds.MinNorth) && Test(dy, bounds.MaxNorth - a.North) && (hi - lo) * double.Hypot(dx, dy) >= 1e-6;
        start = At(lo); end = At(hi); return retained;
    }
}
