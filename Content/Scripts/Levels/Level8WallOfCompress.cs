using Godot;
using System;

public partial class Level8WallOfCompress : Node2D
{
	CharacterBody2D _player;

    private float _wallDefaultSpeed = 3, _wallSpeed;
	public override void _Ready()
	{
		_player = GetNode<CharacterBody2D>("../Player");
	}

	public override void _PhysicsProcess(double delta)
	{
		_wallSpeed = _wallDefaultSpeed + (_player.Position.X - Position.X) / 1920 * 4;
		Position = new Vector2(Position.X + _wallSpeed, Position.Y);
	}
}