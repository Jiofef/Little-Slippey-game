using Godot;
using System;

public partial class SeamlessBoundaryController : Node
{
	[Export] public Node2D NodeToKeepInBorders;
	[Export] public bool TeleportPlayerWithoutGlitchEffect = false;
	[Export] public Rect2 GlobalBorders2D = new Rect2(0, 0, 1280, 640);
	//[Export] public float 

	public override void _PhysicsProcess(double delta)
	{
		if (NodeToKeepInBorders == null) return;

		if (!GlobalBorders2D.HasPoint(NodeToKeepInBorders.GlobalPosition))
		{
			Vector2 nodePos = NodeToKeepInBorders.GlobalPosition;

			Vector2 offsetPos = Vector2.Zero;

			if (nodePos.X > GlobalBorders2D.End.X)
				offsetPos.X = -GlobalBorders2D.Size.X;
			else if (nodePos.X < GlobalBorders2D.Position.X)
				offsetPos.X = GlobalBorders2D.Size.X;
			
			if (nodePos.Y > GlobalBorders2D.End.Y)
				offsetPos.Y = -GlobalBorders2D.Size.Y;
			else if (nodePos.Y < GlobalBorders2D.Position.Y)
				offsetPos.Y = GlobalBorders2D.Size.Y;
			
			if (NodeToKeepInBorders is Player player && G.Main != null)
				G.Main.TeleportPlayerTo(player.GlobalPosition + offsetPos, TeleportPlayerWithoutGlitchEffect);
			else
				NodeToKeepInBorders.GlobalPosition += offsetPos;
		}
	}
}
