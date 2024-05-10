using Godot;

public partial class MainScript : Node2D
{
    private bool _subMenusOpened;
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
        if (G.DidLevelIntroPassed)
        {
            GetNode<CanvasLayer>("EpicIntro").QueueFree();
            GetNode<Node2D>("Level").ProcessMode = ProcessModeEnum.Pausable;
        }
        if (UnchangableMeta.LevelPlayedStatus[G.CurrentLevel - 1] != 1)
        {
            UnchangableMeta.LevelPlayedStatus[G.CurrentLevel - 1] = 1;
            UnchangableMeta.SaveToFile();
        }

        SetProcess(false);
    }
    public override void _PhysicsProcess(double delta)
    {
        if (Input.IsActionJustPressed("Cancel") && !_subMenusOpened && !G._isLevel10Finaling && G.DidLevelIntroPassed)
            UnPause();
        if (_rewindButton.ButtonPressed)
            G.ResetTimer += 0.016667f * 2;

        if (!G._isLevel10Finaling)
        {
            if (Input.IsActionPressed("Reset") && G.DidLevelIntroPassed)
                G.ResetTimer += 0.016667f;
            else G.ResetTimer = G.ResetTimer > 0 ? G.ResetTimer - 0.016667f : 0;

            if (G.ResetTimer > 1.5f)
            {
                G.IsCrossesEnabled = true;
                G.IsProgressPaused = false;
                G.CrossSpawnMultiplier = 1;
                UnchangableMeta.SaveRecords();
                LoadScene("res://Content/Scenes/Levels/FullParts/Level" + G.CurrentLevel + G.LevelAdditionalLink + ".tscn");
            }
        }
    }
    public void UnPause()
    {
        bool IsPaused = GetTree().Paused;
        AudioServer.SetBusEffectEnabled(2, 0, !IsPaused);
        AudioServer.SetBusEffectEnabled(6, 0, !IsPaused);
        GetNode<TextureButton>("Pause/Interface/ButtonsFrame/Resume").GrabFocus();

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
    public void LoadScene(string ScenePath)
    {
        GetTree().ChangeSceneToFile(ScenePath);
    }
    public void MusicAnimationFinished(string animation)
    {
        if (animation == "MusicStopping")
        {
            _levelMusicPlayer.Stream = null;
            _levelMusicPlayer.Stop();
        }
    }
}