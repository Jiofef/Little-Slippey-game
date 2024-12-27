using Godot;
using System;

public partial class EpicIntro : CanvasLayer
{
    public override void _Ready()
    {
        if (!ModManager.IsStandartTimerLibLoaded && !G.DidLevelIntroPassed)
        {
            Random random = new Random();
            if (random.Next(3) == 0)
            {
                QueueFree();
                var rootIntro = (RootIntro)GD.Load<PackedScene>("res://Content/Scenes/Other/RootIntro.tscn").Instantiate();
                GetParent().CallDeferred("add_child", rootIntro);

            }

        }
    }
    public void OnIntroPassed()
    {
        G.DidLevelIntroPassed = true;
        G.Main?.OnIntroFinished();
        G.MusicPlayer?.Play();
    }

    private void Disappear()
    {
        QueueFree();
    }
}
