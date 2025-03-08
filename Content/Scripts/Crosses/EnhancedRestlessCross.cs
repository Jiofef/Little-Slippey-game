using Godot;
using System;

public partial class EnhancedRestlessCross : RestlessCross
{
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        if (TicksLived >= TICKS_TO_APPEAR && TicksLived < TICKS_TO_EXPLOSION)
        {
            Vector2 playerPos = G.Player.GlobalPosition;
            float rotationValue = GetAngleTo(playerPos) * 3;
            rotationValue = Mathf.Clamp(rotationValue, -1.5f, 1.5f);

            if (_shouldRotate)
                R.InitialRotation = rotationValue;
            else
                Rotation += rotationValue;

            var pointingRect = GetNode<Node2D>("PointingRect");
            float targetAngle = pointingRect.GetAngleTo(playerPos);
            pointingRect.Rotation = Mathf.LerpAngle(pointingRect.Rotation, targetAngle, 0.033f);
            pointingRect.Rotation = Mathf.Clamp(pointingRect.Rotation, Mathf.DegToRad(-70), Mathf.DegToRad(70));
        }
    }
}