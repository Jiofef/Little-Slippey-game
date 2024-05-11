using Godot;
using System;

public partial class Level1AdditionalBrightnessAnimationPlayer : AnimationPlayer
{
	public override void _Ready()
	{
		if ((bool)G.TransitiveVariant[0])
		{
			G.TransitiveVariant[0] = "";
			Play("Brightening");
		}
		else GetNode("../..").QueueFree();
	}

}
