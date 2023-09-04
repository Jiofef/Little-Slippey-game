using Godot;

public partial class Level9Script : Node
{
	public override void _Ready()
	{
		if (!G.IsLevel9PlatformSectionSkips)
		{

			GetNode<CharacterBody2D>("../Player").GlobalPosition = new Vector2(-13184, 256);
			GetNode<TileMap>("../TileMap/DoomDoor").QueueFree();
		}
	}

	public override void _PhysicsProcess(double delta)
	{
	}
}
