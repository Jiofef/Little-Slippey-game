using Godot;
using System;

public partial class VideoSettings : Control
{
    private OptionButton _screenFormat;
    public override void _Ready()
    {
        _screenFormat = GetNode<OptionButton>("VBoxContainer/ScreenFormat");
        Meta.Instance.IsFullScreen = DisplayServer.WindowGetMode() == DisplayServer.WindowMode.Fullscreen;
    }
    public override void _Process(double delta)
    {
        _screenFormat.Selected = DisplayServer.WindowGetMode() == DisplayServer.WindowMode.Fullscreen ? 1 : 0;
    }
    public void ChangeScreenFormat(int index)
    {
        Meta.Instance.IsFullScreen = Convert.ToBoolean(index);
        DisplayServer.WindowSetMode(index == 1 ? DisplayServer.WindowMode.Fullscreen : DisplayServer.WindowMode.Windowed);
    }
}
