using Godot;
using System;

public partial class SlippeyChadCorpseMotionMultiplierCrutch : Node
{
	[Signal] public delegate void SetCorpseMotionMultiplierXEventHandler();
    public override void _Ready()
    {
        Connect("SetCorpseMotionMultiplierX", new Callable(GetNode("../../.."), "SetCorpseMotionMultiplierX"));
    }
    public override void _PhysicsProcess(double delta)
	{
        if (G.IsPlayerDead)
		    EmitSignal("SetCorpseMotionMultiplierX", 0.25f);
	}
}
