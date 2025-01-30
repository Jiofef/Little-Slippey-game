using Godot;
using System;
using static OtherExtension.RandomTools;

public partial class EnhancedGoldenCross : Node2D
{
    // This node is just a container for the proton-electron pair, so it can simply be removed.
    public override void _Ready()
	{
        // Initializing nodes
        Node2D negativePart = GetNode<Node2D>("NegativePart");
        Node2D positivePart = GetNode<Node2D>("PositivePart");

		negativePart.Reparent(GetParent());
		positivePart.Reparent(GetParent());

        negativePart.GlobalPosition = RandomVectorInCameraBorders();
        positivePart.GlobalPosition = RandomVectorIn(G.CameraLimits, negativePart.GlobalPosition, 500);


		QueueFree();
    }
}
