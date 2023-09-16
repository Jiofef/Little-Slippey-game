using Godot;
using System;

public partial class Level10RealityDissector : Node
{
	CharacterBody2D _player;
	public override void _Ready()
	{
		G.CrossSpawnMultiplier = 0.25f;
		_player = GetNode<CharacterBody2D>("../Player");
    }


	public override void _PhysicsProcess(double delta)
	{
		if (G.Scores > 10)
			G.Scores = 1;
	}
}
