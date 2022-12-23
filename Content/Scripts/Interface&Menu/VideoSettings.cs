using Godot;
using System;

public class VideoSettings : Control
{
    private Vector2[] Resolutions =
    {
        new Vector2(3840, 2160),
        new Vector2(2560, 1440),
        new Vector2(1920, 1080),
        new Vector2(1336, 768),
        new Vector2(1536, 864),
        new Vector2(1280, 720),
        new Vector2(1600, 900),
        new Vector2(1024, 600),
        new Vector2(800, 600),
        new Vector2(640, 480)
    };
    private OptionButton changeResolution;
    public override void _Ready()
    {
        changeResolution = GetNode<OptionButton>("VBoxContainer/ChangeResolution");
        for (int i = 0; i < Resolutions.Length; i++)
            changeResolution.AddItem(Resolutions[i].x + "x" + Resolutions[i].y);
        for (int i = 0; ; i++)
            if (Meta.Instance.Resolution == Resolutions[i])
            {
                changeResolution.Selected = i;
                break;
            }
        var screenFormat = GetNode<OptionButton>("VBoxContainer/ScreenFormat");
        screenFormat.AddItem("Windowed");
        screenFormat.AddItem("Borderless Window");
        screenFormat.AddItem("Fullscreen");
        screenFormat.Selected = (int)Meta.Instance._screenFormat;
        changeResolution.Disabled = Meta.Instance._screenFormat == Meta.ScreenFormat.Fullscreen ? true : false;
    }
    public void ChangeResolution(int index)
    {
        Meta.Instance.Resolution = Resolutions[index];
        OS.WindowSize = Meta.Instance.Resolution;
    }
    public void ChangeScreenFormat(int index)
    {
        switch (index)
        {
            case 0:
                OS.WindowFullscreen = false;
                OS.WindowBorderless = false;
                Meta.Instance._screenFormat = Meta.ScreenFormat.Windowed;
                changeResolution.Disabled = false;
                break;
            case 1:
                OS.WindowFullscreen = false;
                OS.WindowBorderless = true;
                Meta.Instance._screenFormat = Meta.ScreenFormat.Borderless;
                changeResolution.Disabled = false;
                break;
            case 2:
                OS.WindowFullscreen = true;
                OS.WindowBorderless = false;
                Meta.Instance._screenFormat = Meta.ScreenFormat.Fullscreen;
                changeResolution.Disabled = true;
                break;
        }
        OS.WindowSize = Meta.Instance.Resolution;
    }
}
