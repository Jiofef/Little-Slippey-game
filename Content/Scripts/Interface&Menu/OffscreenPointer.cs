using Godot;
using System;
using System.Reflection.Metadata;
using static Godot.Mathf;

public partial class OffscreenPointer : Node2D
{
	// Nodes
	public Sprite2D PointerSprite;

	public override void _Ready()
	{
		PointerSprite = GetNode<Sprite2D>("Pointer");
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
		if (TargetPosition == null)
		{
            Vector2 newPos = new Vector2();
            newPos.X = Clamp(GlobalPosition.X, bounds.Position.X, bounds.End.X);
            newPos.Y = Clamp(GlobalPosition.Y, bounds.Position.Y, bounds.End.Y);
            PointerSprite.GlobalPosition = newPos;
        }
		else
		{
			Vector2 targetPosition = (Vector2)TargetPosition;
			Vector2 displacement = GlobalPosition - targetPosition;
			float length;

			float topLeft = (bounds.Position - targetPosition).Angle();
			float topRight = (new Vector2(bounds.End.X, bounds.Position.Y) - targetPosition).Angle();
			float bottomLeft = (new Vector2(bounds.Position.X, bounds.End.Y) - targetPosition).Angle();
			float bottomRight = (bounds.End - targetPosition).Angle();

			float displacementAngle = displacement.Angle();
            if ((displacementAngle > topLeft  && displacementAngle < topRight)
			||  (displacementAngle < bottomLeft && displacementAngle > bottomRight))
			{
				float yLength = Clamp(displacement.Y, bounds.Position.Y - targetPosition.Y, bounds.End.Y - targetPosition.Y);

				float angle = displacementAngle - Pi / 2;
				length = Cos(angle) != 0 ? yLength / Cos(angle) : yLength;
			}
			else
			{
                float xLength = Clamp(displacement.X, bounds.Position.X - targetPosition.X, bounds.End.X - targetPosition.X);

                length = Cos(displacementAngle) != 0 ? xLength / Cos(displacementAngle) : xLength;
            }

			PointerSprite.GlobalPosition = new Vector2(length, 0).Rotated(displacementAngle) + targetPosition;
        }

		if (bounds.HasPoint(GlobalPosition))
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
