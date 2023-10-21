using Godot;

public partial class MainScript : Node2D
{
    private bool _subMenusOpened;

    public override void _Ready()
    {
        GetNode<AudioStreamPlayer>("LevelMusicPlayer").Stream = ResourceLoader.Load<AudioStream>("res://Content/Sounds/Soundtrack/Level" + G.CurrentLevel + ".mp3");
        if (UnchangableMeta.LevelPlayedStatus[G.CurrentLevel - 1] != 1)
        {
            UnchangableMeta.LevelPlayedStatus[G.CurrentLevel - 1] = 1;
            UnchangableMeta.SaveToFile();
        }

        SetProcess(false);
    }
    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("Cancel") && !_subMenusOpened && !G._isLevel10Finaling)
            UnPause();
    }
    public void UnPause()
    {
        AudioServer.SetBusEffectEnabled(2, 0, !GetTree().Paused);
        AudioServer.SetBusEffectEnabled(6, 0, !GetTree().Paused);
        GetNode<CanvasLayer>("Pause").Visible = !GetTree().Paused;
        GetNode<TextureButton>("Pause/Buttons/Resume").GrabFocus();
        Input.MouseMode = GetTree().Paused ? Input.MouseModeEnum.Hidden : Input.MouseModeEnum.Visible;
        GetTree().Paused = !GetTree().Paused;
    }
    public void Options()
    {
        var pause = GetNode<CanvasLayer>("Pause");
        pause.ProcessMode = ProcessModeEnum.Disabled;
        pause.Visible = false;
        AddChild(ResourceLoader.Load<PackedScene>("res://Content/Scenes/Interface&Menu/OptionsMenu.tscn").Instantiate<Control>());
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
        var pause = GetNode<CanvasLayer>("Pause");
        pause.ProcessMode = ProcessModeEnum.Always;
        pause.Visible = true;
        GetNode<TextureButton>("Pause/Buttons/Resume").GrabFocus();
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
