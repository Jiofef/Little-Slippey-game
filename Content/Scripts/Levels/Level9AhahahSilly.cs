using Godot;
using System;

public partial class Level9AhahahSilly : VideoStreamPlayer
{
    public void Showed()
    {
        GetTree().Root.GetNode<AudioStreamPlayer>("Main/LevelMusicPlayer").Stop();
        GetTree().Paused = true;
    }
}
