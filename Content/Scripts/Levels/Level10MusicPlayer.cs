using Godot;
using System;
using static Level10ScientistScript;
using static OtherExtension.ActionTools;


public partial class Level10MusicPlayer : AudioStreamPlayer
{
    public LevelState LevelState;
    private float _position => GetPlaybackPosition();
    private float[,] _partsTimeCodes = new float[,]
    {
        { 67.5f, 180 },
        { 265.35f, 384.5f },
        {384.5f, 395.1f },
        { 395.1f, 2730}
    };
    private int[] _partsRelatedScores = { 150, 290, 300, 999999999};

    public override void _Ready()
	{
        BindEventToNodeSafelyWithoutArgs(this, "StartPlaying", G.OnLevelStarted);

        LevelState = GetNode<Level10ScientistScript>("../Level/ScientistNode")._level;
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
        if (G.Scores > _partsRelatedScores[LevelState.CurrentMusicPart])
        {
            LevelState.CurrentMusicPart = LevelState.CurrentMusicPart + 1;
            Play(_partsTimeCodes[LevelState.CurrentMusicPart - 1, 1]);
        }
        if (_position >= _partsTimeCodes[LevelState.CurrentMusicPart, 1])
        {
            Play(_partsTimeCodes[LevelState.CurrentMusicPart, 0]);
        }
	}
}
