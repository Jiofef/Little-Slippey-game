using Godot;
using System;
using System.Net.Mail;

public partial class ThunderStormAddition : AdditionNode
{
	public override void Activate()
	{
		AddToLevel((Node2D)ResourceLoader.Load<PackedScene>("res://Content/Scenes/Other/Level5Rain.tscn").Instantiate());
	}
}
