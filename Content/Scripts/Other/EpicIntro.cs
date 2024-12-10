using Godot;
using System;

public partial class EpicIntro : CanvasLayer
{
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
