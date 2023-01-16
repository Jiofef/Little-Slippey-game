using Godot;

public class RestlessCrossPathFollow : Path2D
{
    private float _timer = 0;
    private bool _stopProcess;
    PathFollow2D _follow;
    public override void _Ready()
    {
        StopMotion();
        _follow = GetNode<PathFollow2D>("PathFollow2D");
    }
    public override void _PhysicsProcess(float delta)
    {
        if (_stopProcess) return;
        _timer += delta * G.ReversedPlayerDeathTimerCoeff;
        _follow.Offset = 300 * _timer * _timer;
    }
    public void StartMotion()
    {
        _stopProcess = false;
    }
    public void StopMotion()
    {
        _stopProcess = true;
    }
}
