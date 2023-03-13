using Godot;

public partial class MainScript : Node2D
{
    private bool _subMenusOpened;

    public override void _Ready()
    {
        if (!UnchangableMeta.DoFirstTimePlayed)
        {
            Input.MouseMode = Input.MouseModeEnum.Visible;
            GetTree().Paused = true;
            OpenMovementTutorial();
        }
    }
    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("Cancel") && !_subMenusOpened)
        {
            UnPause();
        }
    }
    public void UnPause()
    {
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
        G.SaveRecords();
        G.ResetValues();
        GetTree().ChangeSceneToFile("res://Content/Scenes/Interface&Menu/Menu.tscn");
    }
    public void OptionsClosing()
    {
        var pause = GetNode<CanvasLayer>("Pause");
        pause.ProcessMode = ProcessModeEnum.Always;
        pause.Visible = true;
        GetNode<TextureButton>("Pause/Buttons/Resume").GrabFocus();
        _subMenusOpened = false;
    }
    public void MovementTutorialClosed()
    {
        _subMenusOpened = false;
        GetNode<CanvasLayer>("Pause").ProcessMode = ProcessModeEnum.WhenPaused;
    }
    public void OpenMovementTutorial()
    {
        _subMenusOpened = true;
        GetNode<CanvasLayer>("Pause").ProcessMode = ProcessModeEnum.Disabled;
        AddChild(ResourceLoader.Load<PackedScene>("res://Content/Scenes/Interface&Menu/MovementTutorial.tscn").Instantiate());
    }
}
