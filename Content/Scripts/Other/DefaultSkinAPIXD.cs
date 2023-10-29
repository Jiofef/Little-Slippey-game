using Godot;
using System;

public partial class DefaultSkinAPIXD : AnimatedSprite2D
{
	[Signal] public delegate void PlayerDeathEventHandler();
	public override void _Ready()
	{
		GetParent().Connect("PlayerDied", new Callable(this, "PlayerDied"));
	}
	public void PlayerDied()
	{
		EmitSignal("PlayerDeath");
    }
}
