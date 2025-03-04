using Godot;

public partial class Rotator : Node2D
{
    [Export] public float RotationSpeed = 0.1f;
    [Export] public Node2D Target;

    [Signal]
    public delegate void CirclePassedEventHandler();

    public override void _PhysicsProcess(double delta)
    {
        if (Target == null) return;

        Target.Rotation += RotationSpeed;
        if (Target.RotationDegrees > 360)
        {
            Target.Rotation = 0;
            EmitSignal("CirclePassed");
        }
    }
}
