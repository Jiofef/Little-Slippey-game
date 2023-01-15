using Godot;
using System;

public class DeathPlayer : Node2D
{
    [Signal] public delegate void Death();
    private float _xMotion = 0, _yMotion = 0, _gravity = 9.8f;
    private Sprite _deadSprite;
    public override void _Ready()
    {
        _deadSprite = GetNode<Sprite>("Sprite");
    }
    public void Activate()
    {
        Random random = new Random();
        var GlobalXPos = GlobalPosition.x;
        _xMotion = random.Next(100) > 50 ? -5 * (GlobalXPos / 1024) : 5 * ((1 - GlobalXPos / 1024));
        _yMotion = -8;
        GetNode<AudioStreamPlayer>("DeathSound").Play();
        EmitSignal("Death");
    }
    public override void _PhysicsProcess(float delta)
    {
        if (!_deadSprite.Visible) return;
        if (G.PlayerDeathTimer != 4.5f)
        {
            G.PlayerDeathTimer = G.PlayerDeathTimer < 4.5f ? G.PlayerDeathTimer + delta : 4.5f;
            Position = new Vector2(Position.x + _xMotion * (1 - G.PlayerDeathTimer / 4.5f), Position.y + _yMotion * (1 - G.PlayerDeathTimer / 4.5f));
            Rotation += _xMotion / 50 * (1 - G.PlayerDeathTimer / 4.5f);
            _yMotion += _gravity / 100;
        }
        else
        {
            G.AfterPlayerDeadTimer += delta;
        }
    }
}