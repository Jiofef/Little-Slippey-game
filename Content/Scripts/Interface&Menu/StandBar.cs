using Godot;
using System;

public partial class StandBar : TextureProgressBar
{
    Random _random = new Random();


    public override void _Ready()
    {
        SetPhysicsProcess(Visible);

        VisibilityChanged += () =>
        {
            SetPhysicsProcess(Visible);
        };
    }

    float _standCoeff, _standCubicCoeff;
    public override void _PhysicsProcess(double delta)
    {
        _standCoeff = 1 - G.PlayerMoveCoeff;
        _standCubicCoeff = _standCoeff * _standCoeff * _standCoeff;
        Value = _standCoeff;
        Position = new Vector2(0, 164) + new Vector2(_random.NextSingle() - 0.5f, _random.NextSingle() - 0.5f) * 3 * _standCubicCoeff;
        Modulate = new Color(1, 1 - _standCubicCoeff / 2, 1 - _standCubicCoeff / 2);
    }
}
