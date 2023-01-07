using Godot;
using System;

public class MainScript : Node2D
{
    public override void _Process(float delta)
    {
        if (Input.IsActionJustPressed("Cancel"))
        {
            UnPause();
        }
    }
    public void UnPause()
    {
        var pause = GetNode<CanvasLayer>("Pause");
        pause.Visible = !GetTree().Paused;
        var resumeButton = GetNode<TextureButton>("Pause/Buttons/Resume");
        resumeButton.GrabFocus();
        Input.MouseMode = GetTree().Paused ? Input.MouseModeEnum.Hidden : Input.MouseModeEnum.Visible;
        GetTree().Paused = !GetTree().Paused;
    }
    public void Options()
    {
        var pause = GetNode<CanvasLayer>("Pause");
        pause.PauseMode = PauseModeEnum.Stop;
        pause.Visible = false;
        AddChild(ResourceLoader.Load<PackedScene>("res://Content/Scenes/Interface&Menu/OptionsMenu.tscn").Instance<Control>());
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
    }
}
