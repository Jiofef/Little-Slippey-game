using Godot;
using System;

public partial class DefaultSkinAPIXD : AnimatedSprite2D
{
	[Signal] public delegate void PlayerDeathEventHandler();
	[Export] bool _enableSecondJumpAndFallFrame = false;
	public override void _Ready()
	{
		GetParent().Connect("PlayerDied", new Callable(this, "PlayerDied"));
		SetPhysicsProcess(_enableSecondJumpAndFallFrame);
	}
    public override void _PhysicsProcess(double delta)
    {
		if (_enableSecondJumpAndFallFrame && (Animation == "Fall" || Animation == "Jump"))
			Frame = Input.IsActionPressed("ui_right") || Input.IsActionPressed("ui_left") ? 1 : 0;
			
    }
    public void PlayerDied()
	{
		EmitSignal("PlayerDeath");
    }
}
