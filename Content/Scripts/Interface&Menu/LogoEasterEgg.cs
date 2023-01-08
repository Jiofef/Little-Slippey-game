using Godot;
using System;

public class LogoEasterEgg : TextureButton
{
    private float _xMotion = 0, _yMotion = 0, _gravity = 9.8f;
    AnimatedSprite _logo;
    public override void _Ready()
    {
        SetPhysicsProcess(false);
    }
    public void GoingFall()
    {
        SetPhysicsProcess(true);
        Random random = new Random();
        _xMotion = 3;
        _yMotion = -5;
        _logo = GetNode<AnimatedSprite>("Logo");
        _logo.Animation = "EasterEggAnimation";
    }
    public override void _PhysicsProcess(float delta)
    {
        if (_logo.Position.y > 1000) QueueFree();
        _logo.Position = new Vector2(_logo.Position.x + _xMotion, _logo.Position.y + _yMotion);
        _logo.Rotation += _xMotion / 50;
        _yMotion += _gravity / 100;
    }
}
