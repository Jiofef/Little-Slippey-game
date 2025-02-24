using Godot;
using System;

public partial class EnhancedArea2D : Area2D
{
	[Signal] public delegate void AreaEnteredNoArgsEventHandler();
    [Signal] public delegate void AreaExitedNoArgsEventHandler();
    [Signal] public delegate void BodyEnteredNoArgsEventHandler();
    [Signal] public delegate void BodyExitedNoArgsEventHandler();

    public override void _Ready()
	{
        AreaEntered += (junk) => EmitSignal(nameof(AreaEnteredNoArgs));
        AreaExited += (junk) => EmitSignal(nameof(AreaExitedNoArgs));
        BodyEntered += (junk) => EmitSignal(nameof(BodyEnteredNoArgs));
        BodyExited += (junk) => EmitSignal(nameof(BodyExitedNoArgs));
    }
}
