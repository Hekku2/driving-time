using System.Numerics;
using System.Text.Json;
using DrivingTime.Pipeline;
using SharpGLTF.Schema2;
using Xunit;

namespace DrivingTime.Pipeline.Tests;

public sealed class RoadStripTests
{
    private static RoadStripConfiguration Config => new(1,"graph.json","stage","s",1,18,
        new Origin{East=1000,North=2000,Height=13},new Bounds{MinEast=900,MaxEast=1100,MinNorth=1900,MaxNorth=2100},0,
        new(){{"residential",6}},4,0.01,2);
    private static GraphSegment Segment(params (double X,double Z)[] points)=>new("s",1,"a","b","both","default",
        points.Select((p,i)=>new GraphPoint(1000+p.X,2000-p.Z,i,i)).ToArray(),new(){{"highway","residential"}});
    private static StripMesh Straight()=>RoadStrip.Generate(Segment((0,0),(20,0)),Config);
    private static void Valid(StripMesh mesh)
    {
        foreach(var v in mesh.Vertices){Assert.True(float.IsFinite(v.X)&&float.IsFinite(v.Y)&&float.IsFinite(v.Z));Assert.Equal(0,v.Y);}
        for(int i=0;i<mesh.Indices.Length;i+=3)
            Assert.True(Vector3.Cross(mesh.Vertices[mesh.Indices[i+1]]-mesh.Vertices[mesh.Indices[i]],mesh.Vertices[mesh.Indices[i+2]]-mesh.Vertices[mesh.Indices[i]]).Y>1e-8f);
    }
    [Fact] public void StraightBoundsWidthAndOriginAreIndependent()
    {
        var m=Straight();Valid(m);Assert.Equal(new Vector3(1,0,-3),m.Vertices.Aggregate(Vector3.Min));Assert.Equal(new Vector3(19,0,3),m.Vertices.Aggregate(Vector3.Max));
        Assert.Equal(6,m.WidthMetres);Assert.Contains("fallback",m.WidthBasis);
        var translated=RoadStrip.Generate(Segment((0,0),(20,0)),Config with{Origin=Config.Origin with{Height=999}});
        Assert.Equal(m.Vertices,translated.Vertices);
    }
    [Fact] public void DuplicateAndShortPointsDoNotChangeMesh()
    {
        var m=RoadStrip.Generate(Segment((0,0),(0,0),(0.001,0),(20,0)),Config);Valid(m);
        Assert.Equal(Straight().Vertices,m.Vertices);Assert.Contains(m.Assumptions,s=>s.Contains("Removed 2"));
    }
    [Theory]
    [InlineData(10,10)] [InlineData(1,1)] [InlineData(0.2,0.2)] [InlineData(-9,1)]
    public void BendsAndExtremeJoinsStayBounded(double x,double z)
    {
        var m=RoadStrip.Generate(Segment((0,0),(10,0),(10+x,z)),Config with{LengthMetres=9+double.Hypot(x,z)-1});Valid(m);
        var join=m.Vertices[2];Assert.True(Vector3.Distance(join,new Vector3(10,0,0))<=6.001);
        if(x<0)Assert.Contains(m.Assumptions,s=>s.Contains("Capped 1"));
    }
    [Theory] [InlineData("8.8",8.8)] [InlineData("8 m",6)] [InlineData("0",6)] [InlineData("200",6)]
    public void WidthTagsAreConservativeAndReported(string tag,double expected)
    {
        var s=Segment((0,0),(20,0));s.Tags["width"]=tag;var m=RoadStrip.Generate(s,Config);Assert.Equal(expected,m.WidthMetres);
        if(expected==6)Assert.Contains(m.Assumptions,s=>s.Contains("Unsupported width"));
    }
    [Fact] public void RejectsOutsideBoundsInvalidSettingsGradeAndReversal()
    {
        Assert.Throws<ArgumentException>(()=>RoadStrip.Generate(Segment((0,0),(20,0)),Config with{StartMetres=0}));
        Assert.Throws<ArgumentException>(()=>RoadStrip.Generate(Segment((0,0),(20,0)),Config with{LengthMetres=20}));
        Assert.Throws<ArgumentException>(()=>RoadStrip.Generate(Segment((0,0),(20,0)),Config with{Region=Config.Region with{MaxEast=1005}}));
        var s=Segment((0,0),(20,0));s.Tags["bridge"]="yes";Assert.Throws<ArgumentException>(()=>RoadStrip.Generate(s,Config));
        Assert.Throws<ArgumentException>(()=>RoadStrip.Generate(Segment((0,0),(10,0),(0,0)),Config));
        Assert.Throws<ArgumentException>(()=>RoadStrip.Generate(Segment((0,0),(0.001,0)),Config));
    }
    [Fact] public void BothSerializedAssetsHaveSameValidGeometryAndIdentityTransforms()
    {
        var strip=RoadStrip.Generate(Segment((0,0),(10,0),(20,4)),Config);Valid(strip);
        var positions=new List<Vector3[]>();
        foreach(var collision in new[]{false,true})
        {
            var bytes=RoadStrip.Glb(strip,collision);Assert.Equal(bytes,RoadStrip.Glb(strip,collision));
            var model=ModelRoot.ReadGLB(new MemoryStream(bytes));var node=Assert.Single(model.LogicalNodes);Assert.Equal(Matrix4x4.Identity,node.LocalMatrix);
            Assert.Equal(collision ? "RoadCollision-colonly" : "RoadAsphalt",node.Name);
            var primitive=Assert.Single(Assert.Single(model.LogicalMeshes).Primitives);
            var p=primitive.GetVertexAccessor("POSITION").AsVector3Array();positions.Add(p.ToArray());
            var normals=primitive.GetVertexAccessor("NORMAL").AsVector3Array();Assert.All(normals,n=>Assert.Equal(Vector3.UnitY,n));
            Assert.Equal(strip.Indices.Length/3,primitive.GetTriangleIndices().Count());
            foreach(var(a,b,c) in primitive.GetTriangleIndices())Assert.True(Vector3.Cross(p[b]-p[a],p[c]-p[a]).Y>1e-8f);
            Assert.Equal(strip.Vertices.Aggregate(Vector3.Min),p.Aggregate(Vector3.Min));Assert.Equal(strip.Vertices.Aggregate(Vector3.Max),p.Aggregate(Vector3.Max));
        }
        Assert.Equal(positions[0],positions[1]);
    }
    [Fact] public void OfflineCommandWritesDeterministicManifestWithChecksumsAndSeparateCollision()
    {
        var dir=Path.Combine(Path.GetTempPath(),"p12-"+Guid.NewGuid());Directory.CreateDirectory(dir);
        try {
            using var p=JsonDocument.Parse("{\"license\":\"synthetic fixture\"}");
            var graph=new IntermediateRoadGraph(1,"EPSG:3067","metre","none","test",Config.Region,p.RootElement,p.RootElement,[],[],[Segment((0,0),(10,0),(20,4))],[],[],[],[]);
            File.WriteAllText(Path.Combine(dir,"graph.json"),JsonSerializer.Serialize(graph,RoadSample.Json));
            var configPath=Path.Combine(dir,"config.json");File.WriteAllText(configPath,JsonSerializer.Serialize(Config,RoadSample.Json));
            Assert.Equal(0,RoadStripCommand.Run(["--config",configPath]));
            var paths=new[]{"road.glb","collision.glb","manifest.json"}.Select(s=>Path.Combine(dir,"stage",s)).ToArray();var first=paths.Select(File.ReadAllBytes).ToArray();
            Assert.Equal(0,RoadStripCommand.Run(["--config",configPath]));for(var i=0;i<3;i++)Assert.Equal(first[i],File.ReadAllBytes(paths[i]));
            using var manifest=JsonDocument.Parse(first[2]);var root=manifest.RootElement;
            Assert.Equal(2,root.GetProperty("assets").GetArrayLength());Assert.Equal(RoadSample.Hash(first[0]),root.GetProperty("assets")[0].GetProperty("sha256").GetString());
            Assert.Equal(RoadSample.Hash(first[1]),root.GetProperty("assets")[1].GetProperty("sha256").GetString());Assert.Equal("synthetic flat height; not N2000 road elevation",root.GetProperty("verticalDatum").GetString());
            File.WriteAllText(configPath,JsonSerializer.Serialize(Config with{SegmentId="missing"},RoadSample.Json));Assert.Equal(2,RoadStripCommand.Run(["--config",configPath]));Assert.Equal(first[0],File.ReadAllBytes(paths[0]));
        } finally{Directory.Delete(dir,true);}
    }
}
