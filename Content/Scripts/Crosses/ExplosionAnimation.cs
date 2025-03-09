using Godot;
using System;

public partial class ExplosionAnimation : AnimatedSprite2D
{
	[Export] public bool DeleteWhenFinished = true;
	[Export] public bool HideWhenFinished = true;
	public override void _Ready()
	{
		Draw += () => GlobalRotation = 0;

		AnimationFinished += () => {
			if (DeleteWhenFinished)
				QueueFree();
			else if (HideWhenFinished)
				Hide();
		}
		;
	}
	

}
