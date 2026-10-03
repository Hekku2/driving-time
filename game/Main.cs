using Godot;

public partial class Main : Node3D
{
    private Car _car;
    private Label _speed;

    public override void _EnterTree()
    {
        Bind("accelerate", Key.W, Key.Up);
        Bind("reverse", Key.S, Key.Down);
        Bind("steer_left", Key.A, Key.Left);
        Bind("steer_right", Key.D, Key.Right);
        Bind("brake", Key.Space);
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
        _car = GetNode<Car>("Car");
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

        var hud = new CanvasLayer();
        AddChild(hud);
        var panel = new PanelContainer { Position = new Vector2(16, 16) };
        hud.AddChild(panel);
        var labels = new VBoxContainer();
        panel.AddChild(labels);
        _speed = new Label();
        _speed.AddThemeFontSizeOverride("font_size", 28);
        labels.AddChild(_speed);
        labels.AddChild(new Label { Text = "W / ↑ accelerate   S / ↓ brake, then reverse\nA D / ← → steer   Space brake" });
        GD.Print("P04: chase camera driving loop ready (C#), physics=", Engine.PhysicsTicksPerSecond);
    }

    public override void _Process(double delta)
    {
        _speed.Text = $"{_car.SpeedKmh:0.0} km/h   {(_car.SignedSpeed < -0.01f ? "REV" : "FWD")}";
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
}
