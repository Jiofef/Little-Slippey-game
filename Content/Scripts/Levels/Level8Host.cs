using Godot;
using System;

public partial class Level8Host : AnimatedSprite2D
{
	CharacterBody2D _player;
	public override void _Ready()
	{
		_player = GetNode<CharacterBody2D>("../../Player");
	}

	public override void _PhysicsProcess(double delta)
	{
		SpeedScale = 1 / (new Vector2(GlobalPosition.X + 220, GlobalPosition.Y).DistanceTo(_player.GlobalPosition) / 720);
		if (SpeedScale < 0.5f)
			SpeedScale = 0.5f;
		else if (SpeedScale > 25)
			SpeedScale = 25;

        Position = new Vector2(Position.X, Position.Y + (_player.GlobalPosition.Y - GlobalPosition.Y) / 100);

		Rotate(GetAngleTo(_player.GlobalPosition) / 50);
	}
}
