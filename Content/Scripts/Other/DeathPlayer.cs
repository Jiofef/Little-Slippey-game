using Godot;
using System;

public class DeathPlayer : Node2D
{
    [Signal] public delegate void Death();
    float xmotion = 0, ymotion = 0, _gravity = 9.8f;
    public Sprite DeadSprite;
    public override void _Ready()
    {
        DeadSprite = GetNode<Sprite>("Sprite");
    }
    public void Activate()
    {
        Random random = new Random();
        var GlobalXPos = GetGlobalPosition().x;
        xmotion = random.Next(100) > 50 ? -5 * (GlobalXPos / 1024) : 5 * ((1 - GlobalXPos / 1024));
        ymotion = -8;
        var deathSound = GetNode<AudioStreamPlayer>("DeathSound");
        deathSound.Play();
        EmitSignal("Death");
    }
    public override void _PhysicsProcess(float delta)
    {
        if (!DeadSprite.Visible) return;
        if (G._playerdeathtimer != 4.5f)
        {
            G._playerdeathtimer = G._playerdeathtimer < 4.5f ? G._playerdeathtimer + delta : 4.5f;
            Position = new Vector2(Position.x + xmotion * (1 - G._playerdeathtimer / 4.5f), Position.y + ymotion * (1 - G._playerdeathtimer / 4.5f));
            Rotation += xmotion / 50 * (1 - G._playerdeathtimer / 4.5f);
            ymotion += _gravity / 100;
        }
        else
        {
            G.afterdeadtimer += delta;
        }
    }
}