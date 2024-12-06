using Godot;
using System;

public partial class OptionsMenu : DraggableWindow
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
    HSlider _lastFocusedSlider;

    public override void _Ready()
    {
        base._Ready();
        GetViewport().GuiFocusChanged += GuiFocusChanged => WhenFocusChanged(GuiFocusChanged);
        Meta.OptionsReserve = Meta.Instance.Clone();
        if (G.CurrentLevel != 0 || !G.IsLevelVanilla)
        {
            Connect("OptionsClosing", new Callable(GetNode(".."), "OptionsClosing"));
            Connect("GUIOptionsChanged", new Callable(GetNode("../../Level/Player/Camera2D"), "ApplyGUIOptions"));
            GetNode<TextureButton>("MarginContainer/VBoxContainer/HBoxContainer/DeclineButton").GrabFocus();
        }

        UpdateSettingsGUI();
    }

    public void Cancel()
    {
        Meta.Instance = Meta.OptionsReserve.Clone();
        Meta.Instance.ApplyOptions();
        EmitSignal("OptionsClosing");
        EmitSignal("GUIOptionsChanged", false);
        QueueFree();
    }
    public void Accept()
    {
        Meta.Instance.SaveToFile();
        EmitSignal("OptionsClosing");
        QueueFree();
    }
    public void SetByDefault()
    {
        var confirmation = (ConfirmationWindow)GD.Load<PackedScene>("res://Content/Scenes/Interface&Menu/OptionsSetByDefaultConfirmation.tscn").Instantiate();
        confirmation.Connect("Accepted", new Callable(this, "SetByDefaultConfirm")); 

        AddChild(confirmation);
    }
    public void SetByDefaultConfirm()
    {
        var Tabs = GetNode<TabContainer>("MarginContainer/VBoxContainer/Tabs");
        switch (Tabs.CurrentTab)
        {
            case 0: // Video
                Meta.Instance.Video = Meta.GetDefaultVideoSettings();
                EmitSignal("GUIOptionsChanged", false);
                break;
            case 1: // Sound
                Meta.Instance.Sound = Meta.GetDefaultSoundOptions();
                break;
        }

        Meta.Instance.ApplyOptions();
        UpdateSettingsGUI();
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
    private void WhenFocusChanged(Control node)
    {
        if (_lastFocusedSlider != null)
            _lastFocusedSlider.Editable = true;
        if (node.GetClass() == "HSlider" && !Input.IsMouseButtonPressed(MouseButton.Left))
        {
            ((HSlider)node).Editable = false;
            _lastFocusedSlider = (HSlider)node;
        }

        CanvasItem Node = node;
        if (!IsInsideTree()) return;

        while (Node.Name != Name && Node.GetParent() is CanvasItem)
        {
            if (Node.Name == "SoundContainer" || Node.Name == "VideoContainer")
            {
                GetNode<Control>("ControlsMenu").Visible = false;
                break;
            }
            else if (Node.GetParent() != GetTree().Root)
                Node = Node.GetParent<CanvasItem>();
            else
                break;
        }
    }

    string[] SliderNames = { "Global", "Interface", "Music", "Player", "Crosses", "Explosions", "Environment" };
    private void UpdateSettingsGUI()
    {
        const string TabsLink = "MarginContainer/VBoxContainer/Tabs/";
        for (int i = 0; i < SliderNames.Length; i++)
        {
            GetNode<Slider>(TabsLink + "Sound/MarginContainer/VBoxContainer/" + SliderNames[i] + "/Slider").Value = Meta.Instance.Sound.BusVolumes[i];
            GetNode<Label>(TabsLink + "Sound/MarginContainer/VBoxContainer/" + SliderNames[i] + "/Value").Text = ((int)(Meta.Instance.Sound.BusVolumes[i] * 100)).ToString();
        }



        GetNode<OptionButton>(TabsLink + "Video/MarginContainer/VBoxContainer/ScreenMode/OptionButton").Selected = Meta.Instance.Video.IsFullScreen ? 0 : 1;

        Meta.Instance.Video.IsFullScreen = DisplayServer.WindowGetMode() == DisplayServer.WindowMode.Fullscreen;
        var windowSizeOptions = GetNode<OptionButton>(TabsLink + "Video/MarginContainer/VBoxContainer/WindowSize/OptionButton");
        for (int i = 0; i <= windowSizeOptions.ItemCount - 1; i++)
            if (Meta.Instance.Video.WindowSize == _windowSizes[i])
                windowSizeOptions.Selected = i;


        GetNode<CheckBox>(TabsLink + "Video/MarginContainer/VBoxContainer/VSync/CheckBox").ButtonPressed = Meta.Instance.Video.VSyncOn;

        GetNode<OptionButton>(TabsLink + "Video/MarginContainer/VBoxContainer/ScoresLabelMode/OptionButton").Selected = Meta.Instance.Video.ScoresShowingFormatIndex;
        GetNode<CheckBox>(TabsLink + "Video/MarginContainer/VBoxContainer/ScoresLabelLocation/GridContainer/CheckBox" + (Meta.Instance.Video.ScoresLabelLocationX) + "X" + (Meta.Instance.Video.ScoresLabelLocationY) + "Y").ButtonPressed = true;

        GetNode<Slider>(TabsLink + "Video/MarginContainer/VBoxContainer/CameraZoom/Slider").Value = Meta.Instance.Video.CameraZoom;
        GetNode<Label>(TabsLink + "Video/MarginContainer/VBoxContainer/CameraZoom/Value").Text =  Meta.Instance.Video.CameraZoom + "x";

        GetNode<OptionButton>(TabsLink + "Video/MarginContainer/VBoxContainer/Language/OptionButton").Selected = (int)Meta.Instance.Video.language;
    }

    //SoundOptions
    public void SoundChanging(float value, int BusNubmer)
    {
        Meta.Instance.Sound.BusVolumes[BusNubmer] = value;

        float DbValue = Mathf.LinearToDb(value);

        AudioServer.SetBusVolumeDb(BusNubmer, DbValue);
        GetNode<Label>("MarginContainer/VBoxContainer/Tabs/Sound/MarginContainer/VBoxContainer/" + SliderNames[BusNubmer] + "/Value").Text = ((int)(value * 100)).ToString();
    }

    public override void _PhysicsProcess(double delta)
    {
        string[] WindowModes = { "Full", "Window" };
        if (Input.IsActionJustPressed("Cancel"))
            Cancel();

        Control focusOwner = GetWindow().GuiGetFocusOwner();
        if (Input.IsActionJustPressed("ui_accept") && focusOwner != null && focusOwner.GetClass() == "HSlider")
            ((HSlider)focusOwner).Editable = !((HSlider)focusOwner).Editable;
    }

    //VideoOptions
    public void ChangeScreenFormat(int index)
    {
        bool IsFullScreen = index == 0 ? true : false;
        Meta.Instance.Video.IsFullScreen = IsFullScreen;
        DisplayServer.WindowSetMode(IsFullScreen ? DisplayServer.WindowMode.Fullscreen : DisplayServer.WindowMode.Windowed);
        DisplayServer.WindowSetSize(Meta.Instance.Video.WindowSize);

        if (IsFullScreen)
            GetViewport().SdfScale = Viewport.SdfScaleEnum.Scale25Percent;
    }
    public void ChangeWindowSize(int index)
    {
        Meta.Instance.Video.WindowSize = _windowSizes[index];
        DisplayServer.WindowSetSize(Meta.Instance.Video.WindowSize);
    }
    public void ChangeScoresShowingFormat(int index)
    {
        Meta.Instance.Video.ScoresShowingFormatIndex = (byte)index;
        EmitSignal("GUIOptionsChanged", false);
    }
    public void ChangeVSync(bool value)
    {
        Meta.Instance.Video.VSyncOn = value;
        DisplayServer.WindowSetVsyncMode(Meta.Instance.Video.VSyncOn ? DisplayServer.VSyncMode.Enabled : DisplayServer.VSyncMode.Disabled);
    }
    public void ChangeScoresLabelLocation(int indexX, int indexY)
    {
        Meta.Instance.Video.ScoresLabelLocationX = (byte)indexX;
        Meta.Instance.Video.ScoresLabelLocationY = (byte)indexY;
        EmitSignal("GUIOptionsChanged", false);
    }
    public void CameraZoomChanged(float value)
    {
        Meta.Instance.Video.CameraZoom = value;
        GetNode<Label>("MarginContainer/VBoxContainer/Tabs/Video/MarginContainer/VBoxContainer/CameraZoom/Value").Text = "X" + Meta.Instance.Video.CameraZoom;
        EmitSignal("GUIOptionsChanged", false);
    }
    public void SetLanguage(int languageNumber)
    {
        Meta.Instance.Video.language = (Meta.VideoClass.Language)languageNumber;
        TranslationServer.SetLocale(Meta.Instance.Video.language.ToString());
    }
}
