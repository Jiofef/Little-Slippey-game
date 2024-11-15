using Godot;
using System;

public partial class ExplosionAnimation : AnimatedSprite2D
{

	public override void _Ready()
	{
		Draw += () => GlobalRotation = 0;
	}

}
