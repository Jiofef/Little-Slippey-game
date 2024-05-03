using Godot;
using System;

public partial class Level10MusicController : Node
{
	AudioStreamPlayer MusicPlayer;
	public override void _Ready()
	{
		MusicPlayer = GetNode<AudioStreamPlayer>("../../../LevelMusicPlayer");
		if (MusicPlayer.Stream != ResourceLoader.Load<AudioStream>("res://Content/Sounds/Soundtrack/Hopes And Dreams.mp3"))
		{
            MusicPlayer.VolumeDb = 10;
            MusicPlayer.Stream = ResourceLoader.Load<AudioStream>("res://Content/Sounds/Soundtrack/Hopes And Dreams.mp3");
            MusicPlayer.Play();
        }
    }
}
