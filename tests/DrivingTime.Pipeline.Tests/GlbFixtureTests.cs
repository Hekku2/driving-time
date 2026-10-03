using System.Numerics;
using System.Security.Cryptography;
using System.Text.Json;
using DrivingTime.Pipeline;
using SharpGLTF.Schema2;
using Xunit;

namespace DrivingTime.Pipeline.Tests;

public sealed class GlbFixtureTests
{
    private static SceneConfiguration Config => SceneConfiguration.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"synthetic-scene.json")));
    [Fact]
    public void SerializedGlbHasValidOutwardTrianglesMaterialsBoundsAndIdentityTransforms()
    {
        var bytes = GlbFixture.Generate(Config);
        Assert.True(bytes.Length < 20000);
        Assert.Equal(bytes,GlbFixture.Generate(Config));
        using var stream = new MemoryStream(bytes);
        var model = ModelRoot.ReadGLB(stream);
        Assert.Equal(5,model.LogicalMeshes.Count);
        Assert.Empty(model.LogicalImages);
        foreach (var node in model.LogicalNodes)
        {
            Assert.Equal(Matrix4x4.Identity,node.LocalMatrix);
            var expected = GlbFixture.Objects.Single(o => o.Id == node.Name);
            var prim = Assert.Single(node.Mesh.Primitives);
            Assert.False(prim.Material.DoubleSided);
            var rgba = prim.Material.FindChannel("BaseColor")!.Value.Color;
            Assert.Equal(new Vector4(expected.Color[0],expected.Color[1],expected.Color[2],1),rgba);
            var p = prim.GetVertexAccessor("POSITION").AsVector3Array();
            var n = prim.GetVertexAccessor("NORMAL").AsVector3Array();
            var min = p.Aggregate(Vector3.Min); var max = p.Aggregate(Vector3.Max);
            Assert.Equal(new Vector3((float)expected.Min.X,(float)expected.Min.Y,(float)expected.Min.Z),min);
            Assert.Equal(new Vector3((float)expected.Max.X,(float)expected.Max.Y,(float)expected.Max.Z),max);
            Assert.Equal(12,prim.GetTriangleIndices().Count());
            foreach (var (a,b,c) in prim.GetTriangleIndices())
            {
                Assert.True(Vector3.Dot(Vector3.Normalize(Vector3.Cross(p[b]-p[a],p[c]-p[a])),n[a]) > 0.999f);
                Assert.InRange(n[a].Length(),0.99999f,1.00001f);
                Assert.True(Vector3.Dot(n[a],p[a]-(min+max)/2)>0);
                Assert.Equal(n[a],n[b]); Assert.Equal(n[a],n[c]);
            }
        }
    }
    [Fact]
    public void NonzeroSourceOriginIsLocalizedOnceAndManifestMatchesStagedBytes()
    {
        var config = Config with { Origin = new Origin { East=1000,North=2000,Height=40 },
            Region = new Bounds { MinEast=970,MaxEast=1030,MinNorth=1970,MaxNorth=2030 } };
        Assert.Equal(GlbFixture.Generate(Config),GlbFixture.Generate(config));
        var directory = Path.Combine(Path.GetTempPath(),"p08-"+Guid.NewGuid());
        try
        {
            GlbFixture.Write(config,"config-hash",directory);
            using var json = JsonDocument.Parse(File.ReadAllText(Path.Combine(directory,"manifest.json")));
            var root = json.RootElement;
            Assert.Equal(1,root.GetProperty("schemaVersion").GetInt32());
            Assert.Equal(2000,root.GetProperty("origin").GetProperty("north").GetDouble());
            Assert.Equal("metres",root.GetProperty("units").GetString());
            Assert.Equal(-15,root.GetProperty("min").GetProperty("z").GetDouble());
            Assert.Equal(10,root.GetProperty("max").GetProperty("y").GetDouble());
            var asset = root.GetProperty("assets")[0];
            Assert.Equal("synthetic.glb",asset.GetProperty("path").GetString());
            Assert.Equal(Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(Path.Combine(directory,"synthetic.glb")))),asset.GetProperty("sha256").GetString());
            Assert.Throws<ArgumentException>(() => GlbFixture.Generate(Config with {Region = new Bounds {MinEast=-1,MaxEast=1,MinNorth=-1,MaxNorth=1}}));
        }
        finally { if (Directory.Exists(directory)) Directory.Delete(directory,true); }
    }
}
