using Godot;

public partial class MainScript : Node2D
{
    [Signal] public delegate void RecalculateCrossWeightEventHandler();
    [Signal] public delegate void LevelResetingEventHandler();
    private bool _subMenusOpened, _isPauseDisabled = false, _isResetDisabled;
    TextureButton _rewindButton;
    AudioStreamPlayer _levelMusicPlayer;

    public override void _Ready()
    {
        GetTree().Paused = false;

        AudioServer.SetBusEffectEnabled(2, 0, false);
        AudioServer.SetBusEffectEnabled(6, 0, false);

        G.CurrentPopupAchievementsLayer = GetNode<CanvasLayer>("PopupAchievementsLayer");
        _rewindButton = GetNode<TextureButton>("Pause/Interface/ButtonsFrame/Rewind");
        _levelMusicPlayer = GetNode<AudioStreamPlayer>("LevelMusicPlayer");

        Connect("RecalculateCrossWeight", new Callable(GetNode("Level"), "RecalculateCrossWeight"));

        if (G.DidLevelIntroPassed)
        {
            GetNode<CanvasLayer>("EpicIntro").QueueFree();
            GetNode<Node2D>("Level").ProcessMode = ProcessModeEnum.Pausable;
        }
        if (G.IsLevelVanilla && UnchangableMeta.LevelPlayedStatus[G.CurrentLevel - 1] != 1)
        {
            UnchangableMeta.LevelPlayedStatus[G.CurrentLevel - 1] = 1;
            UnchangableMeta.SaveToFile();
        }

        SetProcess(false);
    }
    public override void _PhysicsProcess(double delta)
    {
        if (Input.IsActionJustPressed("Cancel") && !_subMenusOpened && G.DidLevelIntroPassed && !_isPauseDisabled)
            UnPause();
        if (_rewindButton.ButtonPressed && !_isResetDisabled)
            G.ResetTimer += 0.016667f * 2;

        if (Input.IsActionPressed("Reset") && G.DidLevelIntroPassed && !_isResetDisabled)
           G.ResetTimer += 0.016667f;
        else G.ResetTimer = G.ResetTimer > 0 ? G.ResetTimer - 0.016667f : 0;

        if (G.ResetTimer > 1.5f)
        {
            G.IsCrossesEnabled = true;
            G.IsProgressPaused = false;
            G.CrossSpawnMultiplier = 1;
            UnchangableMeta.SaveRecords();
            EmitSignal("LevelReseting");
            LoadScene("res://Content/Scenes/Levels/FullParts/Level" + G.CurrentLevel + G.LevelAdditionalLink + ".tscn");
        }
    }
    public void UnPause()
    {
        bool IsPaused = GetTree().Paused;
        AudioServer.SetBusEffectEnabled(2, 0, !IsPaused);
        AudioServer.SetBusEffectEnabled(6, 0, !IsPaused);
        GetNode<TextureButton>("Pause/Interface/ButtonsFrame/Resume").GrabFocus();
        GetNode<TextureButton>("Pause/Interface/ButtonsFrame/Rewind").Disabled = _isResetDisabled;

        var animationPlayer = GetNode<AnimationPlayer>("Pause/Interface/AnimationPlayer");
        if (!IsPaused)
            animationPlayer.Play("Pause");
        else
            animationPlayer.PlayBackwards("Pause");

        Input.MouseMode = IsPaused ? Input.MouseModeEnum.Hidden : Input.MouseModeEnum.Visible;

        GetTree().Paused = !IsPaused;
    }
    public void Options()
    {
        var pause = GetNode<CanvasLayer>("Pause");
        pause.ProcessMode = ProcessModeEnum.Disabled;
        pause.AddChild(ResourceLoader.Load<PackedScene>("res://Content/Scenes/Interface&Menu/OptionsMenu.tscn").Instantiate<Control>());
        GetNode<AnimationPlayer>("Pause/Interface/AnimationPlayer").Play("OpeningSubMenu");
        _subMenusOpened = true;
    }
    public void Menu()
    {
        UnchangableMeta.SaveRecords();
        UnchangableMeta.SaveToFile();
        G.CompletelyResetValues();
        GetTree().Paused = false;
        GetTree().ChangeSceneToFile("res://Content/Scenes/Interface&Menu/WelcomeToGOS.tscn");
    }
    public void OptionsClosing()
    {
        GetNode<CanvasLayer>("Pause").ProcessMode = ProcessModeEnum.WhenPaused;
        GetNode<TextureButton>("Pause/Interface/ButtonsFrame/Options").GrabFocus();
        GetNode<AnimationPlayer>("Pause/Interface/AnimationPlayer").PlayBackwards("OpeningSubMenu");
        _subMenusOpened = false;
    }

    public void MusicAnimationFinished(string animation)
    {
        if (animation == "MusicStopping")
        {
            _levelMusicPlayer.Stream = null;
            _levelMusicPlayer.Stop();
        }
    }

    public void GiveAchievement(int index)
    {
        G.GetAchievement(index);
    }

    //Methods from above are actively used in game scripts and their sloppy use can break some processes, use at your own risk.
    //The methods below are made specifically for modding, use them however you want.

    public void LoadScene(string ScenePath)
    {
        GetTree().ChangeSceneToFile(ScenePath);
    }
    public void SetCrossesEnabled(bool value)
    {
        G.IsCrossesEnabled = value;
    }
    public void SetCrossesSpawnMultiplier(float value)
    {
        G.CrossSpawnMultiplier = value;
    }
    public void SetCrossesProgressCoeff(float value)
    {
        G.CrossesProgressCoeff = value;
    }
    public void SetProgressPaused(bool value)
    {
        G.IsProgressPaused = value;
    }
    public void DebugTransitiveValue(int index)
    {
        GD.Print(G.TransitiveVariant[index]);
    }
    public void SetTransitiveValue(int index, Variant value)
    {
        G.TransitiveVariant[index] = value;
    }
    public void ResetTransitiveValue(int index)
    {
        G.TransitiveVariant[index] = "";
    }
    public void ResetAllTransitiveValues()
    {
        for (int i = 0; i < G.TransitiveVariant.Length; i++)
            G.TransitiveVariant[i] = "";
    }
    public void SetScores(float value)
    {
        G.Scores = value;
        EmitSignal("RecalculateCrossWeight");
    }
    public void SetPauseDisabled(bool value)
    {
        _isPauseDisabled = value;
    }
    public void SetResetDisabled(bool value)
    {
        _isResetDisabled = value;
    }
    public void SetLevelCompleteTime(float value)
    {
        G.LevelCompleteTime = value;
    }
    public void SetDebugEnabled(bool value)
    {
        G.IsDebugEnabled = value;
    }
}