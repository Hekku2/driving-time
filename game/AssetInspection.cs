using Godot;
using System;
using System.Linq;
using System.Text.Json;

public partial class AssetInspection : Node3D
{
    private Node3D _asset = null!;
    private Camera3D _camera = null!;
    private float _angle = 0.65f;
    public override void _Ready()
    {
        try
        {
            using var file = Godot.FileAccess.Open("res://Generated/P08/manifest.json", Godot.FileAccess.ModeFlags.Read)
                ?? throw new Exception("Generate P08 fixture first; see docs/PIPELINE.md.");
            using var doc = JsonDocument.Parse(file.GetAsText());
            var root = doc.RootElement;
            Require(root.GetProperty("schemaVersion").GetInt32() == 1 && root.GetProperty("units").GetString() == "metres", "manifest version/units");
            var path = root.GetProperty("assets")[0].GetProperty("path").GetString();
            Require(path == "synthetic.glb", "fixture asset path");
            _asset = GD.Load<PackedScene>("res://Generated/P08/" + path).Instantiate<Node3D>();
            AddChild(_asset);
            var meshes = Descendants(_asset).OfType<MeshInstance3D>().ToArray();
            Require(meshes.Length == 5, "five imported meshes");
            foreach (var obj in root.GetProperty("objects").EnumerateArray())
            {
                var name = obj.GetProperty("id").GetString();
                var mesh = meshes.Single(m => m.Name.ToString() == name);
                Require(mesh.GlobalTransform.IsEqualApprox(Transform3D.Identity), name + " identity transform");
                var aabb = mesh.Mesh.GetAabb();
                Require(aabb.Position.IsEqualApprox(Point(obj.GetProperty("min"))) &&
                    aabb.End.IsEqualApprox(Point(obj.GetProperty("max"))), name + " bounds/axis/scale");
                Require(mesh.Mesh.GetSurfaceCount() == 1, name + " one surface");
                var material = mesh.Mesh.SurfaceGetMaterial(0) as StandardMaterial3D;
                Require(material != null && material.CullMode == BaseMaterial3D.CullModeEnum.Back, name + " single-sided material");
                var color = obj.GetProperty("color");
                // glTF factors are linear; Godot stores the imported albedo in sRGB.
                var expectedColor = new Color(color[0].GetSingle(),color[1].GetSingle(),color[2].GetSingle()).LinearToSrgb();
                Require(Mathf.Abs(material!.AlbedoColor.R-expectedColor.R) < 0.001f &&
                    Mathf.Abs(material.AlbedoColor.G-expectedColor.G) < 0.001f &&
                    Mathf.Abs(material.AlbedoColor.B-expectedColor.B) < 0.001f &&
                    Mathf.Abs(material.Metallic) < 0.001f && Mathf.Abs(material.Roughness-0.85f) < 0.001f, name + " material parameters");
                var arrays = mesh.Mesh.SurfaceGetArrays(0);
                var positions = arrays[(int)Mesh.ArrayType.Vertex].AsVector3Array();
                var normals = arrays[(int)Mesh.ArrayType.Normal].AsVector3Array();
                var indices = arrays[(int)Mesh.ArrayType.Index].AsInt32Array();
                Require(indices.Length == 36, name + " 12 triangles");
                for (var i = 0; i < indices.Length; i += 3)
                {
                    var a = indices[i]; var b = indices[i+1]; var c = indices[i+2];
                    var cross = (positions[b]-positions[a]).Cross(positions[c]-positions[a]).Normalized();
                    Require(cross.Dot(normals[a]) < -0.999f, name + " Godot clockwise winding");
                    Require(Mathf.Abs(normals[a].Length()-1) < 0.0001f &&
                        normals[a].Dot(positions[a]-aabb.GetCenter()) > 0 &&
                        normals[a].IsEqualApprox(normals[b]) && normals[a].IsEqualApprox(normals[c]), name + " outward flat unit normals");
                }
            }
            GD.Print("P08 PASS: manifest and 5 imported meshes; bounds, identity transforms, materials, normals, clockwise winding.");
            if (OS.GetCmdlineUserArgs().Contains("--check-fixture")) { GetTree().Quit(); return; }
            _camera = new Camera3D { Current = true, Far = 200 };
            AddChild(_camera);
            var light = new DirectionalLight3D { RotationDegrees = new Vector3(-55,-30,0), LightEnergy = 1.2f };
            AddChild(light);
            foreach (var (text, position) in new[] { ("10 m cube",new Vector3(0,11,0)), ("E +X (red)",new Vector3(10,2,0)),
                ("N -Z (green)",new Vector3(0,2,-10)), ("UP +Y (blue)",new Vector3(-9.5f,7,8.5f)) })
                AddChild(new Label3D { Text = text, Position = position, Billboard = BaseMaterial3D.BillboardModeEnum.Enabled, FontSize = 48 });
            var canvas = new CanvasLayer(); AddChild(canvas);
            canvas.AddChild(new Label { Text = "P08 synthetic asset inspection | Left/Right: orbit\nOrange: 10 m cube | Red: east +X | Green: north -Z | Blue: up +Y", Position = new Vector2(20,20) });
        }
        catch (Exception error) { GD.PushError("P08 FAIL: " + error.Message); GetTree().Quit(1); }
    }
    public override void _Process(double delta)
    {
        if (_camera == null) return;
        _angle += (Input.GetActionStrength("ui_right")-Input.GetActionStrength("ui_left"))*(float)delta;
        _camera.Position = new Vector3(38*Mathf.Sin(_angle),27,38*Mathf.Cos(_angle));
        _camera.LookAt(new Vector3(0,3,0));
    }
    private static Vector3 Point(JsonElement p) => new(p.GetProperty("x").GetSingle(),p.GetProperty("y").GetSingle(),p.GetProperty("z").GetSingle());
    private static void Require(bool condition, string message) { if (!condition) throw new Exception(message); }
    private static System.Collections.Generic.IEnumerable<Node> Descendants(Node node)
    {
        foreach (var child in node.GetChildren()) { yield return child; foreach (var descendant in Descendants(child)) yield return descendant; }
    }
}
