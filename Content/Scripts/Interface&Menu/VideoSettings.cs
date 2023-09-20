using Godot;
using System;

public partial class VideoSettings : Control
{
    private OptionButton _screenFormat;
    public override void _Ready()
    {
        _screenFormat = GetNode<OptionButton>("VBoxContainer/ScreenFormat");
        Meta.Instance.IsFullScreen = DisplayServer.WindowGetMode() == DisplayServer.WindowMode.Fullscreen;
        GetNode<OptionButton>("VBoxContainer/ScoresShowingFormat").Selected = Meta.Instance.ScoresShowingFormatIndex;
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
    public void ChangeScoresShowingFormat(int index)
    {
        Meta.Instance.ScoresShowingFormatIndex = index;
    }
}
