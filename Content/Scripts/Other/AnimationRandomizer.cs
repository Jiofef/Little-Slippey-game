using Godot;
using System;

public partial class AnimationRandomizer : AnimationPlayer
{
	string[] _animationsArray;
	public override void _Ready()
	{
		_animationsArray = GetAnimationList();
		Connect("animation_finished", new Callable(this, "ChangeAnimation"));
	}

	public void ChangeAnimation(string junk)
	{
		Random random = new Random();
		CurrentAnimation = _animationsArray[random.Next(_animationsArray.Length)];
	}
}
