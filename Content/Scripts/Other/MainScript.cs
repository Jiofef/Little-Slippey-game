using Godot;
using System;

public class MainScript : Node2D
{
    private bool _subMenusOpened;
    public override void _Process(float delta)
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
        pause.PauseMode = PauseModeEnum.Stop;
        pause.Visible = false;
        AddChild(ResourceLoader.Load<PackedScene>("res://Content/Scenes/Interface&Menu/OptionsMenu.tscn").Instance<Control>());
        _subMenusOpened = true;
    }
    public void Menu()
    {
        G.SaveRecords();
        G.FitToDefaultValues();
        GetTree().ChangeScene("res://Content/Scenes/Interface&Menu/Menu.tscn");
    }
    public void OptionsClosing()
    {
        var pause = GetNode<CanvasLayer>("Pause");
        pause.PauseMode = PauseModeEnum.Process;
        pause.Visible = true;
        GetNode<TextureButton>("Pause/Buttons/Resume").GrabFocus();
        _subMenusOpened = false;
    }
}
