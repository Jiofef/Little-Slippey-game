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
    public override void _PhysicsProcess(double delta)
    {
        if (G.PlayerCorpseFlightTimer != 4.5f)
        {
            G.PlayerCorpseFlightTimer = G.PlayerCorpseFlightTimer < 4.5f ? G.PlayerCorpseFlightTimer + 0.016667f : 4.5f;
            Position = new Vector2(Position.X + _xMotion * G.ReversedPlayerCorpseFlightTimerCoeff, Position.Y + _yMotion * G.ReversedPlayerCorpseFlightTimerCoeff);
            Rotation += _xMotion / 50 * G.ReversedPlayerCorpseFlightTimerCoeff;
            _yMotion += _gravity / 100;
        }
        else
            G.AfterPlayerCorpseFlightTimer += 0.016667f;
    }
    public void Activate()
    {
        Random random = new Random();
        _xMotion = random.Next(100) > 50 ? -5 * (GlobalPosition.X / G.LevelXYSizes[G.CurrentLevel].X) : 5 * (1 - GlobalPosition.X / G.LevelXYSizes[G.CurrentLevel].X);

        if(G.CurrentLevel == 8)
            _xMotion = random.Next(100) > 50 ? -5 : +5;

        _yMotion = -8;


        GetNode<AudioStreamPlayer>("DeathSound").Play();
        EmitSignal("Death");
        SetPhysicsProcess(true);
    }
}