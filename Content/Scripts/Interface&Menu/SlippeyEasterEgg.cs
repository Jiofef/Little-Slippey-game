using Godot;
using System;

public partial class SlippeyEasterEgg : AnimatedSprite2D
{
    private float _xMotion = 0, _yMotion = 0, _gravity = 9.8f;
    public override void _Ready()
	{
        SetPhysicsProcess(false);
    }
    public void GoingFall()
    {
        SetPhysicsProcess(true);
        Random random = new Random();
        _xMotion = random.Next(100) < 50 ? 3 : -3;
        _yMotion = -5;
        Animation = "Death";
    }
    public override void _PhysicsProcess(double delta)
    {
        if (Position.Y > 1000) QueueFree();
        Position = new Vector2(Position.X + _xMotion, Position.Y + _yMotion);
        Rotation += _xMotion / 50;
        _yMotion += _gravity / 100;
    }
}
