using Godot;

public partial class SoundSettings : Control
{
    public override void _Ready()
    {
        string[] SliderNames = {"GlobalSlider", "InterfaceSlider", "MusicSlider", "PlayerSlider", "CrossSnapSlider", "CrossOtherSlider", "CrossExplosionSlider", "LevelSoundsSlider"};
        for (int i = 0; i < SliderNames.Length; i++)
            GetNode<Slider>("VBoxContainer/" + SliderNames[i]).Value = Meta.Instance.BusVolumes[i];
    }
    private void SoundChanging(float value, int BusNubmer)
    {
        Meta.Instance.BusVolumes[BusNubmer] = value;
        AudioServer.SetBusVolumeDb(BusNubmer, value);
        AudioServer.SetBusMute(BusNubmer, value <= -30);
    }
}
