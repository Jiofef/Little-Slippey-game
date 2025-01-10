using Godot;
using Godot.Collections;
using System;
using static FileSystemExtension;

public partial class Meta : Node
{
    //Meta is the singleton that having various options for gameplay, video and other. It saves and when you enter the game, you will find the same settings that you set earlier

    public static Meta OptionsReserve = new Meta(); //Reserve in case user will click cancel options or will press escape button
    public static Meta Instance = new Meta(); //And default Meta Instance. Has applied options.

    public class SoundClass
    {
        public float[] BusVolumes = { 0.4f, 1, 1, 1, 1, 1, 1 }; //in this array #0 is Master, #1 - Interface, #2 - Music, #3 - Player, #4 - Crossounds, #5 - Crossexplosion
    }
    public SoundClass Sound = new SoundClass();


    public class VideoClass
    {
        public bool IsFullScreen = false;
        public Vector2I WindowSize = new Vector2I(1280, 720);
        public bool VSyncOn = false;
        public int MaxFrameRate = 60;
        public float CameraZoom = 1.25f;
        public byte ScoresLabelLocationX = 1, ScoresLabelLocationY = 0, ScoresShowingFormatIndex = 0;
        public bool CrossRotationWhenSpawning = true;
        public enum Language { en, ru }
        public Language language;
    }
    public VideoClass Video = new VideoClass();

    public class GameplayClass
    {
        public int Dificulty = 0;
        public readonly string[] DificultyNames = ["Hard", "Insane", "Inferno"];
        public bool[] AdditionStatuses = new bool[4];
        public int ChosenSkinIndex = 0;

        public bool IsSkinModded = false;
        public string ChosenModSkin;
    }
    public GameplayClass Gameplay = new GameplayClass();

    public void ApplyOptions()
    {
        for (int i = 0; i < Instance.Sound.BusVolumes.Length; i++)
            AudioServer.SetBusVolumeDb(i, Mathf.LinearToDb(Instance.Sound.BusVolumes[i]));
        DisplayServer.WindowSetMode(Instance.Video.IsFullScreen ? DisplayServer.WindowMode.Fullscreen : DisplayServer.WindowMode.Windowed);
        DisplayServer.WindowSetSize(Instance.Video.WindowSize);
        DisplayServer.WindowSetVsyncMode(Instance.Video.VSyncOn ? DisplayServer.VSyncMode.Enabled : DisplayServer.VSyncMode.Disabled);
        Engine.MaxFps = Instance.Video.MaxFrameRate;
        TranslationServer.SetLocale(Video.language.ToString());
        ProjectSettings.SetSetting("gui/theme/custom_font", "FontPath");
    }
    public Meta Clone()
    {
        Meta ReturnMeta = new Meta();

        //Sound
        for (int i = 0; i < Instance.Sound.BusVolumes.Length; i++)
            ReturnMeta.Sound.BusVolumes[i] = Sound.BusVolumes[i];

        //Video
        ReturnMeta.Video.IsFullScreen = Video.IsFullScreen;
        ReturnMeta.Video.WindowSize = Video.WindowSize;
        ReturnMeta.Video.VSyncOn = Video.VSyncOn;
        ReturnMeta.Video.MaxFrameRate = Video.MaxFrameRate;
        ReturnMeta.Video.ScoresShowingFormatIndex = Video.ScoresShowingFormatIndex;
        ReturnMeta.Video.CameraZoom = Video.CameraZoom;
        ReturnMeta.Video.ScoresLabelLocationX = Video.ScoresLabelLocationX;
        ReturnMeta.Video.ScoresLabelLocationY = Video.ScoresLabelLocationY;
        ReturnMeta.Video.CrossRotationWhenSpawning = Video.CrossRotationWhenSpawning;
        ReturnMeta.Video.language = Video.language;

        //Gameplay
        ReturnMeta.Gameplay.Dificulty = Gameplay.Dificulty;
        for (int i = 0; i < Instance.Gameplay.AdditionStatuses.Length; i++)
            ReturnMeta.Gameplay.AdditionStatuses[i] = Gameplay.AdditionStatuses[i];
        ReturnMeta.Gameplay.ChosenSkinIndex = Gameplay.ChosenSkinIndex;

        return ReturnMeta;
    }
    public Dictionary<string, Variant> GetJson()
    {
        return new Dictionary<string, Variant>()
        {
            {"bus_volumes", Sound.BusVolumes},
            {"is_full_screen", Video.IsFullScreen},
            {"window_size_x", Video.WindowSize.X},
            {"window_size_y", Video.WindowSize.Y},
            {"v_sync_on", Video.VSyncOn},
            {"max_frame_rate", Video.MaxFrameRate},
            {"scores_showing_format_index", Video.ScoresShowingFormatIndex},
            {"scores_label_location_x", Video.ScoresLabelLocationX},
            {"scores_label_location_y", Video.ScoresLabelLocationY},
            {"camera_zoom", Video.CameraZoom},
            {"cross_rotation_when_spawning", Video.CrossRotationWhenSpawning},
            {"language", Convert.ToInt32(Video.language)},
            {"dificulty", Gameplay.Dificulty},
            {"addition_status0", Gameplay.AdditionStatuses[0]},
            {"addition_status1", Gameplay.AdditionStatuses[1]},
            {"addition_status2", Gameplay.AdditionStatuses[2]},
            {"addition_status3", Gameplay.AdditionStatuses[3]},
            {"chosen_skin_index", Gameplay.ChosenSkinIndex},
        };
    }
    public static SoundClass GetDefaultSoundOptions()
    {
        SoundClass ReturnOptions = new SoundClass();

        ReturnOptions.BusVolumes = [0.4f, 1, 1, 1, 1, 1, 1];

        return ReturnOptions;
    }
    public static VideoClass GetDefaultVideoSettings()
    {
        VideoClass ReturnOptions = new VideoClass();

        ReturnOptions.IsFullScreen = false;
        ReturnOptions.WindowSize = new Vector2I(1280, 720);
        ReturnOptions.VSyncOn = false;
        ReturnOptions.MaxFrameRate = 60;
        ReturnOptions.CameraZoom = 1.25f;
        ReturnOptions.ScoresLabelLocationX = 1;
        ReturnOptions.ScoresLabelLocationY = 0;
        ReturnOptions.ScoresShowingFormatIndex = 0;
        ReturnOptions.CrossRotationWhenSpawning = true;
        ReturnOptions.language = VideoClass.Language.en;

        return ReturnOptions;
    }
    public static GameplayClass GetDefaultGameplaySettings()
    {
        GameplayClass ReturnOptions = new GameplayClass();

        ReturnOptions.Dificulty = 0;
        ReturnOptions.AdditionStatuses = new bool[4];
        ReturnOptions.ChosenSkinIndex = 0;
        ReturnOptions.IsSkinModded = false;
        ReturnOptions.ChosenModSkin = null;

        return ReturnOptions;
    }
    public void SaveToFile()
    {
        SaveInJson(Instance.GetJson(), "user://options.json");
    }
    public void LoadOptions()
    {
        try
        {
            var model = GetJsonModel("user://options.json");

            Godot.Collections.Array BusVolumesArray = (Godot.Collections.Array)model["bus_volumes"];
            for (int i = 0; i < BusVolumesArray.Count; i++)
                Sound.BusVolumes[i] = (float)BusVolumesArray[i];

            Video.IsFullScreen = (bool)model["is_full_screen"];
            Video.WindowSize = new Vector2I((int)model["window_size_x"], (int)model["window_size_y"]);
            Video.VSyncOn = (bool)model["v_sync_on"];
            Video.MaxFrameRate = (int)model["max_frame_rate"];
            Video.ScoresShowingFormatIndex = (byte)model["scores_showing_format_index"];
            Video.ScoresLabelLocationX = (byte)model["scores_label_location_x"];
            Video.ScoresLabelLocationY = (byte)model["scores_label_location_y"];
            Video.CrossRotationWhenSpawning = (bool)model["cross_rotation_when_spawning"];
            Video.CameraZoom = (float)model["camera_zoom"];
            Video.language = (VideoClass.Language)(int)(model["language"]);

            Gameplay.Dificulty = (int)model["dificulty"];

            Gameplay.ChosenSkinIndex = (int)model["chosen_skin_index"];

            for (int i = 0; i < Gameplay.AdditionStatuses.Length; i++)
                Gameplay.AdditionStatuses[i] = Convert.ToBoolean((string)model["addition_status" + i]);
        }
        catch{}
    }
}
