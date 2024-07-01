using Godot;
using System;

public partial class OptionsMenu : Control
{
    [Signal]
    public delegate void OptionsClosingEventHandler();
    [Signal]
    public delegate void GUIOptionsChangedEventHandler();
    private Vector2I[] _windowSizes =
    {
        new Vector2I (640, 360),
        new Vector2I (854, 480),
        new Vector2I (960, 540),
        new Vector2I (1280, 720),
        new Vector2I (1600, 900),
        new Vector2I (1920, 1080),
        new Vector2I (2560, 1440),
        new Vector2I (3840, 2160)
    };
    ScrollContainer _videoContainer;

    public override void _Ready()
    {
        _videoContainer = GetNode<ScrollContainer>("VideoContainer");
        Meta.OptionsReserve = Meta.Instance.Clone();
        if (G.CurrentLevel != 0)
        {
            Connect("OptionsClosing", new Callable(GetNode("../.."), "OptionsClosing"));
            Connect("GUIOptionsChanged", new Callable(GetNode("../../Level/Player/Camera2D"), "ApplyGUIOptions"));
            GetNode<TextureButton>("DeclineButton").GrabFocus();
        }

        string[] SliderNames = { "Global", "Interface", "Music", "Player", "Crosses", "Explosions", "Environment" };
        for (int i = 0; i < SliderNames.Length; i++)
            GetNode<Slider>("SoundContainer/" + SliderNames[i] + "Slider").Value = Meta.Instance.BusVolumes[i];


        Meta.Instance.IsFullScreen = DisplayServer.WindowGetMode() == DisplayServer.WindowMode.Fullscreen;
        var windowSizeSlider = GetNode<Slider>("VideoContainer/GridContainer/WindowSizesContainer/WindowSizeSlider");
        for (int i = 0; i <= windowSizeSlider.MaxValue; i++)
            if (Meta.Instance.WindowSize == _windowSizes[i])
                windowSizeSlider.Value = i;

        GetNode<CheckBox>("VideoContainer/GridContainer/VerticalSyncContainer/" + (Meta.Instance.VSyncOn ? "On" : "Off") + "CheckBox").ButtonPressed = true;

        string[] ScoresShowingFormats = {"Default", "Mini", "Hide"};
        GetNode<CheckBox>("VideoContainer/GridContainer/ScoresLabelContainer/" + ScoresShowingFormats[Meta.Instance.ScoresShowingFormatIndex] + "CheckBox").ButtonPressed = true;
        GetNode<CheckBox>("VideoContainer/GridContainer/GridContainer/CheckBox" + (Meta.Instance.ScoresLabelLocationX + 1) + "X" + (Meta.Instance.ScoresLabelLocationY + 1) + "Y").ButtonPressed = true;

        GetNode<Slider>("VideoContainer/GridContainer/CameraZoomContainer/CameraZoomSlider").Value = Meta.Instance.CameraZoom;
        GetNode<Label>("VideoContainer/GridContainer/CameraZoomContainer/CameraZoom").Text = "X" + Meta.Instance.CameraZoom;

        GetNode<AnimationPlayer>("AnimationPlayer").Play("Appearance");
    }

    public void Cancel()
    {
        Meta.Instance = Meta.OptionsReserve.Clone();
        Meta.Instance.ApplyOptions();
        if (G.CurrentLevel == 0)
            Connect("OptionsClosing", new Callable(GetParent(), "OpenedMenuClosed"));
        EmitSignal("OptionsClosing");
        EmitSignal("GUIOptionsChanged", false);
        QueueFree();
    }
    public void Accept()
    {
        DisplayServer.WindowSetSize(Meta.Instance.WindowSize);
        Meta.Instance.SaveToFile();
        if (G.CurrentLevel == 0)
            Connect("OptionsClosing", new Callable(GetParent(), "OpenedMenuClosed"));
        EmitSignal("OptionsClosing");
        QueueFree();
    }
    public void Controls()
    {
        var controlsMenu = GetNode<Control>("ControlsMenu");
        if (controlsMenu.Visible && controlsMenu.Scale.X != 0)
            controlsMenu.Visible = false;
        else
        {
            GetNode<AnimationPlayer>("ControlsMenu/AnimationPlayer").Play("Appearence");
            controlsMenu.Visible = true;
        }
    }

    //SoundOptions
    public void SoundChanging(float value, int BusNubmer)
    {
        Meta.Instance.BusVolumes[BusNubmer] = value;
        AudioServer.SetBusVolumeDb(BusNubmer, value);
    }

    public override void _PhysicsProcess(double delta)
    {
        string[] WindowModes = { "Full", "Window" };
        GetNode<CheckBox>("VideoContainer/GridContainer/ScreenModeContainer/" + WindowModes[DisplayServer.WindowGetMode() == DisplayServer.WindowMode.Fullscreen ? 0 : 1] + "CheckBox").ButtonPressed = true;
        if (Input.IsActionJustPressed("Cancel"))
            Cancel();

        _videoContainer.ScrollVertical += (int)(Input.GetActionStrength("ui_scroll_down") * 5) - (int)(Input.GetActionStrength("ui_scroll_up") * 5);
        _videoContainer.ScrollHorizontal += (int)(Input.GetActionStrength("ui_scroll_right") * 5) - (int)(Input.GetActionStrength("ui_scroll_left") * 5);
    }

    //VideoOptions
    public void ChangeScreenFormat(int index)
    {
        Meta.Instance.IsFullScreen = index == 0 ? true : false;
        DisplayServer.WindowSetMode(index == 0 ? DisplayServer.WindowMode.Fullscreen : DisplayServer.WindowMode.Windowed);
        DisplayServer.WindowSetSize(Meta.Instance.WindowSize);
    }
    public void ChangeWindowSize(int index)
    {
        Meta.Instance.WindowSize = _windowSizes[index];
        GetNode<Label>("VideoContainer/GridContainer/WindowSizesContainer/Label").Text = Meta.Instance.WindowSize.X + "X" + Meta.Instance.WindowSize.Y;
    }
    public void ChangeScoresShowingFormat(int index)
    {
        Meta.Instance.ScoresShowingFormatIndex = (byte)index;
        EmitSignal("GUIOptionsChanged", false);
    }
    public void ChangeVSync(int index)
    {
        Meta.Instance.VSyncOn = index == 0 ? true : false;
        DisplayServer.WindowSetVsyncMode(Meta.Instance.VSyncOn ? DisplayServer.VSyncMode.Enabled : DisplayServer.VSyncMode.Disabled);
    }
    public void ChangeScoresLabelLocation(int indexX, int indexY)
    {
        Meta.Instance.ScoresLabelLocationX = (byte)indexX;
        Meta.Instance.ScoresLabelLocationY = (byte)indexY;
        EmitSignal("GUIOptionsChanged", false);
    }
    public void CameraZoomChanged(float value)
    {
        Meta.Instance.CameraZoom = value;
        GetNode<Label>("VideoContainer/GridContainer/CameraZoomContainer/CameraZoom").Text = "X" + Meta.Instance.CameraZoom;
        EmitSignal("GUIOptionsChanged", false);
    }
    public void SetLanguage(int languageNumber)
    {
        Meta.Instance.language = (Meta.Language)languageNumber;
        TranslationServer.SetLocale(Meta.Instance.language.ToString());
        ProjectSettings.SetSetting("gui/theme/custom_font", "FontPath");
    }
}
