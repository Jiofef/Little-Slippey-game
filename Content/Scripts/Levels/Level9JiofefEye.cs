using Godot;
using System;

public partial class Level9JiofefEye : Node2D
{
    Random _random = new Random();
    Vector2 _moveDirection;
	Sprite2D _sprite2D;
	public override void _Ready()
	{
		_moveDirection = new Vector2(_random.Next(2) == 1 ? 1 : -1, _random.Next(2) == 1 ? 1 : -1);
		_sprite2D = GetNode<Sprite2D>("Sprite2D");
	}

	public override void _PhysicsProcess(double delta)
	{
		_sprite2D.Rotate((float)_random.Next(-1, 2) / 50);

		Translate(_moveDirection * (Scale.X * 3 + G.Scores / 50));

		if (GlobalPosition.X >= (G.LevelXYSizes[G.CurrentLevel].X - 60 * Scale.X) || GlobalPosition.X < 60 * Scale.X)
			_moveDirection.X *= -1;
		if (GlobalPosition.Y >= (G.LevelXYSizes[G.CurrentLevel].Y - 60 * Scale.Y) || GlobalPosition.Y < 60 * Scale.Y)
			_moveDirection.Y *= -1;


		Scale -= new Vector2(0.001f, 0.001f);
		if (Scale.X < 0.5f)
		{
            _sprite2D.QueueFree();
            GetNode<CollisionShape2D>("Area2D/CollisionShape2D").Disabled = true;
            var explosionAnimation = GetNode<AnimatedSprite2D>("ExplosionAnimation");
			explosionAnimation.Visible = true;
			explosionAnimation.Play();
			SetPhysicsProcess(false);
		}
	}
}
