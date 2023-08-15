using Godot;

public partial class Level8FragileTile : AnimatableBody2D
{
	[Export] float TimeToFall = 1;

	private float _fallSpeed = 0;

    public override void _Ready()
	{
		SetPhysicsProcess(false);
	}

	public override void _PhysicsProcess(double delta)
	{
		if (TimeToFall > 0)
		{
			TimeToFall -= 0.016667f;

			if (TimeToFall <= 0)
			{
                GetNode<CpuParticles2D>("Particles2").Emitting = true;
            }
		}
		else if (GlobalPosition.Y < 3000)
		{
			Position = new Vector2(Position.X, Position.Y + _fallSpeed);
            _fallSpeed += 0.075f;
		}
	}

	public void FallingStart()
	{
		SetPhysicsProcess(true);
		GetNode<CpuParticles2D>("Particles").Emitting = true;
	}
}
