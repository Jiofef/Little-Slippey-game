using Godot;
using System;

public partial class Level10MusicPlayer : AudioStreamPlayer
{
	public override void _Ready()
	{
        if (Stream != ResourceLoader.Load<AudioStream>("res://Content/Sounds/Soundtrack/Placebo Hope.mp3"))
        {
            VolumeDb = 10;
            Stream = ResourceLoader.Load<AudioStream>("res://Content/Sounds/Soundtrack/Placebo Hope.mp3");
                Play();
        }
    }

	public override void _PhysicsProcess(double delta)
	{

	}
}
