using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;
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
        public bool IsFullScreen = true;
        public Vector2I WindowSize = new Vector2I(1280, 720);
        public bool VSyncOn = false;
        public int MaxFrameRate = 60;
        public float CameraZoom = 1.25f;
        public byte ScoresLabelLocationX = 1, ScoresLabelLocationY = 0, ScoresShowingFormatIndex = 0;
        public bool CrossRotationWhenSpawning = true;
        public bool ExplosionBloom = true;
        public enum Language { en, ru }
        public Language language;
    }
    public VideoClass Video = new VideoClass();

    public class GameplayClass
    {
        public int Dificulty = 0;
        public readonly string[] DificultyNames = ["Hard", "Insane", "Inferno"];
        public bool[] AdditionStatuses = new bool[4];
        public string ChosenSkinKey = "Slippey";

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
        ReturnMeta.Video.ExplosionBloom = Video.ExplosionBloom;
        ReturnMeta.Video.language = Video.language;

        //Gameplay
        ReturnMeta.Gameplay.Dificulty = Gameplay.Dificulty;
        for (int i = 0; i < Instance.Gameplay.AdditionStatuses.Length; i++)
            ReturnMeta.Gameplay.AdditionStatuses[i] = Gameplay.AdditionStatuses[i];
        ReturnMeta.Gameplay.ChosenSkinKey = Gameplay.ChosenSkinKey;

        return ReturnMeta;
    }
    public System.Collections.Generic.Dictionary<string, object> GetJson()
    {
        return new System.Collections.Generic.Dictionary<string, object>
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
            {"explosion_bloom", Video.ExplosionBloom},
            {"language", Convert.ToInt32(Video.language)},
            {"dificulty", Gameplay.Dificulty},
            {"addition_statuses", Gameplay.AdditionStatuses},
            {"chosen_skin_key", Gameplay.ChosenSkinKey},
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
        ReturnOptions.ChosenSkinKey = "Slippey";
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
        var model = GetSystemJsonModel("user://options.json");

        if (model == null) return;

        void TryLoad<T>(string key, Action<T> setValue, string errorLog = null)
        {
            try
            {
                var loadedValue = JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(model[key]));
                setValue(loadedValue);
            }
            catch
            {
                GD.Print(errorLog ?? key);
            }
        }
        void TryLoadArray<T>(string key, Action<T[]> setValue, Func<int, T[]> defaultArray, int expectedLength, string errorLog = null)
        {
            try
            {
                var loadedArray = JsonSerializer.Deserialize<T[]>(JsonSerializer.Serialize(model[key]));
                var newArray = defaultArray(expectedLength);

                if (loadedArray != null)
                {
                    for (int i = 0; i < Math.Min(loadedArray.Length, expectedLength); i++)
                    {
                        newArray[i] = loadedArray[i];
                    }
                }

                setValue(newArray);
            }
            catch
            {
                GD.Print(errorLog ?? key);
            }
        }
        void TryLoadDictionary<TKey, TValue>(string key, Action<Dictionary<TKey, TValue>> setValue, Func<Dictionary<TKey, TValue>> getDefaultDictionary, bool doNotAddUnknownValues = false, string errorLog = null)
        {
            try
            {
                var loadedDictionary = JsonSerializer.Deserialize<Dictionary<TKey, TValue>>(JsonSerializer.Serialize(model[key]));

                var defaultDictionary = getDefaultDictionary();

                if (loadedDictionary != null)
                {
                    foreach (var kvp in loadedDictionary)
                    {
                        if (defaultDictionary.ContainsKey(kvp.Key))
                            defaultDictionary[kvp.Key] = kvp.Value;
                        else if (!doNotAddUnknownValues)
                            defaultDictionary.Add(kvp.Key, kvp.Value);
                    }
                }

                setValue(defaultDictionary);
            }
            catch
            {
                GD.Print(errorLog ?? key);

                setValue(getDefaultDictionary());
            }
        }

        TryLoadArray( "bus_volumes", value => Sound.BusVolumes = value,length => new float[length], Sound.BusVolumes.Length,"bus_volumes");

        TryLoad<bool>("is_full_screen", value => Video.IsFullScreen = value, "is_full_screen");
        TryLoad<int>("window_size_x", value => Video.WindowSize.X = value, "window_size_x");
        TryLoad<int>("window_size_y", value => Video.WindowSize.Y = value, "window_size_y");
        TryLoad<bool>("v_sync_on",value => Video.VSyncOn = value,"v_sync_on");
        TryLoad<int>("max_frame_rate", value => Video.MaxFrameRate = value, "max_frame_rate");
        TryLoad<byte>("scores_showing_format_index",value => Video.ScoresShowingFormatIndex = value,"scores_showing_format_index");
        TryLoad<byte>("scores_label_location_x", value => Video.ScoresLabelLocationX = value, "scores_label_location_x");
        TryLoad<byte>("scores_label_location_y", value => Video.ScoresLabelLocationY = value, "scores_label_location_y");
        TryLoad<bool>( "cross_rotation_when_spawning", value => Video.CrossRotationWhenSpawning = value,"cross_rotation_when_spawning" );
        TryLoad<bool>("explosion_bloom", value => Video.ExplosionBloom = value, "explosion_bloom");
        TryLoad<float>("camera_zoom", value => Video.CameraZoom = value,"camera_zoom" );
        TryLoad<int>("language",value => Video.language = (VideoClass.Language)value,"language" );

        TryLoad<int>("dificulty",value => Gameplay.Dificulty = value, "dificulty");
        TryLoad<string>("chosen_skin_key", value => Gameplay.ChosenSkinKey = value, "chosen_skin_key");
        TryLoadArray("addition_statuses",  value => Gameplay.AdditionStatuses = value, length => new bool[length], Gameplay.AdditionStatuses.Length, "addition_statuses");
    }
}
