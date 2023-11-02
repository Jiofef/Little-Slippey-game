using Godot;
using System;

public partial class OptionsMenu : Control
{
    [Signal]
    public delegate void OptionsClosingEventHandler();
    [Signal]
    public delegate void GUIOptionsChangedEventHandler();

    public override void _Ready()
    {
        Meta.OptionsReserve = Meta.Instance.Clone();
        if (G.CurrentLevel != 0)
        {
            Connect("OptionsClosing", new Callable(GetNode("../.."), "OptionsClosing"));
            Connect("GUIOptionsChanged", new Callable(GetNode("../../PlayPart").GetChild(0).GetNode("Player/Camera2D"), "ApplyGUIOptions"));
            GetNode<TextureButton>("DeclineButton").GrabFocus();
        }

        string[] SliderNames = { "Global", "Interface", "Music", "Player", "Crosses", "Explosions", "Environment" };
        for (int i = 0; i < SliderNames.Length; i++)
            GetNode<Slider>("SoundContainer/" + SliderNames[i] + "Slider").Value = Meta.Instance.BusVolumes[i];


        Meta.Instance.IsFullScreen = DisplayServer.WindowGetMode() == DisplayServer.WindowMode.Fullscreen;

        string[] ScoresShowingFormats = {"Default", "Mini", "Hide"};
        GetNode<CheckBox>("VideoContainer/GridContainer/ScoresLabelContainer/" + ScoresShowingFormats[Meta.Instance.ScoresShowingFormatIndex] + "CheckBox").ButtonPressed = true;
        GetNode<CheckBox>("VideoContainer/GridContainer/GridContainer/CheckBox" + (Meta.Instance.ScoresLabelLocationX + 1) + "X" + (Meta.Instance.ScoresLabelLocationY + 1) + "Y").ButtonPressed = true;

        GetNode<Slider>("VideoContainer/GridContainer/CameraZoomSlider").Value = Meta.Instance.CameraZoom;

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
        Meta.Instance.SaveToFile();
        if (G.CurrentLevel == 0)
            Connect("OptionsClosing", new Callable(GetParent(), "OpenedMenuClosed"));
        EmitSignal("OptionsClosing");
        QueueFree();
    }

    //SoundOptions
    public void SoundChanging(float value, int BusNubmer)
    {
        Meta.Instance.BusVolumes[BusNubmer] = value;
        AudioServer.SetBusVolumeDb(BusNubmer, value);
        AudioServer.SetBusMute(BusNubmer, value <= -30);
    }

    public override void _PhysicsProcess(double delta)
    {
        string[] WindowModes = { "Full", "Window" };
        GetNode<CheckBox>("VideoContainer/GridContainer/ScreenModeContainer/" + WindowModes[DisplayServer.WindowGetMode() == DisplayServer.WindowMode.Fullscreen ? 0 : 1] + "CheckBox").ButtonPressed = true;
        if (Input.IsActionJustPressed("Cancel"))
            Cancel();
    }

    //VideoOptions
    public void ChangeScreenFormat(int index)
    {
        Meta.Instance.IsFullScreen = Convert.ToBoolean(index);
        DisplayServer.WindowSetMode(index == 0 ? DisplayServer.WindowMode.Fullscreen : DisplayServer.WindowMode.Windowed);
    }
    public void ChangeScoresShowingFormat(int index)
    {
        Meta.Instance.ScoresShowingFormatIndex = (byte)index;
        EmitSignal("GUIOptionsChanged", false);
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
        EmitSignal("GUIOptionsChanged", false);
    }
}
