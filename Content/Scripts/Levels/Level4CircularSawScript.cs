using Godot;

public partial class Level4CircularSawScript : Node2D
{
	[Export] float _rotationSpeed = 0.1f;
    Sprite2D _saw;

    [Signal]
    public delegate void CirclePassedEventHandler();

    public override void _Ready()
    {
        _saw = GetNode<Sprite2D>("Saw");
    }
    public override void _PhysicsProcess(double delta)
    {
        _saw.Rotation += _rotationSpeed;
        if (_saw.RotationDegrees > 360)
        {
            _saw.Rotation = 0;
            EmitSignal("CirclePassed");
        }
    }
}
