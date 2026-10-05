using Godot;
using System;
using System.Linq;
using System.Text.Json;

public partial class HelsinkiRoadChecks : Node
{
    private HelsinkiRoad _world;
    private int _tick;
    private bool _arrived;
    private bool _reset;
    private bool _fall;
    private int _phaseTick;
    private int _recoveries;
    private float _maximumError;

    public override void _Ready()
    {
        _world = GD.Load<PackedScene>("res://HelsinkiRoad.tscn").Instantiate<HelsinkiRoad>();
        AddChild(_world);
        if (_world.InitializationError != null) return;
        _world.Vehicle.ForwardLimit = 5;
        _world.Vehicle.Recovered += () => _recoveries++;
        ProcessPhysicsPriority = 100;
    }

    public override void _PhysicsProcess(double delta)
    {
        try
        {
            _tick++;
            Require(_tick < 2200, "bounded route/check timeout");
            var car = _world.Vehicle;
            Require(car.GlobalTransform.IsFinite() && car.LinearVelocity.IsFinite(), "finite physics");
            if (_tick == 60)
            {
                Require(_world.GetNode<Node3D>("RoadVisual").Transform == Transform3D.Identity &&
                    _world.RoadCollision.Transform == Transform3D.Identity, "identity road placement");
                using var file = Godot.FileAccess.Open("res://Generated/P12/manifest.json", Godot.FileAccess.ModeFlags.Read);
                using var doc = JsonDocument.Parse(file.GetAsText());
                var m = doc.RootElement; var o = m.GetProperty("origin");
                var points = m.GetProperty("centerline").EnumerateArray().ToArray();
                for (int i = 0; i < points.Length; i++)
                {
                    var expected = new Vector3((float)(points[i].GetProperty("east").GetDouble() - o.GetProperty("east").GetDouble()),
                        (float)m.GetProperty("flatLocalHeight").GetDouble(), (float)(o.GetProperty("north").GetDouble() - points[i].GetProperty("north").GetDouble()));
                    Require(expected.DistanceTo(_world.Centerline[i]) < 0.001f, "independent manifest coordinate agreement");
                }
                foreach (var reference in new[] { ("START", 0f), ("MID", 50f), ("END", 100f) })
                    Require(_world.GetNode<Node3D>(reference.Item1).Position.DistanceTo(_world.AtDistance(reference.Item2, out _)) < 0.001f, "reference placement");
                var spawn = _world.GetNode<Node3D>("Spawn");
                Require(spawn.Position.DistanceTo(_world.AtDistance(6, out var direction) + Vector3.Up * 0.45f) < 0.001f &&
                    (-spawn.Basis.Z).Dot(direction) > 0.999f, "spawn inside road and facing route");
                RoadSupport();
            }
            if (_tick < 90) return;
            if (!_arrived)
            {
                var distance = Progress(car.Position, out var lateral);
                _maximumError = Math.Max(_maximumError, lateral);
                Require(lateral < 2.5f, "car remains inside road");
                Require(car.Position.Y > 0.1f && car.Position.Y < 0.8f, "road supports chassis");
                if (_tick % 30 == 0) RoadSupport();
                if (distance >= 90)
                {
                    _arrived = true; _phaseTick = _tick; Input.ActionRelease("accelerate");
                    Input.ActionRelease("steer_left"); Input.ActionRelease("steer_right"); Input.ActionPress("brake");
                }
                else
                {
                    var target = _world.AtDistance(Math.Min(distance + 6, 96), out _);
                    var heading = target - car.Position;
                    var desiredYaw = Mathf.Atan2(-heading.X, -heading.Z);
                    var error = Mathf.Wrap(desiredYaw - car.Rotation.Y, -Mathf.Pi, Mathf.Pi);
                    Input.ActionRelease("steer_left"); Input.ActionRelease("steer_right");
                    if (Math.Abs(error) > 0.005f) Input.ActionPress(error > 0 ? "steer_left" : "steer_right", Math.Min(1, Math.Abs(error) * 2));
                    Input.ActionPress("accelerate");
                }
            }
            else if (!_reset && _tick - _phaseTick >= 90)
            {
                Require(car.SpeedKmh < 1, "brakes before open end");
                Input.ActionRelease("brake"); Input.ActionPress("reset");
                _reset = true; _phaseTick = _tick;
            }
            else if (_reset && !_fall && _tick - _phaseTick >= 60)
            {
                Input.ActionRelease("reset");
                Require(_recoveries == 1, "held R resets once");
                Require(car.Position.DistanceTo(_world.GetNode<Node3D>("Spawn").Position) < 0.3f && car.SpeedKmh < 1, "reset clears motion at configured spawn");
                RoadSupport();
                car.GlobalPosition = new Vector3(200, -12, 0);
                _fall = true; _phaseTick = _tick;
            }
            else if (_fall && _tick - _phaseTick >= 60)
            {
                Require(_recoveries == 2, "fall recovery");
                Require(car.Position.DistanceTo(_world.GetNode<Node3D>("Spawn").Position) < 0.3f, "fall returns to road spawn");
                var camera = _world.GetNode<ChaseCamera>("ChaseCamera").Camera;
                Require((camera.Position - car.Position).Dot(car.Basis.Z) > 0, "camera behind car after recovery");
                RoadSupport();
                GD.Print($"HELSINKI ROAD CHECK PASS: ticks={_tick}, route=6..90m, max lateral error={_maximumError:0.000}m, recoveries={_recoveries}; road wheel contact, markers, braking and camera.");
                GetTree().Quit();
            }
        }
        catch (Exception error)
        {
            GD.PushError($"HELSINKI ROAD CHECK FAIL tick={_tick}: {error.Message}"); GetTree().Quit(1);
        }
    }
    private void RoadSupport()
    {
        foreach (var wheel in _world.Vehicle.Wheels)
        {
            Require(wheel.IsInContact(), "wheel contact");
            var body = wheel.GetContactBody() as Node;
            Require(body != null && _world.RoadCollision.IsAncestorOf(body), "wheel supported by imported road, not temporary ground");
        }
    }
    private float Progress(Vector3 position, out float error)
    {
        float best = float.MaxValue, result = 0, travelled = 0;
        for (int i = 1; i < _world.Centerline.Length; i++)
        {
            var a = _world.Centerline[i - 1]; var edge = _world.Centerline[i] - a;
            var p = position; p.Y = a.Y;
            var t = Mathf.Clamp((p - a).Dot(edge) / edge.LengthSquared(), 0, 1);
            var distance = (p - (a + edge * t)).Length();
            if (distance < best) { best = distance; result = travelled + edge.Length() * t; }
            travelled += edge.Length();
        }
        error = best; return result;
    }
    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
