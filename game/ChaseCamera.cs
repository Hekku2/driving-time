using Godot;

public partial class ChaseCamera : Node3D
{
    [Export] public NodePath TargetPath { get; set; } = new("../Car");
    [Export(PropertyHint.Range, "3,12,0.1")] public float Distance { get; set; } = 7f;
    [Export(PropertyHint.Range, "1,6,0.1")] public float Height { get; set; } = 3f;
    [Export(PropertyHint.Range, "0,8,0.1")] public float LookAhead { get; set; } = 4f;
    [Export(PropertyHint.Range, "1,15,0.1")] public float FollowResponse { get; set; } = 6f;
    [Export(PropertyHint.Range, "0,3,0.1")] public float MaximumLag { get; set; } = 1.2f;
    [Export(PropertyHint.Range, "1,10,0.1")] public float ReturnSpeed { get; set; } = 4f;

    public Camera3D Camera { get; private set; }
    private Node3D _target;
    private Vector3 _anchor;
    private float _yaw;
    private float _armLength;
    private PhysicsShapeQueryParameters3D _query;

    public override void _Ready()
    {
        _target = GetNode<Node3D>(TargetPath);
        Camera = GetNode<Camera3D>("Camera");
        ProcessPhysicsPriority = 10;
        PhysicsInterpolationMode = PhysicsInterpolationModeEnum.On;
        _anchor = _target.GlobalPosition;
        _yaw = _target.GlobalRotation.Y;
        _armLength = new Vector3(0, Height, Distance).Length();
        _query = new PhysicsShapeQueryParameters3D
        {
            Shape = new SphereShape3D { Radius = 0.35f },
            CollisionMask = 1,
            Margin = 0.02f
        };
        if (_target is CollisionObject3D collider)
            _query.Exclude = new Godot.Collections.Array<Rid> { collider.GetRid() };
        UpdateCamera(0);
        ResetPhysicsInterpolation();
    }

    public override void _PhysicsProcess(double delta) => UpdateCamera((float)delta);

    private void UpdateCamera(float delta)
    {
        float weight = 1f - Mathf.Exp(-FollowResponse * delta);
        Vector3 position = _target.GlobalPosition;
        _anchor = _anchor.Lerp(position, weight);
        _anchor = position + (_anchor - position).LimitLength(MaximumLag);
        float yawError = Mathf.Wrap(_target.GlobalRotation.Y - _yaw, -Mathf.Pi, Mathf.Pi);
        _yaw += yawError * weight;
        _yaw = _target.GlobalRotation.Y - Mathf.Clamp(Mathf.Wrap(_target.GlobalRotation.Y - _yaw,
            -Mathf.Pi, Mathf.Pi), -Mathf.Pi / 5f, Mathf.Pi / 5f);
        Basis heading = new(Vector3.Up, _yaw);
        Vector3 pivot = position + Vector3.Up * 1.4f;
        Vector3 desired = _anchor + Vector3.Up * 1.4f + heading * new Vector3(0, Height, Distance);
        Vector3 motion = desired - pivot;
        _query.Transform = new Transform3D(Basis.Identity, pivot);
        _query.Motion = motion;
        float safeLength = motion.Length() * GetWorld3D().DirectSpaceState.CastMotion(_query)[0];
        safeLength = Mathf.Max(0, safeLength - 0.05f);
        bool retracting = safeLength < _armLength;
        _armLength = retracting ? safeLength : Mathf.MoveToward(_armLength, safeLength, ReturnSpeed * delta);
        Camera.GlobalPosition = pivot + motion.Normalized() * _armLength;
        Camera.LookAt(pivot - heading.Z * LookAhead + Vector3.Up * 0.3f, Vector3.Up);
        if (retracting) Camera.ResetPhysicsInterpolation();
    }
}
