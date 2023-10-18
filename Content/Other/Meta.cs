using Godot;
using Godot.Collections;
using System;

public partial class Meta : Node
{
    //Meta is the singleton that having various options for gameplay, video and other. It saves and when you enter the game, you will find the same settings that you set earlier

    public static Meta OptionsReserve = new Meta(); //Reserve in case user will click cancel options or will pressed escape button
    public static Meta Instance = new Meta(); //And default Meta Instance

    //SoundsVolume
    public float[] BusVolumes = { -8, 0, 0, 0, 0, 0, 0}; //in this array #0 is _master, #1 _interface, #2 _music, #3 _player, #4 _crossounds, #5 _crossexplosion

    //VideoOptions
    //Since this is the HTML version, it will be incorrect to save the full screen state, and this is the only non-saving variable in the meta
    public bool IsFullScreen = false;
    public int ScoresShowingFormatIndex = 0;
    public float CameraZoom = 1.25f;

    //Gameplay
    public int Dificulty = 0;
    public bool[] AdditionStatuses = new bool[4];

    public void ApplyOptions()
    {
        for (int i = 0; i < Instance.BusVolumes.Length; i++)
        {
            AudioServer.SetBusVolumeDb(i, Instance.BusVolumes[i]);
            AudioServer.SetBusMute(i, Instance.BusVolumes[i] <= -30);
        }
        DisplayServer.WindowSetMode(Instance.IsFullScreen ? DisplayServer.WindowMode.Fullscreen : DisplayServer.WindowMode.Windowed);
    }
    public Meta Clone()
    {
        Meta ReturnMeta = new Meta();
        for (int i = 0; i < Instance.BusVolumes.Length; i++)
            ReturnMeta.BusVolumes[i] = BusVolumes[i];
        ReturnMeta.Dificulty = Dificulty;
        ReturnMeta.IsFullScreen = IsFullScreen;
        ReturnMeta.ScoresShowingFormatIndex = ScoresShowingFormatIndex;
        ReturnMeta.CameraZoom = CameraZoom;
        for (int i = 0; i < Instance.AdditionStatuses.Length; i++)
            ReturnMeta.AdditionStatuses[i] = AdditionStatuses[i];
        return ReturnMeta;
    }
    public Dictionary<string, Variant> GetJson()
    {
        return new Dictionary<string, Variant>()
        {
            {"bus_volumes", BusVolumes},
            {"scores_showing_format_index", ScoresShowingFormatIndex},
            {"camera_zoom", CameraZoom},
            {"dificulty", Dificulty},
            {"addition_status0", AdditionStatuses[0]},
            {"addition_status1", AdditionStatuses[1]},
            {"addition_status2", AdditionStatuses[2]},
            {"addition_status3", AdditionStatuses[3]},
        };
    }
    public void SaveToFile()
    {
        using FileAccess file = FileAccess.Open("user://options.json", FileAccess.ModeFlags.Write);
        file.StoreString(Instance.GetJson().ToString());
        file.Close();
    }
    public void LoadOptions()
    {
        try
        {
            using FileAccess file = FileAccess.Open("user://options.json", FileAccess.ModeFlags.Read);
            var text = file.GetAsText();
            var model = Json.ParseString(file.GetAsText()).Obj as Dictionary;

            Godot.Collections.Array BusVolumesArray = (Godot.Collections.Array)model["bus_volumes"];
            for (int i = 0; i < BusVolumesArray.Count; i++)
                BusVolumes[i] = (float)BusVolumesArray[i];

            ScoresShowingFormatIndex = (int)model["scores_showing_format_index"];
            CameraZoom = (float)model["camera_zoom"];

            Dificulty = (int)model["dificulty"];

            for (int i = 0; i < AdditionStatuses.Length; i++)
                AdditionStatuses[i] = Convert.ToBoolean((string)model["addition_status" + i]);

            file.Close();
        }
        catch{}
    }
}
