using Godot;

public partial class Level4CircularSawScript : Node2D
{
	[Export] float _rotationSpeed = 0.1f;
    Sprite2D _saw;

    public override void _Ready()
    {
        _saw = GetNode<Sprite2D>("Saw");
    }
    public override void _PhysicsProcess(double delta)
    {
        _saw.Rotation += _rotationSpeed;
    }
}
