using Godot;
using System;

public partial class Car : CharacterBody3D
{
    [Export(PropertyHint.Range, "1,20,0.5")] public float Acceleration { get; set; } = 6f;
    [Export(PropertyHint.Range, "1,30,0.5")] public float Braking { get; set; } = 12f;
    [Export(PropertyHint.Range, "0.1,10,0.1")] public float CoastDeceleration { get; set; } = 2f;
    [Export(PropertyHint.Range, "1,30,0.5")] public float ForwardLimit { get; set; } = 12f;
    [Export(PropertyHint.Range, "1,15,0.5")] public float ReverseLimit { get; set; } = 4f;
    [Export(PropertyHint.Range, "10,180,1")] public float SteeringDegrees { get; set; } = 75f;
    [Export] public NodePath SpawnPath { get; set; } = new("../Spawn");
    [Export] public float RecoveryHeight { get; set; } = -10f;

    public event Action Recovered;
    private Node3D _spawn;

    public float SignedSpeed { get; private set; }
    public float SpeedKmh => new Vector2(Velocity.X, Velocity.Z).Length() * 3.6f;

    public override void _Ready()
    {
        _spawn = GetNode<Node3D>(SpawnPath);
        AddChild(new CollisionShape3D { Shape = new BoxShape3D { Size = new Vector3(1.8f, 0.7f, 3.4f) }, Position = new Vector3(0, 0.35f, 0) });
        Main.AddBox(this, "Body", new Vector3(1.8f, 0.65f, 3.4f), new Vector3(0, 0.55f, 0), new Color(0.95f, 0.42f, 0.12f));
        Main.AddBox(this, "Cabin", new Vector3(1.4f, 0.6f, 1.5f), new Vector3(0, 1.15f, 0.3f), new Color(0.16f, 0.24f, 0.3f));
        // Bright nose and red tail distinguish forward (-Z) from reverse.
        foreach (float x in new[] { -0.58f, 0.58f })
        {
            Main.AddBox(this, "Headlight", new Vector3(0.35f, 0.2f, 0.05f), new Vector3(x, 0.6f, -1.72f), Colors.LightYellow);
            Main.AddBox(this, "TailLight", new Vector3(0.35f, 0.2f, 0.05f), new Vector3(x, 0.6f, 1.72f), Colors.Red);
            foreach (float z in new[] { -1f, 1f })
                Main.AddBox(this, "Wheel", new Vector3(0.25f, 0.45f, 0.65f), new Vector3(x * 1.55f, 0.3f, z), new Color(0.08f, 0.08f, 0.09f));
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (Input.IsActionJustPressed("reset") || GlobalPosition.Y < RecoveryHeight)
        {
            ResetToSpawn();
            return; // No gravity or held driving input on the recovery tick.
        }
        float dt = (float)delta;
        float throttle = Input.GetAxis("reverse", "accelerate");
        if (Input.IsActionPressed("brake"))
            SignedSpeed = Mathf.MoveToward(SignedSpeed, 0, Braking * dt);
        else if (throttle == 0)
            SignedSpeed = Mathf.MoveToward(SignedSpeed, 0, CoastDeceleration * dt);
        else if (SignedSpeed * throttle < 0)
            // Opposite pedal brakes to zero before engaging the other direction.
            SignedSpeed = Mathf.MoveToward(SignedSpeed, 0, Braking * dt);
        else
            SignedSpeed = Mathf.MoveToward(SignedSpeed, throttle > 0 ? ForwardLimit : -ReverseLimit, Acceleration * dt);

        float steering = Input.GetAxis("steer_left", "steer_right");
        float turn = Mathf.DegToRad(SteeringDegrees) * Mathf.Clamp(SignedSpeed / 4f, -1, 1);
        RotateY(-steering * turn * dt);
        Vector3 horizontal = -GlobalBasis.Z * SignedSpeed;
        Velocity = new Vector3(horizontal.X, IsOnFloor() ? 0 : Velocity.Y - 9.8f * dt, horizontal.Z);
        // MoveAndSlide integrates velocity itself; do not multiply by delta again.
        MoveAndSlide();
        if (IsOnWall())
        {
            // Do not retain speed removed by an impact (or brake that hidden speed
            // before backing away). Preserve only remaining motion along the heading.
            float remaining = Velocity.Dot(-GlobalBasis.Z);
            SignedSpeed = SignedSpeed >= 0
                ? Mathf.Clamp(remaining, 0, SignedSpeed)
                : Mathf.Clamp(remaining, SignedSpeed, 0);
        }
    }

    public void ResetToSpawn()
    {
        GlobalTransform = _spawn.GlobalTransform;
        Velocity = Vector3.Zero;
        SignedSpeed = 0;
        ResetPhysicsInterpolation();
        Recovered?.Invoke();
    }
}
