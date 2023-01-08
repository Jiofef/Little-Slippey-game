using Godot;
using System;

public class SoundSettings : Control
{
    public override void _Ready()
    {
        string[] SliderNames = {"GlobalSlider", "InterfaceSlider", "MusicSlider", "PlayerSlider", "CrossSnapSlider", "CrossOtherSlider", "CrossExplosionSlider"};
        for (int i = 0; i < SliderNames.Length; i++)
            GetNode<Slider>("VBoxContainer/" + SliderNames[i]).Value = Meta.Instance.BusVolumes[i];
    }
    public void GlobalChanged(float value)
    {
        SoundChanging(0, value);
    }
    public void InterfaceChanged(float value)
    {
        SoundChanging(1, value);
    }
    public void MusicChanged(float value)
    {
        SoundChanging(2, value);
    }
    public void PlayerChanged(float value)
    {
        SoundChanging(3, value);
    }
    public void CrossSnapChanged(float value)
    {
        SoundChanging(4, value);
    }
    public void CrossOtherChanged(float value)
    {
        SoundChanging(5, value);
    }
    public void CrossExplosionChanged(float value)
    {
        SoundChanging(6, value);
    }
    private void SoundChanging(int BusNubmer, float value)
    {
        if (value > -29)
        {
            Meta.Instance.BusVolumes[BusNubmer] = value;
            Meta.Instance.DoBusMuted[BusNubmer] = false;
            AudioServer.SetBusVolumeDb(BusNubmer, value);
            AudioServer.SetBusMute(BusNubmer, false);
        }
        else
        {
            Meta.Instance.BusVolumes[BusNubmer] = value;
            Meta.Instance.DoBusMuted[BusNubmer] = true;
            AudioServer.SetBusVolumeDb(BusNubmer, value);
            AudioServer.SetBusMute(BusNubmer, true);
        }
    }
}
