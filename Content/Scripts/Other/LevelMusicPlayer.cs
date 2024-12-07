using Godot;
using System;

public partial class LevelMusicPlayer : AudioStreamPlayer
{
    [Export] bool RestartMusicWhenItFinished = true, SaveTimeCodeWhenLevelResets = true, KeepPlayingWhenLevelResets = true;
    private string _currentMusicName;
    private float _startPosition = 0;

    public override void _Ready()
    {
        G.MusicPlayer = this;
        TreeExited += () =>
        {
            if (G.MusicPlayer == this)
                G.MusicPlayer = null;
            SaveTimeCode();
        };

        Connect("finished", new Callable(this, "OnMusicFinished"));

        if (KeepPlayingWhenLevelResets && G.MusicName != "")
            PlayMusic(G.MusicName, G.MusicStartPosition);
    }
    public void PlayMusic(string MusicName, float TrackStartPosition = 0, float AppearanceDuration = 0)
    {
        var musicAnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        if (MusicName != _currentMusicName)
        {
            Stream = ResourceLoader.Load<AudioStream>($"res://Content/Sounds/Soundtrack/{MusicName}.mp3");
            if (Stream == null)
            {
                GD.PrintErr($"Failed to load music: {MusicName}");
                return;
            }

            _currentMusicName = MusicName;
            _startPosition = TrackStartPosition;
            if (AppearanceDuration > 0)
                musicAnimationPlayer.Play("MusicStarting", -1, 1 / AppearanceDuration);
            else
                VolumeDb = 10;
            Play(SaveTimeCodeWhenLevelResets ? G.MusicStopTimeCode : 0);
            if (KeepPlayingWhenLevelResets)
            {
                G.MusicName = _currentMusicName;
                G.MusicStartPosition = _startPosition;
            }
        }
    }
    public void StopMusic(float DisappearanceDuration = 0)
    {
        var musicAnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        if (DisappearanceDuration > 0)
            musicAnimationPlayer.Play("MusicStopping", -1, 1 / DisappearanceDuration);
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
            Play(_startPosition);
    }

    //Activated when AnimationPlayer finishes animation
    private void MusicAnimationFinished(string animation)
    {
        if (animation == "MusicStopping")
        {
            Stream = null;
            Stop();
        }
    }

    private void SaveTimeCode()
    {
        if (SaveTimeCodeWhenLevelResets)
            G.MusicStopTimeCode = GetPlaybackPosition();
    }


    public void SetRestartMusicWhenItFinished(bool value)
    {
        RestartMusicWhenItFinished = value;
    }
    public void SetSaveTimeCodeWhenLevelResets(bool value)
    {
        SaveTimeCodeWhenLevelResets = value;
    }
    public void SetKeepPlayingWhenLevelResets(bool value)
    {
        KeepPlayingWhenLevelResets = value;
    }
    public void SetStartPosition(float value)
    {
        _startPosition = value;
        G.MusicStartPosition = _startPosition;
    }
}
