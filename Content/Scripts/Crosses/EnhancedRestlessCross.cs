using Godot;
using System;

public partial class EnhancedRestlessCross : RestlessCross
{
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        if (_ticksToExplosion > 0)
        {
            Vector2 playerPos = G.Player.GlobalPosition;
            float RotationValue = GetAngleTo(playerPos) * 3;

            RotationValue = Mathf.Clamp(RotationValue, -1.5f, 1.5f);
            GD.Print(_defaultRotation);
            if (_ticksToAppear > 0)
                _defaultRotation += RotationValue;
            else 
                RotationDegrees += RotationValue;


            var pointingRect = GetNode<Node2D>("PointingRect");
            pointingRect.Rotation += pointingRect.GetAngleTo(playerPos) / 30;
            pointingRect.RotationDegrees = Mathf.Clamp(pointingRect.RotationDegrees, -70, 70);
        }
    }
}
