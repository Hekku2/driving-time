using System.Numerics;
using System.Security.Cryptography;
using System.Text.Json;
using SharpGLTF.Geometry;
using SharpGLTF.Geometry.VertexTypes;
using SharpGLTF.Materials;
using SharpGLTF.Scenes;

namespace DrivingTime.Pipeline;

public sealed record FixtureObject(string Id, EnginePoint Min, EnginePoint Max, float[] Color);
public sealed record FixtureAsset(string Path, string Sha256);
public sealed record FixtureManifest(int SchemaVersion, string SceneId, string Units,
    string CoordinateSystem, Origin Origin, string Axes, string VerticalDatum,
    Bounds Region, EnginePoint Min, EnginePoint Max, FixtureAsset[] Assets,
    FixtureObject[] Objects, Provenance Provenance);

/// <summary>Small authored fixture, not a general geometry converter.</summary>
public static class GlbFixture
{
    public static FixtureObject[] Objects { get; } =
    [
        new("Building10m", new(-5,0,-5), new(5,10,5), [0.9f,0.45f,0.12f,1]),
        new("East", new(8,0,-1), new(12,1,1), [1,0.1f,0.1f,1]),
        new("North", new(-1,0,-12), new(1,1,-8), [0.1f,0.8f,0.2f,1]),
        new("Up", new(-10,0,8), new(-9,6,9), [0.1f,0.3f,1,1]),
        new("Ground", new(-15,-0.2,-15), new(15,0,15), [0.25f,0.3f,0.35f,1])
    ];

    public static byte[] Generate(SceneConfiguration config)
    {
        // Define source corners in E,N,H then use the P07 transform exactly once.
        var frame = new CoordinateFrame(new(config.Origin.East, config.Origin.North, config.Origin.Height),
            VerticalDatum.Synthetic, config.TileSizeMetres);
        var scene = new SceneBuilder();
        foreach (var obj in Objects)
        {
            var mesh = new MeshBuilder<VertexPositionNormal>(obj.Id);
            var material = new MaterialBuilder(obj.Id + "Material").WithMetallicRoughnessShader()
                .WithChannelParam(KnownChannel.BaseColor, KnownProperty.RGBA, new Vector4(obj.Color[0],obj.Color[1],obj.Color[2],1))
                .WithChannelParam(KnownChannel.MetallicRoughness, KnownProperty.MetallicFactor, 0f)
                .WithChannelParam(KnownChannel.MetallicRoughness, KnownProperty.RoughnessFactor, 0.85f);
            var primitive = mesh.UsePrimitive(material);
            Vector3 Corner(int x, int y, int z)
            {
                var east = config.Origin.East + (x == 0 ? obj.Min.X : obj.Max.X);
                var north = config.Origin.North - (z == 0 ? obj.Min.Z : obj.Max.Z);
                var height = config.Origin.Height + (y == 0 ? obj.Min.Y : obj.Max.Y);
                if (east < config.Region.MinEast || east > config.Region.MaxEast ||
                    north < config.Region.MinNorth || north > config.Region.MaxNorth)
                    throw new ArgumentException("region must contain the synthetic fixture (15 m around origin).");
                var p = frame.ToEngine(new(east,north,height), VerticalDatum.Synthetic);
                return new((float)p.X,(float)p.Y,(float)p.Z);
            }
            void Face(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
            {
                // glTF front faces are counterclockwise; Godot's importer reverses indices.
                var normal = Vector3.Normalize(Vector3.Cross(b-a,c-a));
                var va = new VertexPositionNormal(a,normal);
                var vb = new VertexPositionNormal(b,normal);
                var vc = new VertexPositionNormal(c,normal);
                var vd = new VertexPositionNormal(d,normal);
                primitive.AddTriangle(va,vb,vc);
                primitive.AddTriangle(va,vc,vd);
            }
            Face(Corner(1,0,0),Corner(1,1,0),Corner(1,1,1),Corner(1,0,1));
            Face(Corner(0,0,1),Corner(0,1,1),Corner(0,1,0),Corner(0,0,0));
            Face(Corner(0,1,0),Corner(0,1,1),Corner(1,1,1),Corner(1,1,0));
            Face(Corner(0,0,1),Corner(0,0,0),Corner(1,0,0),Corner(1,0,1));
            Face(Corner(1,0,1),Corner(1,1,1),Corner(0,1,1),Corner(0,0,1));
            Face(Corner(0,0,0),Corner(0,1,0),Corner(1,1,0),Corner(1,0,0));
            scene.AddRigidMesh(mesh, new NodeBuilder(obj.Id));
        }
        return scene.ToGltf2().WriteGLB().ToArray();
    }

    public static void Write(SceneConfiguration config, string configurationHash, string stage)
    {
        stage = Path.GetFullPath(stage);
        var bytes = Generate(config); // Validate before touching prior staged files.
        var manifest = new FixtureManifest(1, config.SceneId, "metres", config.CoordinateSystem,
            config.Origin, "right-handed; X=east, Y=up, Z=south; already localized; node transforms identity",
            "synthetic; no geographic vertical datum", config.Region,
            new(-15,-0.2,-15), new(15,10,15),
            [new("synthetic.glb",Convert.ToHexStringLower(SHA256.HashData(bytes)))], Objects,
            new("Repository-authored synthetic fixture; no downloaded data",
                "Repository code terms; no third-party dataset license applies", null, configurationHash,
                "Synthetic height in metres", "No collision, real data, textures or streaming"));
        Directory.CreateDirectory(stage);
        File.WriteAllBytes(Path.Combine(stage,"synthetic.glb"),bytes);
        File.WriteAllText(Path.Combine(stage,"manifest.json"),JsonSerializer.Serialize(manifest,SceneConfiguration.JsonOptions)+"\n");
        Console.WriteLine($"Staged fixture v1: {stage} ({bytes.Length} GLB bytes)");
    }
}
