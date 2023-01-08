using Godot;
using Godot.Collections;
using System;
using System.Linq;

public class Meta : Node
{
    //Meta is the singleton that having various options for gameplay, video and other. It saves and when you enter the game, you will find the same settings that you set earlier

    //Reserve in case user will click cancel options or will pressed escape button
    public static Meta OptionsReserve = new Meta();
    //And default Meta Instance
    public static Meta Instance = new Meta();

    //SoundsVolume
    //in this array #0 is _master, #1 _interface, #2 _music, #3 _player, #4 _crosssnap, #5 _crossother, %6 _crossexplosion
    public float[] BusVolumes = { -8, 0, 0, 0, -15, 0, 0 };
    public bool[] DoBusMuted = new bool[7];

    //VideoOptions
    //Since this is the HTML version, it will be incorrect to save the full screen state, and this is the only non-saving variable in the meta
    public bool IsFullScreen = false;

    //Gameplay
    public byte Dificulty = 0;
    public void ApplyOptions()
    {
        for (int i = 0; i < Instance.BusVolumes.Length; i++)
        {
            AudioServer.SetBusVolumeDb(i, Instance.BusVolumes[i]);
            AudioServer.SetBusMute(i, Instance.DoBusMuted[i]);
        }

        OS.WindowFullscreen = Meta.Instance.IsFullScreen;
    }
    public Meta Clone()
    {
        Meta returnmeta = new Meta();
        for (int i = 0; i < Instance.BusVolumes.Length; i++)
        {
            returnmeta.BusVolumes[i] = BusVolumes[i];
            returnmeta.DoBusMuted[i] = DoBusMuted[i];
        }
        returnmeta.Dificulty = Dificulty;
        returnmeta.IsFullScreen = IsFullScreen;
        return returnmeta;
    }
    public Dictionary<string, object> GetJson()
    {
        return new Dictionary<string, object>()
        {
            {"bus_volumes", BusVolumes},
            {"do_bus_muted0",  DoBusMuted[0]},
            {"do_bus_muted1",  DoBusMuted[1]},
            {"do_bus_muted2",  DoBusMuted[2]},
            {"do_bus_muted3",  DoBusMuted[3]},
            {"do_bus_muted4",  DoBusMuted[4]},
            {"do_bus_muted5",  DoBusMuted[5]},
            {"do_bus_muted6",  DoBusMuted[6]},
            {"dificulty", Dificulty}
        };
    }
    public void SaveToFile()
    {
        const string SavePath = "user://options.json";
        File file = new File();
        var data = Instance.GetJson();
        var jsondata = JSON.Print(data);
        file.Open(SavePath, File.ModeFlags.Write);
        file.StoreString(jsondata);
        file.Close();
    }
    public void LoadOptions()
    {
        try
        {
            const string ReadPath = "user://options.json";

            File file = new File();
            if (!file.FileExists(ReadPath)) return;
            file.Open(ReadPath, File.ModeFlags.Read);
            var text = file.GetAsText();
            var model = JSON.Parse(text).Result as Dictionary;

            var BusVolumesArray = model["bus_volumes"] as Godot.Collections.Array;
            for (int i = 0; i < BusVolumesArray.Count; i++)
                BusVolumes[i] = (float)BusVolumesArray[i];

            for (int i = 0; i < 7; i++)
                DoBusMuted[i] = (bool)model["do_bus_muted" + i];

            Dificulty = Convert.ToByte(model["dificulty"]);

            file.Close();
        }
        catch{}
    }
}