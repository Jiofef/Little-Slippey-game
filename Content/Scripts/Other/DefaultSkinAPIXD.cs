using Godot;
using System;

public partial class DefaultSkinAPIXD : AnimatedSprite2D
{
	[Signal] public delegate void PlayerDeathEventHandler();
	[Export] bool _enableSecondJumpAndFallFrame = false;
	public override void _Ready()
	{
		GetNode("../..").Connect("PlayerDied", new Callable(this, "PlayerDied"));
		SetPhysicsProcess(_enableSecondJumpAndFallFrame || Meta.Instance.ChosenSkinIndex == 5);
	}
    public override void _PhysicsProcess(double delta)
    {
		if (_enableSecondJumpAndFallFrame && (Animation == "Fall" || Animation == "Jump"))
			Frame = Input.IsActionPressed("ui_right") || Input.IsActionPressed("ui_left") ? 1 : 0;

		if (Meta.Instance.ChosenSkinIndex == 5)
		{
			var zParticles = GetNode<CpuParticles2D>("ZParticles");
			if (Animation == "Idle" && Frame == 2 && !zParticles.Emitting)
				zParticles.Emitting = true;
			else if (zParticles.Emitting && Animation != "Sleep")
				zParticles.Emitting = false;
		}
    }
    public void PlayerDied()
	{
		EmitSignal("PlayerDeath");
    }
	public void SetGlobalRotationDegrees(float value)
	{
		GetParent<Node2D>().GlobalRotationDegrees = value;
	}
}
