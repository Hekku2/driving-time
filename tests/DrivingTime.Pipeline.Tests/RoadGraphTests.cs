using System.Text.Json;
using System.Xml.Linq;
using DrivingTime.Pipeline;
using Xunit;

namespace DrivingTime.Pipeline.Tests;

public sealed class RoadGraphTests
{
    private static readonly Bounds Region = new() { MinEast=384300, MinNorth=6674100, MaxEast=384500, MaxNorth=6674300 };
    private static RoadWay Way(long id, long[] nodes, (double E, double N)[] points, params (string Key, string Value)[] tags)
    {
        var projection = new Tm35FinProjection();
        var attributes = new Dictionary<string, string> { ["highway"] = "residential", ["name"] = "Authored fixture" };
        foreach (var t in tags) attributes[t.Key] = t.Value;
        return new(id, "2026-10-04T00:00:00Z", nodes, points.Select(p => projection.ToEurefFin(p.E, p.N)).ToArray(), attributes);
    }
    private static IntermediateRoadGraph Graph(params RoadWay[] ways)
    {
        using var source = JsonDocument.Parse("{}");
        return RoadGraph.Convert(new("fixture", "2026-10-04T00:00:00Z", ways), Region, true, source.RootElement, source.RootElement);
    }
    [Fact]
    public void TwoWayPreservesIntermediateGeometryTagsAndIds()
    {
        var way = Way(10, [1,2,3], [(384350,6674200),(384400,6674210),(384450,6674200)], ("oneway","no"), ("lanes","2"), ("maxspeed","30"));
        var g = Graph(way); var s = Assert.Single(g.Segments);
        Assert.Equal("both", s.Direction); Assert.Equal(3, s.Geometry.Length); Assert.Equal(new long?[] {1,2,3}, s.Geometry.Select(p=>p.OsmNodeId));
        Assert.Equal(way.Tags, s.Tags); Assert.Equal(10, s.OsmWayId); Assert.Equal(2,g.Nodes.Length);
        Assert.Contains(g.Arcs,a=>a.From=="osm/1" && a.To=="osm/3"); Assert.Contains(g.Arcs,a=>a.From=="osm/3" && a.To=="osm/1");
        Assert.InRange(Math.Abs(s.Geometry[1].East-384400),0,1e-5);
        Assert.Equal(way,g.SourceWays[0].Source); Assert.Single(g.IsolatedSegments);
    }
    [Theory]
    [InlineData("-1","reverse","osm/2","osm/1")]
    [InlineData("yes","forward","osm/1","osm/2")]
    public void OneWayArcsRespectOriginalOrder(string tag,string direction,string from,string to)
    {
        var g=Graph(Way(10,[1,2],[(384350,6674200),(384450,6674200)],("oneway",tag)));
        Assert.Equal(direction,Assert.Single(g.Segments).Direction);
        var arc=Assert.Single(g.Arcs); Assert.Equal(from,arc.From); Assert.Equal(to,arc.To);
        Assert.Equal(1,g.Segments[0].Geometry[0].OsmNodeId);
    }
    [Fact]
    public void SharedIntersectionSplitsBothWaysAndConnectsFourArms()
    {
        var g=Graph(Way(10,[1,2,3],[(384350,6674200),(384400,6674200),(384450,6674200)]),
            Way(20,[4,2,5],[(384400,6674150),(384400,6674200),(384400,6674250)]));
        Assert.Equal(4,g.Segments.Length); Assert.Equal(8,g.Arcs.Length); Assert.Single(g.Components);
        Assert.True(g.Nodes.Single(n=>n.OsmNodeId==2).Junction);
        Assert.Equal(4,g.Segments.Count(s=>s.From=="osm/2" || s.To=="osm/2")); Assert.Empty(g.IsolatedSegments);
    }
    [Fact]
    public void CrossingAtDifferentLevelsNeverCreatesIntersection()
    {
        var g=Graph(Way(10,[1,2],[(384350,6674200),(384450,6674200)],("layer","0")),
            Way(20,[3,4],[(384400,6674150),(384400,6674250)],("layer","1"),("bridge","yes")));
        Assert.Equal(2,g.Components.Length); Assert.Equal(4,g.Nodes.Length); Assert.All(g.Nodes,n=>Assert.False(n.Junction));
        Assert.Equal("yes",g.Segments.Single(s=>s.OsmWayId==20).Tags["bridge"]);
        Assert.Equal(2,g.IsolatedSegments.Length); Assert.Contains(g.Diagnostics,d=>d.Code=="grade-without-height");
    }
    [Fact]
    public void CoincidentCoordinatesWithDistinctOsmIdsRemainSeparate()
    {
        var g=Graph(Way(10,[1,2],[(384350,6674200),(384400,6674200)]),Way(20,[3,4],[(384400,6674200),(384450,6674200)]));
        Assert.Equal(2,g.Components.Length); Assert.All(g.Nodes,n=>Assert.False(n.Junction));
    }
    [Fact]
    public void ClipsOutsideToOutsideAndKeepsCutsDistinctAcrossWays()
    {
        var g=Graph(Way(10,[1,2],[(384250,6674200),(384550,6674200)]),Way(20,[3,4],[(384250,6674200),(384550,6674200)]));
        Assert.Equal(2,g.Segments.Length); Assert.Equal(4,g.Nodes.Length); Assert.Equal(2,g.Components.Length);
        Assert.All(g.Nodes,n=>{Assert.True(n.Boundary);Assert.False(n.Junction);Assert.Null(n.OsmNodeId);});
        Assert.All(g.Segments,s=>{ Assert.InRange(Math.Abs(s.Geometry[0].East-384300),0,1e-7); Assert.InRange(Math.Abs(s.Geometry[^1].East-384500),0,1e-7); });
        Assert.Equal(2,g.SourceWays[0].Geometry.Length); Assert.Equal(1,g.SourceWays[0].Geometry[0].OsmNodeId);
    }
    [Fact]
    public void SharedOriginalNodeOnBoundaryRetainsIdWithoutIntersectionFlag()
    {
        var first=Way(10,[1,2],[(384350,6674200),(384400,6674200)]);
        var second=Way(20,[2,3],[(384400,6674200),(384450,6674200)]);
        var actual=new Tm35FinProjection().FromWgs84(first.Geometry[1],true);
        using var doc=JsonDocument.Parse("{}");
        var graph=RoadGraph.Convert(new("fixture","2026-10-04T00:00:00Z",[first,second]),
            Region with { MaxEast=actual.East },true,doc.RootElement,doc.RootElement);
        var node=graph.Nodes.Single(n=>n.OsmNodeId==2);
        Assert.True(node.Boundary); Assert.False(node.Junction); Assert.Equal("osm/2",node.Id);
        Assert.Single(graph.Segments);
    }
    [Fact]
    public void ImpliedOneWayAndExplicitOverrideAreRecorded()
    {
        var implied=Graph(Way(10,[1,2],[(384350,6674200),(384450,6674200)],("junction","roundabout")));
        Assert.Equal("implied-oneway",Assert.Single(implied.Segments).DirectionBasis); Assert.Single(implied.Arcs);
        var overridden=Graph(Way(10,[1,2],[(384350,6674200),(384450,6674200)],("junction","roundabout"),("oneway","no")));
        Assert.Equal(2,overridden.Arcs.Length);
    }
    [Fact]
    public void ExitAndReentryCreateDisconnectedPieces()
    {
        var g=Graph(Way(10,[1,2,3,4],[(384350,6674150),(384550,6674150),(384550,6674250),(384350,6674250)]));
        Assert.Equal(2,g.Segments.Length); Assert.Equal(2,g.Components.Length);
        Assert.Equal(2,g.Nodes.Count(n=>n.Boundary));
    }
    [Fact]
    public void UnsupportedDirectionsHaveNoArcsAndConditionalTagsAreReported()
    {
        var g=Graph(Way(10,[1,2],[(384350,6674200),(384450,6674200)],("oneway","reversible"),("oneway:conditional","yes @ (08:00-10:00)"),("tunnel","yes")));
        Assert.Empty(g.Arcs); Assert.Equal("unknown",Assert.Single(g.Segments).Direction);
        Assert.Contains(g.Diagnostics,d=>d.Code=="unsupported-oneway"); Assert.Contains(g.Diagnostics,d=>d.Code=="unsupported-restriction");
        Assert.Equal("yes",g.Segments[0].Tags["tunnel"]);
    }
    [Fact]
    public void SharedGradeTransitionRetainsExplicitConnectivityAndWarns()
    {
        var g=Graph(Way(10,[1,2],[(384350,6674200),(384400,6674200)],("layer","0")),
            Way(20,[2,3],[(384400,6674200),(384450,6674200)],("layer","1"),("bridge","yes")));
        Assert.Single(g.Components); Assert.Contains(g.Diagnostics,d=>d.Code=="shared-node-grade-transition");
    }
    [Fact]
    public void ApproximateDatumRequiresOptInAndInvalidBoundsFail()
    {
        using var doc=JsonDocument.Parse("{}");
        var extract=new RoadExtract("fixture","2026-10-04T00:00:00Z",[Way(10,[1,2],[(384350,6674200),(384450,6674200)])]);
        Assert.Throws<ArgumentException>(()=>RoadGraph.Convert(extract,Region,false,doc.RootElement,doc.RootElement));
        Assert.Throws<ArgumentException>(()=>RoadGraph.Convert(extract,new Bounds { MinEast=0, MinNorth=0, MaxEast=0, MaxNorth=1 },true,doc.RootElement,doc.RootElement));
    }
    [Fact]
    public void RepeatedAndDegenerateNodesAreReported()
    {
        var g=Graph(Way(10,[1,2,2,3],[(384350,6674200),(384400,6674200),(384400,6674200),(384450,6674200)]));
        Assert.Contains(g.Diagnostics,d=>d.Code=="degenerate-edge"); Assert.Contains(g.Diagnostics,d=>d.Code=="repeated-node");
        Assert.Equal(2,g.Segments.Length);
    }
    [Fact]
    public void OfflineCommandPreservesSourceMetadataAndRejectsTamperedInput()
    {
        var root=Path.Combine(Path.GetTempPath(),"p11-"+Guid.NewGuid().ToString("N")); Directory.CreateDirectory(root);
        try
        {
            var bytes=File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory,"osm-road-sample.json")); var extract=RoadSample.Parse(bytes);
            File.WriteAllBytes(Path.Combine(root,"raw.json"),bytes);
            var p=new { schemaVersion=1,payloadSha256=RoadSample.Hash(bytes),byteCount=bytes.Length,license="fixture",attribution="authored",horizontalCrs="EPSG:4326",verticalDatum="none",importedOrRetrievedUtc="2026-10-04T00:00:00Z",source="fixture",osmBase=extract.OsmBase,generator=extract.Generator };
            File.WriteAllText(Path.Combine(root,"provenance.json"),JsonSerializer.Serialize(p));
            var config=new RoadGraphConfiguration(1,"raw.json","provenance.json","graph.json","graph.svg",new Bounds { MinEast=383736, MinNorth=6673710, MaxEast=384736, MaxNorth=6674710 },true);
            var configPath=Path.Combine(root,"config.json"); File.WriteAllText(configPath,JsonSerializer.Serialize(config,RoadSample.Json));
            Assert.Equal(0,RoadGraphCommand.Run(["--config",configPath]));
            var output=File.ReadAllBytes(Path.Combine(root,"graph.json"));
            Assert.Equal(0,RoadGraphCommand.Run(["--config",configPath])); Assert.Equal(output,File.ReadAllBytes(Path.Combine(root,"graph.json")));
            using var graph=JsonDocument.Parse(output); using var raw=JsonDocument.Parse(bytes);
            Assert.True(JsonElement.DeepEquals(raw.RootElement.GetProperty("elements"),graph.RootElement.GetProperty("sourceDocument").GetProperty("elements")));
            Assert.Equal(p.payloadSha256,graph.RootElement.GetProperty("provenance").GetProperty("payloadSha256").GetString());
            Assert.Equal("svg",XDocument.Load(Path.Combine(root,"graph.svg")).Root!.Name.LocalName);
            File.AppendAllText(Path.Combine(root,"raw.json")," "); Assert.Equal(2,RoadGraphCommand.Run(["--config",configPath]));
        }
        finally { Directory.Delete(root,true); }
    }
}
