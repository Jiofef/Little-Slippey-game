using Godot;

public partial class Level6FlyingCar : AnimatableBody2D
{
	private int _speed;
	private bool _isInForeground = false;
    public override void _Ready()
    {
		_speed = 5 * (int)Scale.X;
    }
    public override void _PhysicsProcess(double delta)
	{
		Position = new Vector2(Position.X + _speed, Position.Y);
		if (Position.X > 2705 || Position.X < -145)
			QueueFree();

        if (!_isInForeground && Modulate.A < 1)
            Modulate = new Color(Modulate.R, Modulate.G, Modulate.B, Modulate.A + 0.1f);
        else if (_isInForeground && Modulate.A > 0.2f)
            Modulate = new Color(Modulate.R, Modulate.G, Modulate.B, Modulate.A - 0.1f);
    }

	public void AreaEntered(Area2D area)
	{
		if (area.Name == "CarArea" || area.Name == "PlayerDamageDetector")
		{
			QueueFree();
		}
		else if (area.Name == "ForegroundArea")
		{
			_isInForeground = true;
            GetNode<CollisionShape2D>("CarArea/CollisionShape2D").SetDeferred("disabled", true);
        }
	}

    public void AreaExited(Area2D area)
	{
		if (area.Name == "ForegroundArea")
		{
            _isInForeground = false;
			GetNode<Area2D>("CarArea").SetCollisionMaskValue(4, true);
            GetNode<CollisionShape2D>("CarArea/CollisionShape2D").SetDeferred("disabled", false);
        }
	}
}