using Godot;

public partial class MainScript : Node2D
{
    private bool _subMenusOpened;
    TextureButton _rewindButton;

    public override void _Ready()
    {
        G.CurrentPopupAchievementsLayer = GetNode<CanvasLayer>("PopupAchievementsLayer");
        GetNode<AudioStreamPlayer>("LevelMusicPlayer").Stream = ResourceLoader.Load<AudioStream>("res://Content/Sounds/Soundtrack/Level" + G.CurrentLevel + ".mp3");
        _rewindButton = GetNode<TextureButton>("Pause/Interface/ButtonsFrame/Rewind");
        if (UnchangableMeta.LevelPlayedStatus[G.CurrentLevel - 1] != 1)
        {
            UnchangableMeta.LevelPlayedStatus[G.CurrentLevel - 1] = 1;
            UnchangableMeta.SaveToFile();
        }

        SetProcess(false);
    }
    public override void _PhysicsProcess(double delta)
    {
        if (Input.IsActionJustPressed("Cancel") && !_subMenusOpened && !G._isLevel10Finaling)
            UnPause();
        if (_rewindButton.ButtonPressed)
            G.ResetTimer += 0.016667f * 2;

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

    public void MusicFinished()
    {
        GetNode<AudioStreamPlayer>("LevelMusicPlayer").Play(0);
    }
    public void LevelLoad()
    {
        GetNode("PlayPart").AddChild(ResourceLoader.Load<PackedScene>("res://Content/Scenes/Levels/FullParts/Level" + G.CurrentLevel + G.LevelAdditionalLink + ".tscn").Instantiate());
    }
}
