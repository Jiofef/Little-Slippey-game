using Godot;
using System;

public partial class Level10MusicPlayer : AudioStreamPlayer
{
    private float _position => GetPlaybackPosition();
    private float[,] _partsTimeCodes = new float[,]
    {
        { 67.5f, 180 },
        { 265.35f, 384.5f },
        {384.5f, 395.1f },
        { 395.1f, 2730}
    };
    private int[] _partsRelatedScores = { 150, 290, 300, 999999999};

    private G.LevelStartedEventHandler _onLevelStartedHandler;
    public override void _Ready()
	{
        _onLevelStartedHandler = (bool wasIntroShown) => StartPlaying();
        G.OnLevelStarted += _onLevelStartedHandler;
    }
    public override void _ExitTree()
    {
        if (_onLevelStartedHandler != null)
            G.OnLevelStarted -= _onLevelStartedHandler;
    }

    public void StartPlaying()
    {
        Stream = ResourceLoader.Load<AudioStream>("res://Content/Sounds/Soundtrack/Placebo Hope.mp3");
        Play(G.MusicStopTimeCode);
    }

    public void SaveTimeCode()
    {
        G.MusicStopTimeCode = _position;
    }

    public override void _PhysicsProcess(double delta)
	{
        if (G.Scores > _partsRelatedScores[(int)G.TransitiveVariant[5]])
        {
            G.TransitiveVariant[5] = (int)G.TransitiveVariant[5] + 1;
            Play(_partsTimeCodes[(int)G.TransitiveVariant[5] - 1, 1]);
        }
        if (_position >= _partsTimeCodes[(int)G.TransitiveVariant[5], 1])
        {
            Play(_partsTimeCodes[(int)G.TransitiveVariant[5], 0]);
        }
	}
}
