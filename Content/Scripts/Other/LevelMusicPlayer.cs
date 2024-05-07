using Godot;
using System;

public partial class LevelMusicPlayer : AudioStreamPlayer
{
    private string _currentMusicName;
    private float _trackRestartPosition = 0;

    public void PlayMusic(string MusicName, float TrackRestartPosition = 0, float StartingDuration = 0)
    {
        var musicAnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        if (MusicName != _currentMusicName)
        {
            Stream = ResourceLoader.Load<AudioStream>("res://Content/Sounds/Soundtrack/" + MusicName + ".mp3");
            _currentMusicName = MusicName;
            _trackRestartPosition = TrackRestartPosition;
            if (StartingDuration > 0)
                musicAnimationPlayer.Play("MusicStarting", -1, 1 / StartingDuration);
            else
                VolumeDb = 10;
            Play();
        }
    }

    public void StopMusic(float StoppingDuration = 0)
    {
        var musicAnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        if (StoppingDuration > 0)
            musicAnimationPlayer.Play("MusicStopping", -1, 1 / StoppingDuration);
        else
        {
            musicAnimationPlayer.Stop();
            Stop();
            VolumeDb = -20;
        }
        _currentMusicName = "";
    }

    public void MusicFinished()
    {
        Play(_trackRestartPosition);
    }
}
