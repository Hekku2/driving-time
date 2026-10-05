using Godot;
using System;
using System.Linq;
using System.Text.Json;
using DrivingTime.Pipeline;

// Reuse Main's input bindings without its synthetic loop/obstacle setup.
public partial class HelsinkiRoad : Main
{
    public VehicleCar Vehicle { get; private set; }
    public Vector3[] Centerline { get; private set; }
    public Node3D RoadCollision { get; private set; }
    public string InitializationError { get; private set; }
    private Label _speed;

    public override void _Ready()
    {
        try
        {
            using var config = ReadJson("res://HelsinkiRoad.json");
            var settings = config.RootElement;
            var path = settings.GetProperty("manifestPath").GetString();
            using var manifest = ReadJson(path);
            var m = manifest.RootElement;
            if (m.GetProperty("schemaVersion").GetInt32() != 1 || m.GetProperty("kind").GetString() != "flat-road-strip" ||
                m.GetProperty("units").GetString() != "metres" || m.GetProperty("horizontalCrs").GetString() != "EPSG:3067")
                throw new InvalidOperationException("Expected flat-road-strip v1 in EPSG:3067 metres.");
            var o = m.GetProperty("origin");
            var frame = new CoordinateFrame(new(o.GetProperty("east").GetDouble(), o.GetProperty("north").GetDouble(),
                o.GetProperty("height").GetDouble()), VerticalDatum.Synthetic, 500);
            var y = m.GetProperty("flatLocalHeight").GetDouble();
            Centerline = m.GetProperty("centerline").EnumerateArray().Select(p =>
            {
                var local = frame.ToEngine(new(p.GetProperty("east").GetDouble(), p.GetProperty("north").GetDouble(),
                    frame.Origin.Height + y), VerticalDatum.Synthetic);
                return new Vector3((float)local.X, (float)local.Y, (float)local.Z);
            }).ToArray();
            if (Centerline.Length < 2) throw new InvalidOperationException("Road needs at least two centerline points.");
            var assets = m.GetProperty("assets").EnumerateArray().ToArray();
            Node3D Load(string role)
            {
                var asset = assets.Single(a => a.GetProperty("role").GetString().StartsWith(role, StringComparison.Ordinal));
                var relative = asset.GetProperty("path").GetString();
                if (relative.Contains('/') || relative.Contains('\\') || relative is "." or "..")
                    throw new InvalidOperationException("Expected asset filename next to manifest.");
                var resource = path[..(path.LastIndexOf('/') + 1)] + relative;
                var packed = GD.Load<PackedScene>(resource) ?? throw new InvalidOperationException($"Missing imported asset {resource}.");
                var node = packed.Instantiate<Node3D>();
                node.Name = role == "visual" ? "RoadVisual" : "RoadCollision";
                AddChild(node);
                return node;
            }
            Load("visual");
            RoadCollision = Load("static concave collision");
            var spawnDistance = Number(settings, "spawnDistanceMetres", 0.5, 20);
            var clearance = Number(settings, "spawnClearanceMetres", 0.2, 2);
            var margin = Number(settings, "groundMarginMetres", 1, 10);
            var drop = Number(settings, "groundDropMetres", 0.05, 1);
            var start = AtDistance(spawnDistance, out var direction);
            var spawn = new Marker3D { Name = "Spawn", Position = start + Vector3.Up * clearance,
                Rotation = new Vector3(0, Mathf.Atan2(-direction.X, -direction.Z), 0) };
            AddChild(spawn);
            Vehicle = new VehicleCar { Name = "Car", Transform = spawn.Transform };
            AddChild(Vehicle);

            Vector3 Point(JsonElement p) => new((float)p.GetProperty("x").GetDouble(), (float)p.GetProperty("y").GetDouble(), (float)p.GetProperty("z").GetDouble());
            var min = Point(m.GetProperty("min")); var max = Point(m.GetProperty("max"));
            var size = new Vector3(max.X - min.X + margin * 2, 0.2f, max.Z - min.Z + margin * 2);
            var ground = new StaticBody3D { Name = "TemporaryGround", Position = new Vector3((min.X + max.X) / 2, (float)y - drop - 0.1f, (min.Z + max.Z) / 2) };
            ground.AddChild(new CollisionShape3D { Shape = new BoxShape3D { Size = size } });
            Main.AddBox(ground, "GroundMesh", size, Vector3.Zero, new Color(0.32f, 0.42f, 0.3f));
            AddChild(ground);
            var length = m.GetProperty("lengthMetres").GetDouble();
            foreach (var reference in new[] { ("START", 0.0, Colors.LightGreen), ("MID", length / 2, Colors.LightYellow), ("END", length, Colors.OrangeRed) })
            {
                var point = AtDistance((float)reference.Item2, out var tangent);
                var marker = new Marker3D { Name = reference.Item1, Position = point };
                AddChild(marker);
                // Reference node is on the centerline; label/pole sit beyond the road edge.
                var offset = new Vector3(-tangent.Z, 0, tangent.X) * ((float)m.GetProperty("widthMetres").GetDouble() / 2 + 1);
                Main.AddBox(marker, "Pole", new Vector3(0.15f, 2, 0.15f), offset + Vector3.Up, reference.Item3);
                marker.AddChild(new Label3D { Text = $"{reference.Item1}  {reference.Item2:0} m", Position = offset + Vector3.Up * 2.4f,
                    Billboard = BaseMaterial3D.BillboardModeEnum.Enabled, FontSize = 64, PixelSize = 0.01f });
            }
            AddChild(new DirectionalLight3D { Name = "Sun", RotationDegrees = new Vector3(-55, -30, 0), LightEnergy = 1.2f, ShadowEnabled = true });
            var rig = new ChaseCamera { Name = "ChaseCamera" };
            rig.AddChild(new Camera3D { Name = "Camera", Near = 0.1f, Fov = 70, Current = true });
            AddChild(rig);
            var hud = new CanvasLayer(); AddChild(hud);
            var panel = new PanelContainer { Position = new Vector2(16, 16) }; hud.AddChild(panel);
            var labels = new VBoxContainer(); panel.AddChild(labels);
            _speed = new Label(); labels.AddChild(_speed);
            labels.AddChild(new Label { Text = "Stenbäckinkatu — 100 m flat prototype\nW / ↑ accelerate   S / ↓ brake/reverse   A D / ← → steer\nSpace brake   R reset   Follow START → MID → END\nSynthetic flat height; temporary ground. No real elevation.\n© OpenStreetMap contributors — ODbL 1.0" });
            GD.Print("Helsinki road ready: spawn=", spawn.Position, " heading=", direction);
        }
        catch (Exception error)
        {
            InitializationError = error.Message;
            GD.PushError($"Helsinki road setup failed: {error.Message}. Generate road-graph and road-strip offline, then import; see docs/PIPELINE.md.");
            GetTree().Quit(1);
        }
    }

    public Vector3 AtDistance(float distance, out Vector3 direction)
    {
        for (int i = 1; i < Centerline.Length; i++)
        {
            var edge = Centerline[i] - Centerline[i - 1];
            var length = edge.Length();
            if (length < 0.001f) throw new InvalidOperationException("Degenerate road centerline edge.");
            if (distance <= length + 0.001f)
            {
                direction = edge / length;
                return Centerline[i - 1] + direction * Mathf.Clamp(distance, 0, length);
            }
            distance -= length;
        }
        throw new InvalidOperationException("Distance exceeds road centerline length.");
    }
    public override void _Process(double delta)
    {
        if (_speed != null) _speed.Text = $"{Vehicle.SpeedKmh:0.0} km/h";
    }
    private static float Number(JsonElement config, string key, double min, double max)
    {
        var value = config.GetProperty(key).GetDouble();
        if (!double.IsFinite(value) || value < min || value > max) throw new InvalidOperationException($"{key} must be {min}..{max} metres.");
        return (float)value;
    }
    private static JsonDocument ReadJson(string path)
    {
        using var file = Godot.FileAccess.Open(path, Godot.FileAccess.ModeFlags.Read);
        if (file == null) throw new InvalidOperationException($"Missing {path}.");
        return JsonDocument.Parse(file.GetAsText());
    }
}
