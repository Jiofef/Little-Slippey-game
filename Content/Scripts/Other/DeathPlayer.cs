using Godot;
using System;

public partial class DeathPlayer : Node2D
{
    [Signal] public delegate void DeathEventHandler();
    private float _xMotion = 0, _yMotion = 0, _gravity = 9.8f;
    private Sprite2D _deadSprite;
    public override void _Ready()
    {
        _deadSprite = GetNode<Sprite2D>("Sprite2D");
        SetPhysicsProcess(false);
    }
    public void Activate()
    {
        Random random = new Random();
        _xMotion = random.Next(100) > 50 ? -5 * (GlobalPosition.X / G.LevelXYSizes[G.CurrentLevel].X) : 5 * (1 - GlobalPosition.X / G.LevelXYSizes[G.CurrentLevel].X);
        _yMotion = -8;
        GetNode<AudioStreamPlayer>("DeathSound").Play();
        EmitSignal("Death");
        SetPhysicsProcess(true);
    }
    public override void _PhysicsProcess(double delta)
    {
        if (G.PlayerDeathTimer != 4.5f)
        {
            G.PlayerDeathTimer = G.PlayerDeathTimer < 4.5f ? G.PlayerDeathTimer + 0.016667f : 4.5f;
            Position = new Vector2(Position.X + _xMotion * G.ReversedPlayerDeathTimerCoeff, Position.Y + _yMotion * G.ReversedPlayerDeathTimerCoeff);
            Rotation += _xMotion / 50 * G.ReversedPlayerDeathTimerCoeff;
            _yMotion += _gravity / 100;
        }
        else
            G.AfterPlayerDeadTimer += 0.016667f;
    }
}