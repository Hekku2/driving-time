using Godot;
using System;

// Experimental force-driven alternative. Local nose remains -Z for the existing
// scene/camera; Godot's vehicle engine force uses +Z, so pedal force is negated.
public partial class VehicleCar : VehicleBody3D
{
    [Export] public float DriveForce { get; set; } = 2800f;
    [Export] public float BrakeForce { get; set; } = 55f;
    [Export] public float ForwardLimit { get; set; } = 12f;
    [Export] public float ReverseLimit { get; set; } = 4f;
    [Export] public float SteeringDegrees { get; set; } = 28f;
    [Export] public float SteeringResponse { get; set; } = 3f;
    [Export] public float SuspensionStiffness { get; set; } = 30f;
    [Export] public NodePath SpawnPath { get; set; } = new("../Spawn");
    [Export] public float RecoveryHeight { get; set; } = -10f;

    public event Action Recovered;
    public float SignedSpeed => LinearVelocity.Dot(-GlobalBasis.Z);
    public float SpeedKmh => new Vector2(LinearVelocity.X, LinearVelocity.Z).Length() * 3.6f;
    public VehicleWheel3D[] Wheels { get; private set; }
    private Node3D _spawn;
    private bool _resetRequested;

    public override void _Ready()
    {
        _spawn = GetNode<Node3D>(SpawnPath);
        Mass = 1000;
        CenterOfMassMode = CenterOfMassModeEnum.Custom;
        CenterOfMass = Vector3.Zero;
        ContinuousCd = true;
        CanSleep = false;
        PhysicsInterpolationMode = PhysicsInterpolationModeEnum.On;
        AddChild(new CollisionShape3D { Shape = new BoxShape3D { Size = new Vector3(1.8f, 0.7f, 3.4f) }, Position = new Vector3(0, 0.45f, 0) });
        Main.AddBox(this, "Body", new Vector3(1.8f, 0.65f, 3.4f), new Vector3(0, 0.45f, 0), new Color(0.95f, 0.42f, 0.12f));
        Main.AddBox(this, "Cabin", new Vector3(1.4f, 0.6f, 1.5f), new Vector3(0, 1.05f, 0.3f), new Color(0.16f, 0.24f, 0.3f));
        Wheels = new VehicleWheel3D[4];
        int index = 0;
        foreach (float x in new[] { -0.95f, 0.95f })
        {
            Main.AddBox(this, "Headlight", new Vector3(0.35f, 0.2f, 0.05f), new Vector3(x * 0.61f, 0.5f, -1.72f), Colors.LightYellow);
            Main.AddBox(this, "TailLight", new Vector3(0.35f, 0.2f, 0.05f), new Vector3(x * 0.61f, 0.5f, 1.72f), Colors.Red);
            foreach (float z in new[] { -1.1f, 1.1f })
            {
                var wheel = new VehicleWheel3D
                {
                    Name = z < 0 ? (x < 0 ? "FrontLeft" : "FrontRight") : (x < 0 ? "RearLeft" : "RearRight"),
                    Position = new Vector3(x, 0.2f, z),
                    UseAsSteering = z < 0,
                    UseAsTraction = z > 0,
                    WheelRadius = 0.32f,
                    WheelRestLength = 0.3f,
                    SuspensionTravel = 0.2f,
                    SuspensionStiffness = SuspensionStiffness,
                    SuspensionMaxForce = 8000f,
                    DampingCompression = 0.8f,
                    DampingRelaxation = 1f,
                    WheelFrictionSlip = 1f,
                    WheelRollInfluence = 0.1f,
                    PhysicsInterpolationMode = PhysicsInterpolationModeEnum.On
                };
                AddChild(wheel);
                wheel.AddChild(new MeshInstance3D
                {
                    Rotation = new Vector3(0, 0, Mathf.Pi / 2),
                    Mesh = new CylinderMesh { TopRadius = 0.32f, BottomRadius = 0.32f, Height = 0.25f,
                        Material = new StandardMaterial3D { AlbedoColor = new Color(0.08f, 0.08f, 0.09f) } }
                });
                Wheels[index++] = wheel;
            }
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (Input.IsActionJustPressed("reset")) ResetToSpawn();
        if (_resetRequested || GlobalPosition.Y < RecoveryHeight)
        {
            _resetRequested = true;
            EngineForce = Brake = Steering = 0;
            return;
        }
        float pedal = Input.GetAxis("reverse", "accelerate");
        float speed = SignedSpeed;
        bool braking = Input.IsActionPressed("brake") || pedal * speed < -0.2f;
        Brake = braking ? BrakeForce : pedal == 0 ? 1f : 0;
        float limit = pedal > 0 ? ForwardLimit : ReverseLimit;
        EngineForce = braking || pedal == 0 ? 0 : -pedal * DriveForce * Mathf.Clamp((limit - Mathf.Abs(speed)) / 2f, 0, 1);
        float target = -Input.GetAxis("steer_left", "steer_right") * Mathf.DegToRad(SteeringDegrees);
        Steering = Mathf.MoveToward(Steering, target, SteeringResponse * (float)delta);
    }

    public void ResetToSpawn() => _resetRequested = true;

    public override void _IntegrateForces(PhysicsDirectBodyState3D state)
    {
        if (!_resetRequested && state.Transform.Origin.Y >= RecoveryHeight) return;
        state.Transform = _spawn.GlobalTransform;
        state.LinearVelocity = Vector3.Zero;
        state.AngularVelocity = Vector3.Zero;
        // Synchronize the node for the camera reset callback on this exceptional
        // teleport; normal driving leaves transform integration to the engine.
        GlobalTransform = state.Transform;
        LinearVelocity = AngularVelocity = Vector3.Zero;
        EngineForce = Brake = Steering = 0;
        _resetRequested = false;
        ResetPhysicsInterpolation();
        foreach (var wheel in Wheels) wheel.ResetPhysicsInterpolation();
        Recovered?.Invoke();
    }
}
