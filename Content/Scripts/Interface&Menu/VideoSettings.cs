using Godot;
using System;

public class VideoSettings : Control
{
    public override void _Ready()
    {
        var screenFormat = GetNode<OptionButton>("VBoxContainer/ScreenFormat");
        screenFormat.AddItem("Standart");
        screenFormat.AddItem("Fullscreen");
        screenFormat.Selected = Convert.ToInt32(Meta.Instance._isfullscreen);
    }
    public void ChangeScreenFormat(int index)
    {
        Meta.Instance._isfullscreen = Convert.ToBoolean(index);
        OS.WindowFullscreen = Convert.ToBoolean(index);
    }
}
