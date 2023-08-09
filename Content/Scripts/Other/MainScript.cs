using Godot;

public partial class MainScript : Node2D
{
    private bool _subMenusOpened;

    public override void _Ready()
    {
        var levelMusicPlayer = GetNode<AudioStreamPlayer>("LevelMusicPlayer");
        AudioStream Music = ResourceLoader.Load<AudioStream>("res://Content/Sounds/Soundtrack/Level" + G.CurrentLevel + ".wav");
        levelMusicPlayer.Stream = Music;
        levelMusicPlayer.Play();
    }
    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("Cancel") && !_subMenusOpened)
            UnPause();
    }
    public void UnPause()
    {
        AudioServer.SetBusEffectEnabled(2, 0, !GetTree().Paused);
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
        G.ResetValues();
        GetTree().ChangeSceneToFile("res://Content/Scenes/Interface&Menu/Menu.tscn");
        AudioServer.SetBusEffectEnabled(2, 0, false);
    }
    public void OptionsClosing()
    {
        var pause = GetNode<CanvasLayer>("Pause");
        pause.ProcessMode = ProcessModeEnum.Always;
        pause.Visible = true;
        GetNode<TextureButton>("Pause/Buttons/Resume").GrabFocus();
        _subMenusOpened = false;
    }
}
