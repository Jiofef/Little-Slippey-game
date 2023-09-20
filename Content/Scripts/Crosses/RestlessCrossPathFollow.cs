using Godot;

public partial class RestlessCrossPathFollow : Path2D
{
    private float _timer = 0;
    PathFollow2D _follow;
    public override void _Ready()
    {
        SetPhysicsProcess(false);
        _follow = GetNode<PathFollow2D>("PathFollow2D");
    }
    public override void _PhysicsProcess(double delta)
    {
        _timer += 0.016667f;
        _follow.Progress = 300 * _timer * _timer;
    }
}
