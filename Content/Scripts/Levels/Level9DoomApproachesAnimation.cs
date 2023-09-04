using Godot;

public partial class Level9DoomApproachesAnimation : AnimatedSprite2D
{
    public override void _Ready()
    {
        SetPhysicsProcess(false);
    }
    public override void _PhysicsProcess(double delta)
	{
        Modulate = new Color(Modulate.R, Modulate.G, Modulate.B, 0.25f - Mathf.Abs(128 + -GetNode<CharacterBody2D>("../../../Player").Position.X) / 15000);
	}
}
