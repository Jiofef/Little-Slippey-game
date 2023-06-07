using Godot;
using System;

public partial class ForegroundScript : Node2D
{
	private bool _doHideForeground = false;

    public override void _PhysicsProcess(double delta)
    {
        if (!_doHideForeground && Modulate.A < 0.9f)
            Modulate = new Color(Modulate.R, Modulate.G, Modulate.B, Modulate.A + 0.04f);
        else if (_doHideForeground && Modulate.A > 0.3f)
            Modulate = new Color(Modulate.R, Modulate.G, Modulate.B, Modulate.A - 0.04f);
    }
    public void Hide(Node2D junk)
	{
        _doHideForeground = true;
        SetPlayerCollidingWithFlyingCars(false);

    }
    public void Show(Node2D junk)
    {
        _doHideForeground = false;
        SetPlayerCollidingWithFlyingCars(true);
    }

    private void SetPlayerCollidingWithFlyingCars(bool value)
    {
        GetNode<CharacterBody2D>("../Player").SetCollisionMaskValue(8, value);
        GetNode<Area2D>("../Player/Areas/WallDirectionLeftDetector").SetCollisionMaskValue(8, value);
        GetNode<Area2D>("../Player/Areas/WallDirectionRightDetector").SetCollisionMaskValue(8, value);
    }
}