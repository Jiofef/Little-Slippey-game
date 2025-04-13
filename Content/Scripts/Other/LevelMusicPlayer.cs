using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class LevelMusicPlayer : AudioStreamPlayer
{
	#region Export
	[ExportGroup("Main Settings")]
	[Export] private string _musicName;

	[Export] public string MusicFolder = "res://Content/Sounds/Soundtrack/";

	[ExportGroup("Music Restart")]
	[Export] public bool RestartMusicWhenItFinished = true;
	[Export]  public float RestartPosition = 0;

	[ExportGroup("Level Restart")]
    [Export] public bool SaveTimeCodeWhenLevelResets = true, KeepPlayingWhenLevelResets = true;
	
	[ExportGroup("Stages")]
	private StagesModeEnum _stagesMode = StagesModeEnum.NoStages;
	[Export] public StagesModeEnum StagesMode { get => _stagesMode; set => SetStagesMode(value);}
	public void SetStagesMode(StagesModeEnum mode)
	{
		if (mode != StagesModeEnum.NoStages && StagesMode == StagesModeEnum.NoStages)
		{
			if (mode is StagesModeEnum.ManualStages)
			    SetStage(0);
			else if (mode is StagesModeEnum.ScoresDependentStages)
				UpdateStageToCurrentScore();
		}

		_stagesMode = mode;
	}
	public enum StagesModeEnum 
	{
		NoStages, // Stages are off. When the track ends, it either stops or starts again depending on the settings
		ScoresDependentStages, // Stages in its standard form. The next stage begins when the specified number of points is reached.
		ManualStages // The stages are switched manually via the corresponding methods
	};
	[Export] public Vector2[] StagesTimeCodes = []; // In the vector, X is the second at which the stage starts when looping, and Y is the second at which the stage ends
	[Export] public float[] StagesAssociatedScores = [];  // If stages mode is ScoresDependentStages
	public int CurrentStage { get; private set;}

	#endregion
    
    private string _currentMusicName;

	public float Position => GetPlaybackPosition();

    public override void _EnterTree()
    {
        G.MusicPlayer = this;
    }
    public override void _Ready()
	{
        TreeExiting += () =>
        {
            if (G.MusicPlayer == this)
                G.MusicPlayer = null;
            SaveTimeCode();
        };

        Connect("finished", new Callable(this, "OnMusicFinished"));
		if (KeepPlayingWhenLevelResets && G.MusicName != "")
            PlayMusic(G.MusicName, G.MusicStartPosition);
		else if(_musicName != null)
			PlayMusic(_musicName, 0);
    }

	public override void _PhysicsProcess(double delta)
	{
		// The code here is only responsible for processing music stages. If there are none, the code does not need to be processed.
		if (StagesMode == StagesModeEnum.NoStages) return;

		// Transition to a new stage when the required number of scores is reached. Works only in ScoresDependentStages mode
        if (StagesMode == StagesModeEnum.ScoresDependentStages && G.Scores > StagesAssociatedScores[CurrentStage])
        {
            CurrentStage++;
            Play(StagesTimeCodes[CurrentStage - 1].Y);
        }

		// Looping the stage of music
        if (Position >= StagesTimeCodes[CurrentStage].Y)
        {
            Play(StagesTimeCodes[CurrentStage].X);
        }
	}

	#region Main methods
	private void PlayMusicBase(string MusicName, float TrackStartPosition = 0, float AppearanceDuration = 0)
    {
        var musicAnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        Stream = ResourceLoader.Load<AudioStream>($"{MusicFolder}{MusicName}.mp3");
        if (Stream == null)
        {
            GD.PrintErr($"Failed to load music: {MusicName}");
            return;
        }

        _currentMusicName = MusicName;
        RestartPosition = TrackStartPosition;
        if (AppearanceDuration > 0)
            musicAnimationPlayer.Play("MusicStarting", -1, 1 / AppearanceDuration);
        else
            VolumeDb = 10;
        if (KeepPlayingWhenLevelResets)
        {
            G.MusicName = _currentMusicName;
            G.MusicStartPosition = RestartPosition;
        }
    }

    public void StopMusic(float DisappearanceDuration, bool forgetMusicState)
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

        if (forgetMusicState)
        {
            G.ResetMusicVariables();
        }
    }
    public void StopMusic(float DisappearanceDuration)
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

        G.ResetMusicVariables();
    }
    public void StopMusic()
    {
        var musicAnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        musicAnimationPlayer.Stop();
        Stop();
        VolumeDb = -20;

        _currentMusicName = "";

        G.ResetMusicVariables();
    }

	public void PlayMusic(string MusicName, float TrackStartPosition = 0, float AppearanceDuration = 0)
    {
        if (MusicName != _currentMusicName)
        {
            PlayMusicBase(MusicName, TrackStartPosition, AppearanceDuration);
            Play(SaveTimeCodeWhenLevelResets ? G.MusicStopTimeCode : 0);
        }
    }
    public void PlayMusicFrom(float Position, string MusicName, float TrackStartPosition = 0, float AppearanceDuration = 0)
    {
        if (MusicName != _currentMusicName)
        {
            PlayMusicBase(MusicName, TrackStartPosition, AppearanceDuration);
            Play(Position);
        }
    }
	#endregion

	#region Stages methods
	public void SetStage(int stage)
	{
		if (stage < 0) 
			{GD.PushError("The stage must be equal or greater than 0"); return; }

		if (stage > StagesTimeCodes.Count()) 
			{GD.PushError($"The last stage's id is {StagesTimeCodes.Count() - 1} bro L:)"); return; }

		if (stage == 0)
			Play();

		else
			Play(StagesTimeCodes[stage - 1].Y);
	}
	public void UpdateStageToCurrentScore() // If stages mode is ScoresDependentStages
	{
		if (StagesMode != StagesModeEnum.ScoresDependentStages)
			return;
		
		int stage = 0;

		while (stage < StagesAssociatedScores.Length)
		{
			if (G.Scores < StagesAssociatedScores[stage])
				break;

			stage++;
		}

		CurrentStage = stage;
		SetStage(CurrentStage);
	}
	
	public void NextStage()
	{
		CurrentStage++;
		SetStage(CurrentStage);
	}
	public void PreviousStage()
	{
		CurrentStage--;
		SetStage(CurrentStage);
	}
	#endregion
    private void OnMusicFinished()
    {
        if (RestartMusicWhenItFinished)
            Play(RestartPosition);
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
        if (G.BlockSavingSomeValues) return;

        if (SaveTimeCodeWhenLevelResets)
            G.MusicStopTimeCode = GetPlaybackPosition();
    }

	#region Setters
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
        RestartPosition = value;
        G.MusicStartPosition = RestartPosition;
    }
    public void SetPosition()
    {
        GetPlaybackPosition();
    }

    public void SetMusicWithoutPlaying(string MusicName, float TrackStartPosition = 0, float AppearanceDuration = 0)
	{
        PlayMusic(MusicName, TrackStartPosition, AppearanceDuration);
        StreamPaused = true;
    }
	#endregion

}
