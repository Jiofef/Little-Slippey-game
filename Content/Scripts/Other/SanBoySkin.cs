using Godot;
using System;

public partial class SanBoySkin : AnimatedSprite2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GetParent().Connect("PlayerDied", new Callable(this, "PlayerDied"));
	}
	public void PlayerDied()
	{
		GetNode<AnimationPlayer>("AnimationPlayer").Play("Death");
    }
}
