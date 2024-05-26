using Godot;
using System;

public partial class Level6SneakyAssArea : Area2D
{
    public void Entered()
    {
        G.IsProgressPaused = true;
    }
    public void Exited()
    {
        G.IsProgressPaused = false;
    }
}
