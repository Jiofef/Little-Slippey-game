using Godot;
using System;

public partial class LevelMusicPlayer : AudioStreamPlayer
{
    [Export] bool RestartMusicWhenItFinished = true, SaveTimeCodeWhenLevelResets = true, SavePlayingWhenLevelResets = true;
    private string _currentMusicName;
    private float _trackRestartPosition = 0;

    public override void _Ready()
    {
        G.MusicPlayer = this;
        TreeExited += () =>
        {
            if (G.MusicPlayer == this)
                G.MusicPlayer = null;
            SaveTimeCode();
        };

        Connect("finished", new Callable(this, "MusicFinished"));

        if (SavePlayingWhenLevelResets && G.MusicName != "")
            PlayMusic(G.MusicName, G.MusicRestartPosition);
    }
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
            Play(SaveTimeCodeWhenLevelResets ? G.MusicStopTimeCode : 0);
            if (SavePlayingWhenLevelResets)
            {
                G.MusicName = _currentMusicName;
                G.MusicRestartPosition = _trackRestartPosition;
            }
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

    private void OnMusicFinished()
    {
        if (RestartMusicWhenItFinished)
            Play(_trackRestartPosition);
    }

    private void SaveTimeCode()
    {
        if (SaveTimeCodeWhenLevelResets)
            G.MusicStopTimeCode = GetPlaybackPosition();
    }
}
