using Godot;

public partial class SkinScript : AnimatedSprite2D
{
	[Export] public bool HasAnimationAnalogues = false, HasDeathAnimation = false, HasSideJumpAndFallFrame = false;

    public override void _Ready()
	{
		SetPhysicsProcess(HasSideJumpAndFallFrame);
	}
    public override void _PhysicsProcess(double delta)
    {
		if (HasSideJumpAndFallFrame && (Animation == "Fall" || Animation == "Jump"))
			Frame = Input.IsActionPressed("ui_right") || Input.IsActionPressed("ui_left") ? 1 : 0;
    }

	new public void SetGlobalRotationDegrees(float value)
	{
		GetParent<Node2D>().GlobalRotationDegrees = value;
	}
}
