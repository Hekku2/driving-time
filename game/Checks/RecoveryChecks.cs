using System;
using Godot;

// Exercises the playable scene's real solids, input actions and physics callbacks.
public partial class RecoveryChecks : Node
{
    private Car _car;
    private ChaseCamera _rig;
    private Node3D _spawn;
    private int _tick;
    private int _recoveries;
    private bool _hit;
    private bool _failed;
    private Vector3 _stoppedOnSlope;
    private Transform3D _spawnCamera;

    public override void _Ready()
    {
        var world = GD.Load<PackedScene>("res://Main.tscn").Instantiate<Main>();
        AddChild(world);
        _car = world.GetNode<Car>("Car");
        _rig = world.GetNode<ChaseCamera>("ChaseCamera");
        _spawn = world.GetNode<Node3D>("Spawn");
        _spawnCamera = _rig.Camera.GlobalTransform;
        _car.Recovered += CheckRecovery;
        ProcessPhysicsPriority = 100;
        Place(new Vector3(-6, 0, 6));
        SetInput("accelerate");
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_failed) return;
        try
        {
            _tick++;
            Require(_car.GlobalTransform.IsFinite(), "finite car transform");
            // Shrink away from contact margins, so a supported car is not an overlap.
            var query = new PhysicsShapeQueryParameters3D
            {
                Shape = new BoxShape3D { Size = new Vector3(1.78f, 0.68f, 3.38f) },
                Transform = new Transform3D(_car.GlobalBasis, _car.GlobalPosition + Vector3.Up * 0.35f),
                Exclude = new Godot.Collections.Array<Rid> { _car.GetRid() }
            };
            Require(_car.GetWorld3D().DirectSpaceState.IntersectShape(query).Count == 0, "car collider outside solid interiors");
            _hit |= _car.IsOnWall();
            switch (_tick)
            {
                case 180:
                    Require(_hit && _car.GlobalPosition.Z > -1.81f && _car.SpeedKmh < 0.1f, "full-speed impact blocked");
                    Require(Mathf.Abs(_car.SignedSpeed) < 0.01f, "impact clears blocked controller speed");
                    GD.Print("P05 impact: requested signed speed=", _car.SignedSpeed, " actual km/h=", _car.SpeedKmh);
                    SetInput("reverse");
                    break;
                case 300:
                    Require(_car.GlobalPosition.Z > 1f, "can reverse away from impact");
                    Place(new Vector3(-4, 0, 2), Mathf.Pi);
                    SetInput("accelerate");
                    break;
                case 480:
                    Require(_hit && _car.GlobalPosition.Z < 5.31f && _car.SpeedKmh < 0.1f, "low block is solid");
                    Place(new Vector3(4, 0, 12));
                    SetInput("accelerate");
                    break;
                case 660:
                    Require(_hit && _car.GlobalPosition.Z > 7.69f && _car.SpeedKmh < 0.1f, "pillar is solid");
                    Place(new Vector3(-14, 0, 10));
                    SetInput("accelerate");
                    break;
                case 750:
                    Require(_car.GlobalPosition.Y > 0.5f && _car.IsOnFloor(), "climbs 10-degree ramp as floor");
                    SetInput("brake");
                    break;
                case 795:
                    Require(_car.IsOnFloor() && Mathf.Abs(_car.SignedSpeed) < 0.01f, "brakes on ramp");
                    _stoppedOnSlope = _car.GlobalPosition;
                    SetInput();
                    break;
                case 855:
                    Require(_car.GlobalPosition.DistanceTo(_stoppedOnSlope) < 0.02f, "no idle slope drift");
                    SetInput("reverse");
                    break;
                case 1095:
                    Require(_car.GlobalPosition.Y < 0.03f && _car.GlobalPosition.Z > 8 && _car.IsOnFloor(), "descends ramp back to floor");
                    // Compress and turn the camera at the existing wall before resetting.
                    Place(new Vector3(17, 0, 0), Mathf.Pi / 2);
                    SetInput();
                    break;
                case 1155:
                    Require(_rig.Camera.GlobalPosition.DistanceTo(_car.GlobalPosition + Vector3.Up * 1.4f) < 3, "camera arm compressed before reset");
                    _car.Velocity = new Vector3(5, -7, 3);
                    SetInput("reset", "accelerate", "steer_right");
                    break;
                case 1156:
                    Require(_recoveries == 1, "R action recovers once");
                    // Leave reset pressed; releasing/repressing would be a new action.
                    Input.ActionRelease("accelerate");
                    Input.ActionRelease("steer_right");
                    break;
                case 1170:
                    Require(_recoveries == 1 && _car.GlobalPosition.DistanceTo(_spawn.GlobalPosition) < 0.03f, "reset leaves stationary car, holding R does not retrigger");
                    Place(new Vector3(26, 0, 0), -Mathf.Pi / 2);
                    _car.RecoveryHeight = -3; // Verify configurable threshold, not hardcoded -10.
                    SetInput("accelerate");
                    break;
                case 1260:
                    Require(_car.GlobalPosition.X > 31 && !_car.IsOnFloor(), "drives off bounded ground");
                    SetInput();
                    break;
                case 1380:
                    Require(_recoveries == 2, "natural fall auto-recovers exactly once");
                    Require(_car.GlobalPosition.DistanceTo(_spawn.GlobalPosition) < 0.03f && _car.SpeedKmh < 0.01f, "remains safely at spawn after fall");
                    // Repeat R after releasing, from a different yaw and with nonzero speed.
                    Place(new Vector3(0, 0, -20), 0.8f);
                    SetInput("accelerate");
                    break;
                case 1410:
                    Require(_car.SignedSpeed > 2, "moving before second manual reset");
                    SetInput("reset");
                    break;
                case 1411:
                    Require(_recoveries == 3, "released and pressed R recovers again");
                    SetInput();
                    GD.Print("P05 CHECK PASS physics_ticks=", _tick, " recoveries=", _recoveries, " cap=", Engine.MaxFps);
                    GetTree().Quit();
                    break;
            }
        }
        catch (Exception error)
        {
            Fail(error);
        }
    }

    private void CheckRecovery()
    {
        try
        {
            _recoveries++;
            Require(_car.GlobalTransform.IsEqualApprox(_spawn.GlobalTransform), "restores full spawn transform");
            Require(_car.Velocity == Vector3.Zero && _car.SignedSpeed == 0, "clears vertical, horizontal and controller speed");
            Require(_rig.Camera.GlobalTransform.IsEqualApprox(_spawnCamera), "clears camera anchor, yaw and compressed arm immediately");
        }
        catch (Exception error) { Fail(error); }
    }

    private void Place(Vector3 position, float yaw = 0)
    {
        _car.GlobalPosition = position;
        _car.Rotation = new Vector3(0, yaw, 0);
        _car.Velocity = Vector3.Zero;
        _car.ResetPhysicsInterpolation();
        _hit = false;
    }

    private void Fail(Exception error)
    {
        _failed = true;
        GD.PushError($"P05 CHECK FAIL tick={_tick}: {error.Message}; pos={_car.GlobalPosition}");
        GetTree().Quit(1);
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    private static void SetInput(params string[] pressed)
    {
        foreach (string action in new[] { "accelerate", "reverse", "brake", "steer_left", "steer_right", "reset" })
            Input.ActionRelease(action);
        foreach (string action in pressed) Input.ActionPress(action);
    }
}
