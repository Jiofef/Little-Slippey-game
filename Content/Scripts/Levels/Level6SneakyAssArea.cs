using Godot;
using System;

public partial class Level6SneakyAssArea : Area2D
{
    public void Entered()
    {
            if (!G.IsPlayerDead)
        G.IsProgressPaused = true;
    }
    public void Exited()
    {
        G.IsProgressPaused = false;
    }
}
