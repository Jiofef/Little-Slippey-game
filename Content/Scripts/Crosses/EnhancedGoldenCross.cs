using Godot;
using System;
using static OtherExtension.RandomTools;

public partial class EnhancedGoldenCross : UnusualCrossNode
{
    // This node is just a spawner and respawner of the proton-electron pair;

    public ElementaryParticle NegativePart, PositivePart;
    public byte FinishedParts = 0;
    public override void _Ready()
	{
        // Initializing nodes
        NegativePart = GetNode<ElementaryParticle>("NegativePart");
        PositivePart = GetNode<ElementaryParticle>("PositivePart");

		NegativePart.Reparent(GetParent());
		PositivePart.Reparent(GetParent());

        NegativePart.GlobalPosition = RandomVectorInCameraBorders();
        PositivePart.GlobalPosition = RandomVectorIn(G.CameraLimits, NegativePart.GlobalPosition, 500);

        NegativePart.ShouldBeSavedInPool = ShouldBeSavedInPool;
        PositivePart.ShouldBeSavedInPool = ShouldBeSavedInPool;

        NegativePart.Finished += PartFinished;
        PositivePart.Finished += PartFinished;

        // So as not to waste an extra two and a half operations per frame
        ProcessMode = ProcessModeEnum.Disabled;
    }

    public void PartFinished()
    {
        FinishedParts++;

        if (FinishedParts == 2)
            OnFinished();
    }

    public override void Respawn()
    {
        base.Respawn();

        FinishedParts = 0;

        NegativePart.GlobalPosition = RandomVectorInCameraBorders();
        PositivePart.GlobalPosition = RandomVectorIn(G.CameraLimits, NegativePart.GlobalPosition, 500);

        NegativePart.Respawn();
        PositivePart.Respawn();

        ProcessMode = ProcessModeEnum.Disabled;
    }
}
