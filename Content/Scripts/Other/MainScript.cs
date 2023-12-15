using Godot;

public partial class MainScript : Node2D
{
    private bool _subMenusOpened;
    private string _currentMusicName;
    private float _trackRestartPosition = 0;
    TextureButton _rewindButton;
    AudioStreamPlayer _levelMusicPlayer;

    public override void _Ready()
    {
        G.CurrentPopupAchievementsLayer = GetNode<CanvasLayer>("PopupAchievementsLayer");
        _rewindButton = GetNode<TextureButton>("Pause/Interface/ButtonsFrame/Rewind");
        _levelMusicPlayer = GetNode<AudioStreamPlayer>("LevelMusicPlayer");
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
        GetNode<AudioStreamPlayer>("LevelMusicPlayer").Play(_trackRestartPosition);
    }

    public void LevelLoad()
    {
        GetNode("PlayPart").AddChild(ResourceLoader.Load<PackedScene>("res://Content/Scenes/Levels/FullParts/Level" + G.CurrentLevel + G.LevelAdditionalLink + ".tscn").Instantiate());
    }

    public void PlayMusic(string MusicName, float TrackRestartPosition = 0, float StartingDuration = 0)
    {
        var musicAnimationPlayer = GetNode<AnimationPlayer>("LevelMusicPlayer/AnimationPlayer");
        if (MusicName != _currentMusicName)
        {
            _levelMusicPlayer.Stream = ResourceLoader.Load<AudioStream>("res://Content/Sounds/Soundtrack/" + MusicName + ".mp3");
            _currentMusicName = MusicName;
            _trackRestartPosition = TrackRestartPosition;
            if (StartingDuration > 0)
                musicAnimationPlayer.Play("MusicStarting", -1, 1 / StartingDuration);
            else
                _levelMusicPlayer.VolumeDb = 0;
            _levelMusicPlayer.Play();
        }
    }

    public void StopMusic(float StoppingDuration = 0)
    {
        var musicAnimationPlayer = GetNode<AnimationPlayer>("LevelMusicPlayer/AnimationPlayer");
        if (StoppingDuration > 0)
            musicAnimationPlayer.Play("MusicStopping", -1, 1 / StoppingDuration);
        else
        {
            musicAnimationPlayer.Stop();
            _levelMusicPlayer.Stop();
            _levelMusicPlayer.VolumeDb = -20;
        }
        _currentMusicName = "";
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