using Godot;
using System;
using System.Text;

// Integration check: real input actions, physics callbacks, floor and MoveAndSlide.
// Run this scene explicitly; it is never added to the playable scene.
public partial class DrivingChecks : Node
{
    private Car _car;
    private int _ticks;
    private int _frames;
    private float _stoppedYaw;
    private readonly StringBuilder _trace = new();

    public override void _Ready()
    {
        var world = GD.Load<PackedScene>("res://Main.tscn").Instantiate<Main>();
        AddChild(world);
        _car = world.GetNode<Car>("Car");
        ProcessPhysicsPriority = 100; // Observe after the car has moved this tick.
        Input.ActionPress("accelerate");
    }

    public override void _Process(double delta) => _frames++;

    public override void _PhysicsProcess(double delta)
    {
        try
        {
            _ticks++;
            Require(_car.GlobalPosition.IsFinite(), "finite position");
            Require(Mathf.Abs(_car.GlobalPosition.Y) < 0.03f, "car stays on flat floor");
            if (_ticks % 60 != 0) return;
            _trace.Append(FormattableString.Invariant($"tick={_ticks} pos={_car.GlobalPosition.X:F4},{_car.GlobalPosition.Y:F4},{_car.GlobalPosition.Z:F4} speed={_car.SignedSpeed:F4} yaw={_car.Rotation.Y:F4}; "));
            switch (_ticks)
            {
                case 120:
                    Require(_car.SignedSpeed > 11.9f && _car.SpeedKmh > 43f, "forward limit and km/h");
                    Require(_car.GlobalPosition.Z < -3.5f, "forward is -Z");
                    SetInput("brake", "accelerate"); // Brake takes priority over throttle.
                    break;
                case 180:
                    Require(Mathf.Abs(_car.SignedSpeed) < 0.01f, "space stops without reversing");
                    SetInput("reverse", "steer_right");
                    break;
                case 240:
                    Require(_car.SignedSpeed < -3.9f && _car.SpeedKmh > 14f, "reverse limit, positive HUD speed");
                    Require(_car.Rotation.Y > 0.5f, "right steering reverses while backing");
                    SetInput("brake", "steer_right");
                    break;
                case 300:
                    Require(Mathf.Abs(_car.SignedSpeed) < 0.01f, "brake stops reverse");
                    _stoppedYaw = _car.Rotation.Y;
                    SetInput("steer_right");
                    break;
                case 360:
                    Require(Mathf.Abs(_car.Rotation.Y - _stoppedYaw) < 0.0001f, "no turning in place");
                    SetInput("accelerate", "steer_right");
                    break;
                case 420:
                    Require(_car.SignedSpeed > 5.9f && _car.Rotation.Y < _stoppedYaw - 0.5f, "forward right steering");
                    SetInput("reverse");
                    break;
                case 480:
                    Require(_car.SignedSpeed < -2f && _car.SignedSpeed > -4f, "opposite pedal stops then reverses");
                    SetInput("brake");
                    break;
                case 540:
                    Require(Mathf.Abs(_car.SignedSpeed) < 0.01f, "second stop");
                    SetInput("accelerate", "steer_left");
                    _stoppedYaw = _car.Rotation.Y;
                    break;
                case 600:
                    Require(_car.Rotation.Y > _stoppedYaw + 0.5f, "forward left steering");
                    SetInput();
                    break;
                case 660:
                    Require(_car.SignedSpeed > 3.9f && _car.SignedSpeed < 4.1f, "coasting slows car");
                    SetInput("brake");
                    break;
                case 720:
                    Require(Mathf.Abs(_car.SignedSpeed) < 0.01f && _car.IsOnFloor(), "final stop on ground");
                    SetInput();
                    GD.Print("P03 CHECK PASS physics_ticks=", _ticks, " render_frames=", _frames, " cap=", Engine.MaxFps);
                    GD.Print(_trace.ToString());
                    GetTree().Quit();
                    break;
            }
        }
        catch (Exception error)
        {
            GD.PushError($"P03 CHECK FAIL tick={_ticks}: {error.Message}");
            GetTree().Quit(1);
        }
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    private static void SetInput(params string[] pressed)
    {
        foreach (string action in new[] { "accelerate", "reverse", "brake", "steer_left", "steer_right" })
            Input.ActionRelease(action);
        foreach (string action in pressed) Input.ActionPress(action);
    }
}
