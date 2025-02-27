using Godot;
using System;

public partial class BonusSpawner : Node2D
{
	[Export] public PackedScene PossibleBonuses { get; set; }

	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
