using Godot;
using System;

public partial class UpdaterTimer : Timer
{
    [Export] public Node NodeToUpdate;
    [Export] public string PropertyToUpdate;
    [Export] public float ValueMultiplier = 1, ValueCorrection = 0;
    [Export] public bool UpdateDeferred = false;
    [Export] public bool ProcessStoppedWhenFinished = false;

    public override void _Ready()
    {
        Timeout += () =>
        {
            if (ProcessStoppedWhenFinished)
                SetProcess(false);
        };

        if (ProcessStoppedWhenFinished && !Autostart)
            SetProcess(false);
    }

    public override void _Process(double delta)
    {
        if (IsStopped() || NodeToUpdate == null || PropertyToUpdate == null) return;

        if (UpdateDeferred)
            NodeToUpdate.SetDeferred(PropertyToUpdate, TimeLeft * ValueMultiplier + ValueCorrection);
        else
            NodeToUpdate.Set(PropertyToUpdate, TimeLeft * ValueMultiplier + ValueCorrection);
    }
}
