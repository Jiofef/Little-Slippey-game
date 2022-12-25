using Godot;
using System;
using System.Reflection;

public class VideoSettings : Control
{
    public override void _Ready()
    {
        var screenFormat = GetNode<OptionButton>("VBoxContainer/ScreenFormat");
        screenFormat.AddItem("Standart");
        screenFormat.AddItem("Fullscreen");
        Meta.Instance._isfullscreen = OS.WindowFullscreen;
        screenFormat.Selected = Convert.ToInt32(OS.WindowFullscreen);
    }
    public void ChangeScreenFormat(int index)
    {
        Meta.Instance._isfullscreen = Convert.ToBoolean(index);
        OS.WindowFullscreen = Convert.ToBoolean(index);
    }
}
