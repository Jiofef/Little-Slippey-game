using Godot;
using System;

public class VideoSettings : Control
{
    public override void _Ready()
    {
        var screenFormat = GetNode<OptionButton>("VBoxContainer/ScreenFormat");
        screenFormat.AddItem("Standart");
        screenFormat.AddItem("Fullscreen");
        Meta.Instance.IsFullScreen = OS.WindowFullscreen;
        screenFormat.Selected = Convert.ToInt32(OS.WindowFullscreen);
    }
    public void ChangeScreenFormat(int index)
    {
        Meta.Instance.IsFullScreen = Convert.ToBoolean(index);
        OS.WindowFullscreen = Convert.ToBoolean(index);
    }
}
