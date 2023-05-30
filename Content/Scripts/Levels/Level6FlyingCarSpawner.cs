using Godot;
using System;

public partial class Level6FlyingCarSpawner : Node2D
{
	PackedScene _flyingCar = new PackedScene();
	Random _random = new Random();
	public override void _Ready()
	{
		_flyingCar = ResourceLoader.Load<PackedScene>("res://Content/Scenes/Other/Level6FlyingCar.tscn");
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_random.Next(10) == 0)
		{
			AnimatableBody2D flyingCar = _flyingCar.Instantiate<AnimatableBody2D>();
            flyingCar.Scale = new Vector2(_random.Next(100) >= 50 ? 1 : -1, 1);
            flyingCar.Position = flyingCar.Scale.X == -1 ? new Vector2(2700, _random.Next(1280)) : new Vector2(-140, _random.Next(1280));
			AddChild(flyingCar);
        }
	}
}
