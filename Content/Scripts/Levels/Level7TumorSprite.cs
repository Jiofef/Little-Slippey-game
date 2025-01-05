using Godot;
using System;
using System.Linq;
using System.Runtime.CompilerServices;

public partial class Level7TumorSprite : Node2D
{
    Random random = new Random();
    private const int HOLE_RADIUS = 35;

    public override void _Process(double delta)
    {
        QueueRedraw();
    }
}
