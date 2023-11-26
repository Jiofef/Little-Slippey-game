using Godot;

public partial class SkinScript : AnimatedSprite2D
{
	[Export] bool _enableSecondJumpAndFallFrame = false;

    public override void _Ready()
	{
		SetPhysicsProcess(_enableSecondJumpAndFallFrame || Meta.Instance.ChosenSkinIndex == 6);
	}
    public override void _PhysicsProcess(double delta)
    {
		if (_enableSecondJumpAndFallFrame && (Animation == "Fall" || Animation == "Jump"))
			Frame = Input.IsActionPressed("ui_right") || Input.IsActionPressed("ui_left") ? 1 : 0;

		if (Meta.Instance.ChosenSkinIndex == 6)
		{
			var zParticles = GetNode<CpuParticles2D>("ZParticles");
			if (Animation == "Idle" && Frame == 2 && !zParticles.Emitting)
				zParticles.Emitting = true;
			else if (zParticles.Emitting && Animation != "Sleep")
				zParticles.Emitting = false;
		}
    }

	public void SetGlobalRotationDegrees(float value)
	{
		GetParent<Node2D>().GlobalRotationDegrees = value;
	}
}
