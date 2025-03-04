using Godot;

public partial class Trail : Line2D
{
    [Export] int MaxLength = 10;

    public override void _PhysicsProcess(double delta)
    {
        AddPoint(GetParent<Node2D>().GlobalPosition);

        if (GetPointCount() > MaxLength)
            RemovePoint(0);
    }
}
