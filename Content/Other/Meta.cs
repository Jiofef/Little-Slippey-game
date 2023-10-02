using Godot;
using Godot.Collections;

public partial class Meta : Node
{
    //Meta is the singleton that having various options for gameplay, video and other. It saves and when you enter the game, you will find the same settings that you set earlier

    //Reserve in case user will click cancel options or will pressed escape button
    public static Meta OptionsReserve = new Meta();
    //And default Meta Instance
    public static Meta Instance = new Meta();

    //SoundsVolume
    //in this array #0 is _master, #1 _interface, #2 _music, #3 _player, #4 _crossounds, #5 _crossexplosion
    public float[] BusVolumes = { -8, 0, 0, 0, 0, 0, 0};

    //VideoOptions
    //Since this is the HTML version, it will be incorrect to save the full screen state, and this is the only non-saving variable in the meta
    public bool IsFullScreen = false;
    public int ScoresShowingFormatIndex = 0;
    public float CameraZoom = 1.25f;

    //Gameplay
    public int Dificulty = 0;
    public bool EnhancedCrossesAtAllLevels = false;
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
        Meta returnmeta = new Meta();
        for (int i = 0; i < Instance.BusVolumes.Length; i++)
            returnmeta.BusVolumes[i] = BusVolumes[i];
        returnmeta.Dificulty = Dificulty;
        returnmeta.IsFullScreen = IsFullScreen;
        return returnmeta;
    }
    public Dictionary<string, Variant> GetJson()
    {
        return new Dictionary<string, Variant>()
        {
            {"bus_volumes", BusVolumes},
            {"scores_showing_format_index", ScoresShowingFormatIndex},
            {"dificulty", Dificulty},
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

            Array BusVolumesArray = (Array)model["bus_volumes"];
            for (int i = 0; i < BusVolumesArray.Count; i++)
                BusVolumes[i] = (float)BusVolumesArray[i];

            ScoresShowingFormatIndex = Dificulty = (int)model["scores_showing_format_index"]; ;

            Dificulty = (int)model["dificulty"];

            file.Close();
        }
        catch{}
    }
}
