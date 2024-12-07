using Godot;
using System;

public partial class SleepySkin : SkinScript
{
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        var zParticles = GetNode<CpuParticles2D>("ZParticles");
        if (Animation == "Idle" && Frame == 2 && !zParticles.Emitting)
            zParticles.Emitting = true;
        else if (zParticles.Emitting && Animation != "Sleep")
            zParticles.Emitting = false;
    }
}
