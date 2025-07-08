using Godot;
using System;
using System.Net.Mail;

public partial class TechAGravAddition : AdditionNode
{
	public override void Activate()
	{
		if (G.Player == null) return;

		G.Player.RotationDegrees = 180;
		G.Player.Camera.IgnoreRotation = false;
		G.Player.UpDirection *= -1;
	}
}
