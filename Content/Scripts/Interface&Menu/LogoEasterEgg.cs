using Godot;
using System;

public class LogoEasterEgg : TextureButton
{
    float xmotion = 0, ymotion = 0, _gravity = 9.8f;
    AnimatedSprite logo;
    public override void _Ready()
    {
        SetPhysicsProcess(false);
    }
    public void GoingFall()
    {
        SetPhysicsProcess(true);
        Random random = new Random();
        xmotion = 3;
        ymotion = -5;
        logo = GetNode<AnimatedSprite>("Logo");
        logo.Animation = "EasterEggAnimation";
    }
    public override void _PhysicsProcess(float delta)
    {
        if (logo.Position.y > 1000) QueueFree();
        logo.Position = new Vector2(logo.Position.x + xmotion, logo.Position.y + ymotion);
        logo.Rotation += xmotion / 50;
        ymotion += _gravity / 100;
    }
}
