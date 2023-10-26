using Godot;
using System;

public partial class OptionsMenu : Control
{
    [Signal]
    public delegate void OptionsClosingEventHandler();

    public override void _Ready()
    {
        Meta.OptionsReserve = Meta.Instance.Clone();
        if (G.CurrentLevel != 0)
        {
            Connect("OptionsClosing", new Callable(GetNode("../.."), "OptionsClosing"));
            Connect("OptionsClosing", new Callable(GetNode("../../PlayPart/Level" + G.CurrentLevel + "/Player/Camera2D"), "OptionsChanged"));
            GetNode<TextureButton>("DeclineButton").GrabFocus();
        }
        else
            Connect("tree_exited", new Callable(GetParent(), "OpenedMenuClosed"));

        string[] SliderNames = { "Global", "Interface", "Music", "Player", "Crosses", "Explosions", "Environment" };
        for (int i = 0; i < SliderNames.Length; i++)
            GetNode<Slider>("SoundContainer/" + SliderNames[i] + "Slider").Value = Meta.Instance.BusVolumes[i];


        Meta.Instance.IsFullScreen = DisplayServer.WindowGetMode() == DisplayServer.WindowMode.Fullscreen;

        string[] ScoresShowingFormats = {"Default", "Mini", "Hide"};
        GetNode<CheckBox>("VideoContainer/Control/" + ScoresShowingFormats[Meta.Instance.ScoresShowingFormatIndex] + "CheckBox").ButtonPressed = true;

        GetNode<Slider>("VideoContainer/CameraZoomSlider").Value = Meta.Instance.CameraZoom;

        GetNode<AnimationPlayer>("AnimationPlayer").Play("Appearance");
    }

    public void Cancel()
    {
        Meta.Instance = Meta.OptionsReserve.Clone();
        Meta.Instance.ApplyOptions();
        EmitSignal("OptionsClosing");
        QueueFree();
    }
    public void Accept()
    {
        Meta.Instance.SaveToFile();
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
        GetNode<CheckBox>("VideoContainer/ScreenModeContainer/" + WindowModes[DisplayServer.WindowGetMode() == DisplayServer.WindowMode.Fullscreen ? 0 : 1] + "CheckBox").ButtonPressed = true;
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
        Meta.Instance.ScoresShowingFormatIndex = index;
    }
    public void CameraZoomChanged(float value)
    {
        Meta.Instance.CameraZoom = value;
    }
}
