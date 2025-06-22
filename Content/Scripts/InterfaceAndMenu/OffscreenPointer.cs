using Godot;
using System;
using System.Reflection.Metadata;
using static Godot.Mathf;

public partial class OffscreenPointer : Node2D
{
	// Nodes
	public Node2D PointerSprite;

	public bool IsHidden = false;

	public override void _Ready()
	{
		PointerSprite = GetNode<Node2D>("Pointer");
	}

    public override void _PhysicsProcess(double delta)
    {
		var canvas = GetCanvasTransform();
		Vector2 topLeft = -canvas.Origin / canvas.Scale;
		Vector2 size = GetViewportRect().Size / canvas.Scale;

		UpdatePointerPosition(new Rect2(topLeft, size));
		UpdatePointerRotation();
    }

	public Vector2? TargetPosition = null;
    public void UpdatePointerPosition(Rect2 bounds)
	{
        Vector2 newPos = new Vector2();
        newPos.X = Clamp(GlobalPosition.X, bounds.Position.X, bounds.End.X);
        newPos.Y = Clamp(GlobalPosition.Y, bounds.Position.Y, bounds.End.Y);
        PointerSprite.GlobalPosition = newPos;

		if (bounds.HasPoint(GlobalPosition) || IsHidden)
			Hide();
		else
			Show();
    }

	public void UpdatePointerRotation()
	{
		float newAngle = (GlobalPosition - PointerSprite.GlobalPosition).Angle();
		PointerSprite.GlobalRotation = newAngle;
	}
}
