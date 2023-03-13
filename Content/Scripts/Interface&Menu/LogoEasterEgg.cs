using Godot;
using System;

public partial class LogoEasterEgg : TextureButton
{
    private float _xMotion = 0, _yMotion = 0, _gravity = 9.8f;
    AnimatedSprite2D _logo;
    public override void _Ready()
    {
        SetPhysicsProcess(false);
    }
    public void GoingFall()
    {
        SetPhysicsProcess(true);
        _xMotion = 3;
        _yMotion = -5;
        _logo = GetNode<AnimatedSprite2D>("Logo");
        _logo.Animation = "EasterEggAnimation";
    }
    public override void _PhysicsProcess(double delta)
    {
        if (_logo.Position.Y > 1000) QueueFree();
        _logo.Position = new Vector2(_logo.Position.X + _xMotion, _logo.Position.Y + _yMotion);
        _logo.Rotation += _xMotion / 50;
        _yMotion += _gravity / 100;
    }
}
