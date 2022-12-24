using Godot;
using System;

public class VideoSettings : Control
{
    public override void _Ready()
    {
        var screenFormat = GetNode<OptionButton>("VBoxContainer/ScreenFormat");
        screenFormat.AddItem("Standart");
        screenFormat.AddItem("Fullscreen");
        screenFormat.Selected = (int)Meta.Instance._screenFormat;
    }
    public void ChangeScreenFormat(int index)
    {
        switch (index)
        {
            case 0:
                OS.WindowFullscreen = false;
                OS.WindowBorderless = false;
                Meta.Instance._screenFormat = Meta.ScreenFormat.Standart;
                break;
            case 1:
                OS.WindowFullscreen = true;
                OS.WindowBorderless = false;
                Meta.Instance._screenFormat = Meta.ScreenFormat.Fullscreen;
                break;
        }
    }
}
