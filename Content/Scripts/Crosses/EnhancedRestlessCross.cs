using Godot;
using System;

public partial class EnhancedRestlessCross : RestlessCross
{
    public Node2D PointingRect;
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        if (TicksLived < TICKS_TO_EXPLOSION && G.Player != null)
        {
            Vector2 playerPos = G.Player.GlobalPosition;
            float rotationValue = GetAngleTo(playerPos) * 3;
            rotationValue = Mathf.Clamp(rotationValue, -1.5f, 1.5f);

            if (TicksLived < TICKS_TO_APPEAR)
                R.InitialRotation += rotationValue;
            else
                RotationDegrees += rotationValue;

            float targetAngle = PointingRect.GetAngleTo(playerPos);
            PointingRect.Rotation = Mathf.LerpAngle(PointingRect.Rotation, targetAngle, 0.033f);
            PointingRect.RotationDegrees = Mathf.Clamp(PointingRect.RotationDegrees, -70, 70);
        }
    }

    public override void _Ready()
    {
        base._Ready();

        PointingRect = GetNode<Node2D>("PointingRect");
        Exploded += PointingRect.Hide;
    }

    public override void Respawn()
    {
        base.Respawn();

        PointingRect.Show();
        PointingRect.Rotation = 0;
    }
}