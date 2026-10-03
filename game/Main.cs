using Godot;

public partial class Main : Node3D
{
    private Car _car;
    private VehicleCar _vehicle;
    private Label _speed;

    public override void _EnterTree()
    {
        Bind("accelerate", Key.W, Key.Up);
        Bind("reverse", Key.S, Key.Down);
        Bind("steer_left", Key.A, Key.Left);
        Bind("steer_right", Key.D, Key.Right);
        Bind("brake", Key.Space);
        Bind("reset", Key.R);
    }

    private static void Bind(string action, params Key[] keys)
    {
        if (InputMap.HasAction(action)) return;
        InputMap.AddAction(action);
        foreach (Key key in keys)
            InputMap.ActionAddEvent(action, new InputEventKey { PhysicalKeycode = key });
    }

    public override void _Ready()
    {
        _car = GetNodeOrNull<Car>("Car");
        _vehicle = GetNodeOrNull<VehicleCar>("Car");
        // Flat markings form a square loop: 36 m outer edge, 20 m inner edge.
        // No barriers or raised curbs. The existing ground is extended to 60 m.
        Color asphalt = new(0.15f, 0.17f, 0.19f);
        foreach (float side in new[] { -14f, 14f })
        {
            AddBox(this, "LoopStraight", new Vector3(36, 0.01f, 8), new Vector3(0, 0.005f, side), asphalt);
            AddBox(this, "LoopSide", new Vector3(8, 0.01f, 20), new Vector3(side, 0.005f, 0), asphalt);
        }
        Color paint = new(0.9f, 0.85f, 0.55f);
        foreach (float side in new[] { -14f, 14f })
            for (int offset = -12; offset <= 12; offset += 4)
            {
                AddBox(this, "LaneMark", new Vector3(1.8f, 0.01f, 0.12f), new Vector3(offset, 0.016f, side), paint);
                AddBox(this, "LaneMark", new Vector3(0.12f, 0.01f, 1.8f), new Vector3(side, 0.016f, offset), paint);
            }
        AddBox(this, "StartLine", new Vector3(8, 0.01f, 0.3f), new Vector3(14, 0.018f, 8), Colors.White);

        var wall = new StaticBody3D { Name = "CameraWall", Position = new Vector3(20, 2.5f, 0) };
        wall.AddChild(new CollisionShape3D { Shape = new BoxShape3D { Size = new Vector3(0.5f, 5, 24) } });
        AddBox(wall, "WallMesh", new Vector3(0.5f, 5, 24), Vector3.Zero, new Color(0.65f, 0.7f, 0.66f));
        AddChild(wall);

        // Blocks sit inside the loop; the ramp below exercises the west straight.
        AddSolidBox("ImpactBlock", new Vector3(4, 2, 1), new Vector3(-6, 1, -4), new Color(0.7f, 0.3f, 0.25f));
        AddSolidBox("Pillar", new Vector3(2, 3, 2), new Vector3(4, 1.5f, 5), new Color(0.65f, 0.5f, 0.25f));
        AddSolidBox("LowBlock", new Vector3(3, 1, 2), new Vector3(-4, 0.5f, 8), new Color(0.55f, 0.3f, 0.6f));
        // 12 m ramp rising toward -Z. The low top edge meets the floor at Y=0.
        float angle = Mathf.DegToRad(10);
        var ramp = AddSolidBox("Slope", new Vector3(6, 0.3f, 12),
            new Vector3(-14, 6 * Mathf.Sin(angle) - 0.15f * Mathf.Cos(angle), 0), new Color(0.35f, 0.6f, 0.35f));
        ramp.Rotation = new Vector3(angle, 0, 0);

        var hud = new CanvasLayer();
        AddChild(hud);
        var panel = new PanelContainer { Position = new Vector2(16, 16) };
        hud.AddChild(panel);
        var labels = new VBoxContainer();
        panel.AddChild(labels);
        _speed = new Label();
        _speed.AddThemeFontSizeOverride("font_size", 28);
        labels.AddChild(_speed);
        labels.AddChild(new Label { Text = "W / ↑ accelerate   S / ↓ brake, then reverse\nA D / ← → steer   Space brake   R reset\nGreen ramp on west side; solid blocks inside loop" });
        labels.AddChild(new Label { Text = _vehicle != null ? "VehicleBody3D experiment" : "CharacterBody3D baseline" });
        GD.Print(_vehicle != null ? "P05a: VehicleBody3D experiment ready (C#), physics=" : "P05: collision and recovery scene ready (C#), physics=", Engine.PhysicsTicksPerSecond);
    }

    public override void _Process(double delta)
    {
        float speed = _vehicle != null ? _vehicle.SpeedKmh : _car.SpeedKmh;
        float signed = _vehicle != null ? _vehicle.SignedSpeed : _car.SignedSpeed;
        _speed.Text = $"{speed:0.0} km/h   {(signed < -0.01f ? "REV" : "FWD")}";
    }

    public static void AddBox(Node3D parent, string name, Vector3 size, Vector3 position, Color color)
    {
        parent.AddChild(new MeshInstance3D
        {
            Name = name,
            Position = position,
            Mesh = new BoxMesh { Size = size, Material = new StandardMaterial3D { AlbedoColor = color, Roughness = 0.9f } }
        });
    }

    private StaticBody3D AddSolidBox(string name, Vector3 size, Vector3 position, Color color)
    {
        var body = new StaticBody3D { Name = name, Position = position };
        body.AddChild(new CollisionShape3D { Shape = new BoxShape3D { Size = size } });
        AddBox(body, "Mesh", size, Vector3.Zero, color);
        AddChild(body);
        return body;
    }
}
