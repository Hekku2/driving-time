using System;
using System.Linq;
using Godot;

// Physical behavior checks, rather than the arcade controller's exact traces.
public partial class VehicleChecks : Node
{
    private VehicleCar _car;
    private ChaseCamera _rig;
    private Node3D _spawn;
    private int _tick;
    private int _recoveries;
    private Vector3 _start;
    private float _yaw;
    private bool _impact;
    private bool _climbed;
    private bool _leftGround;
    private Exception _recoveryFailure;

    public override void _Ready()
    {
        var world = GD.Load<PackedScene>("res://VehicleExperiment.tscn").Instantiate<Main>();
        AddChild(world);
        _car = world.GetNode<VehicleCar>("Car");
        _rig = world.GetNode<ChaseCamera>("ChaseCamera");
        _spawn = world.GetNode<Node3D>("Spawn");
        _car.Recovered += () =>
        {
            try
            {
                _recoveries++;
                Require(_car.GlobalTransform.IsEqualApprox(_spawn.GlobalTransform), "reset pose");
                Require(_car.LinearVelocity.Length() < 0.001f && _car.AngularVelocity.Length() < 0.001f, "reset clears linear/angular velocity");
                Require(_car.EngineForce == 0 && _car.Brake == 0 && _car.Steering == 0, "reset clears controls");
                Require((_rig.Camera.GlobalPosition - _car.GlobalPosition).Dot(_car.GlobalBasis.Z) > 0, "camera resets behind nose");
            }
            catch (Exception error) { _recoveryFailure = error; }
        };
        _car.ContactMonitor = true;
        _car.MaxContactsReported = 8;
        _car.BodyEntered += body => _impact |= body.Name == "ImpactBlock";
        ProcessPhysicsPriority = 100;
    }

    public override void _PhysicsProcess(double delta)
    {
        try
        {
            _tick++;
            if (_recoveryFailure != null) throw _recoveryFailure;
            Require(_car.GlobalTransform.IsFinite() && _car.LinearVelocity.IsFinite(), "finite physics state");
            if (_tick > 700 && _tick < 850) _climbed |= _car.GlobalPosition.Y > 0.9f;
            if (_tick > 900) _leftGround |= _car.GlobalPosition.X > 31;
            switch (_tick)
            {
                case 90:
                    Require(_car.Wheels.All(w => w.IsInContact()), "four wheels support car on floor");
                    Require(_car.GlobalPosition.Y > 0.15f && _car.GlobalPosition.Y < 0.65f, "chassis settles above floor");
                    _start = _car.GlobalPosition;
                    SetInput("accelerate");
                    break;
                case 210:
                    Trace("forward");
                    Require(_car.GlobalPosition.Z < _start.Z - 3 && _car.SignedSpeed > 3, "W drives nose-first toward -Z");
                    SetInput("brake", "accelerate");
                    break;
                case 270:
                    Trace("brake");
                    Require(_car.SpeedKmh < 1, "Space stops and overrides throttle");
                    _start = _car.GlobalPosition;
                    SetInput("reverse");
                    break;
                case 360:
                    Trace("reverse");
                    Require(_car.GlobalPosition.Z > _start.Z + 1 && _car.SignedSpeed < -1, "S reverses after stopping");
                    SetInput("brake");
                    break;
                case 420:
                    _yaw = _car.GlobalRotation.Y;
                    SetInput("accelerate", "steer_right");
                    break;
                case 510:
                    Trace("right turn");
                    Require(Mathf.Wrap(_car.GlobalRotation.Y - _yaw, -Mathf.Pi, Mathf.Pi) < -0.1f, "D turns nose right while moving forward");
                    SetInput("reset");
                    break;
                case 513:
                    Require(_recoveries == 1, "R resets once while held");
                    _spawn.Position = new Vector3(-6, 0.45f, 6);
                    _car.ResetToSpawn();
                    SetInput();
                    break;
                case 550:
                    SetInput("accelerate");
                    break;
                case 700:
                    Trace("impact");
                    Require(_impact && _car.GlobalPosition.Z > -2.5f, "actual chassis contact blocks obstacle crossing");
                    _spawn.Position = new Vector3(-14, 0.45f, 10);
                    _car.ResetToSpawn();
                    SetInput();
                    break;
                case 730:
                    SetInput("accelerate");
                    break;
                case 850:
                    Trace("ramp");
                    Require(_climbed, "wheels/chassis climb existing ramp");
                    SetInput("brake");
                    break;
                case 910:
                    _spawn.Position = new Vector3(26, 0.45f, 0);
                    _spawn.Rotation = new Vector3(0, -Mathf.Pi / 2, 0);
                    _car.ResetToSpawn();
                    _car.RecoveryHeight = -3;
                    SetInput();
                    break;
                case 940:
                    // Restore the safe recovery destination after placing at the edge.
                    _spawn.Position = new Vector3(14, 0.45f, 8);
                    _spawn.Rotation = Vector3.Zero;
                    SetInput("accelerate");
                    break;
                case 1040:
                    SetInput();
                    break;
                case 1150:
                    Trace("fall recovery");
                    Require(_leftGround && _recoveries == 5, "natural fall recovers using configurable threshold");
                    Require(_car.GlobalPosition.DistanceTo(_spawn.GlobalPosition) < 0.25f && _car.SpeedKmh < 1, "settles safely after recovery");
                    SetInput();
                    GD.Print("P05a CHECK PASS physics_ticks=", _tick, " recoveries=", _recoveries, " cap=", Engine.MaxFps);
                    GetTree().Quit();
                    break;
            }
        }
        catch (Exception error)
        {
            GD.PushError($"P05a CHECK FAIL tick={_tick}: {error.Message}; pos={_car.GlobalPosition}");
            GetTree().Quit(1);
        }
    }

    private void Trace(string label) => GD.Print("P05a ", label, ": pos=", _car.GlobalPosition, " speed=", _car.SignedSpeed, " yaw=", _car.GlobalRotation.Y);
    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
    private static void SetInput(params string[] pressed)
    {
        foreach (string action in new[] { "accelerate", "reverse", "brake", "steer_left", "steer_right", "reset" }) Input.ActionRelease(action);
        foreach (string action in pressed) Input.ActionPress(action);
    }
}
