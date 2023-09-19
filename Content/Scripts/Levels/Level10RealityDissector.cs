using Godot;
using System;

public partial class Level10RealityDissector : Node
{
	CharacterBody2D _player;
	public override void _Ready()
	{
		G.CrossSpawnMultiplier = 0.25f;
		G.IsProgressPaused = true;
		_player = GetNode<CharacterBody2D>("../Player");
    }


	public override void _PhysicsProcess(double delta)
	{
        G.Scores += 0.016667f;
        if (G.Scores > 10)
			G.Scores = 1;
	}
}
