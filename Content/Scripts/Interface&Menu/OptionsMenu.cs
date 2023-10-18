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
            Connect("OptionsClosing", new Callable(GetNode(".."), "OptionsClosing"));
            Connect("OptionsClosing", new Callable(GetNode("../PlayPart/Level" + G.CurrentLevel + "/Player/Camera2D"), "OptionsChanged"));
        }
        GetNode<TextureButton>("CanvasLayer/Cancel").GrabFocus();


        string[] SliderNames = { "GlobalSlider", "InterfaceSlider", "MusicSlider", "PlayerSlider", "CrossSoundsSlider", "CrossExplosionSlider", "LevelSoundsSlider" };
        for (int i = 0; i < SliderNames.Length; i++)
            GetNode<Slider>("CanvasLayer/Sound/VBoxContainer/" + SliderNames[i]).Value = Meta.Instance.BusVolumes[i];


        Meta.Instance.IsFullScreen = DisplayServer.WindowGetMode() == DisplayServer.WindowMode.Fullscreen;
        GetNode<OptionButton>("CanvasLayer/Video/VBoxContainer/ScoresShowingFormat").Selected = Meta.Instance.ScoresShowingFormatIndex;
    }

    public void Cancel()
    {
        Meta.Instance = Meta.OptionsReserve.Clone();
        Meta.Instance.ApplyOptions();
        if (G.CurrentLevel == 0)
            GetTree().ChangeSceneToFile("res://Content/Scenes/Interface&Menu/Menu.tscn");
        else
        {
            EmitSignal("OptionsClosing");
            QueueFree();
        }
    }
    public void Apply()
    {
        Meta.Instance.SaveToFile();
        if (G.CurrentLevel == 0)
            GetTree().ChangeSceneToFile("res://Content/Scenes/Interface&Menu/Menu.tscn");
        else
        {
            EmitSignal("OptionsClosing");
            QueueFree();
        }
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
        GetNode<OptionButton>("CanvasLayer/Video/VBoxContainer/ScreenFormat").Selected = DisplayServer.WindowGetMode() == DisplayServer.WindowMode.Fullscreen ? 1 : 0;
        if (Input.IsActionJustPressed("Cancel"))
            Cancel();
    }

    //VideoOptions
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
