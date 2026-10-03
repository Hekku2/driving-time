using System;
using Godot;

public partial class CameraChecks : Node
{
    private Car _car;
    private ChaseCamera _rig;
    private Camera3D _camera;
    private int _ticks;
    private Vector3 _previous;
    private float _blockedDistance;

    public override void _Ready()
    {
        var world = GD.Load<PackedScene>("res://Main.tscn").Instantiate<Main>();
        AddChild(world);
        _car = world.GetNode<Car>("Car");
        _rig = world.GetNode<ChaseCamera>("ChaseCamera");
        _camera = _rig.Camera;
        _car.SetPhysicsProcess(false);
        ProcessPhysicsPriority = 100;
        _previous = _camera.GlobalPosition;
    }

    public override void _PhysicsProcess(double delta)
    {
        try
        {
            _ticks++;
            Vector3 pivot = _car.GlobalPosition + Vector3.Up * 1.4f;
            Require(_camera.GlobalPosition.IsFinite() && _camera.GlobalBasis.IsFinite(), "finite camera transform");
            float distance = _camera.GlobalPosition.DistanceTo(pivot);
            Require(distance < 9f, "bounded camera distance");
            var query = new PhysicsShapeQueryParameters3D
            {
                Shape = new SphereShape3D { Radius = 0.34f },
                Transform = _camera.GlobalTransform,
                Exclude = new Godot.Collections.Array<Rid> { _car.GetRid() }
            };
            Require(_car.GetWorld3D().DirectSpaceState.IntersectShape(query).Count == 0, "camera volume outside solids");
            if (_ticks <= 120)
            {
                Require(_camera.GlobalPosition.DistanceTo(_previous) < 0.5f, "smooth turn steps");
                Vector3 ahead = pivot - _car.GlobalBasis.Z * 4f;
                Require(_camera.IsPositionInFrustum(ahead), "road ahead visible during turn");
                _car.RotateY(Mathf.DegToRad(1.25f));
                _car.GlobalPosition += -_car.GlobalBasis.Z * 0.05f;
            }
            if (_ticks == 120)
            {
                _car.GlobalPosition = new Vector3(17, 0, 0);
                _car.Rotation = new Vector3(0, Mathf.Pi / 2f, 0);
                _car.ResetPhysicsInterpolation();
            }
            if (_ticks == 240)
            {
                Require(_camera.GlobalPosition.X < 19.4f && distance < 3f, "wall retracts arm");
                _blockedDistance = distance;
                _car.GlobalPosition = new Vector3(10, 0, 0);
                _car.ResetPhysicsInterpolation();
            }
            if (_ticks == 241)
                Require(distance < _blockedDistance + 0.1f, "no snap outward when wall clears");
            if (_ticks == 420)
            {
                Require(distance > 7.4f, "arm returns after wall clears");
                GD.Print("P04 CHECK PASS physics_ticks=", _ticks, " cap=", Engine.MaxFps);
                GetTree().Quit();
            }
            _previous = _camera.GlobalPosition;
        }
        catch (Exception error)
        {
            GD.PushError($"P04 CHECK FAIL tick={_ticks}: {error.Message}");
            GetTree().Quit(1);
        }
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
