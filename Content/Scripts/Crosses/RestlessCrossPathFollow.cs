using Godot;
using System;

public class RestlessCrossPathFollow : Path2D
{
    private float _timer = 0;
    private bool _stopprocess;
    PathFollow2D follow;
    public override void _Ready()
    {
        StopMotion();
        follow = GetNode<PathFollow2D>("PathFollow2D");
    }
    public override void _PhysicsProcess(float delta)
    {
        if (_stopprocess) return;
        _timer += delta * G._pdcoeff2;
        follow.Offset = 300 * _timer * _timer;
    }
    public void StartMotion()
    {
        _stopprocess = false;
    }
    public void StopMotion()
    {
        _stopprocess = true;
    }
}
