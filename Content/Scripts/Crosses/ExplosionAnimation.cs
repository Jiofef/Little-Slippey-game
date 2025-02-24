using Godot;
using System;

public partial class ExplosionAnimation : AnimatedSprite2D
{
	[Export] public bool DeleteWhenFinished = true;
	public override void _Ready()
	{
		Draw += () => GlobalRotation = 0;

		AnimationFinished += () => {
			if (DeleteWhenFinished)
				QueueFree();
		}
		;
	}
	

}
