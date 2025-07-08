using Godot;
using System;
using System.Net.Mail;

public partial class FattyAddition : AdditionNode
{
	public override void Activate()
	{
		if (G.Player == null) return;

		G.Player.Scale = G.Player.Scale with { X = G.Player.Scale.X * 2.5f };
	}
}
