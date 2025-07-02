using Godot;
using System;
using System.Text.Json;
using System.Collections.Generic;
using static OtherExtension.OtherTools;
using static OtherExtension.DicTools;
using System.Runtime.CompilerServices;

public partial class UnchangableMeta : Node
{
    // Unchangable Meta is the saving singleton with the data which the player cannot directly change (not the settings, simply put)
    private static int _goldenCrossesAmount;
    public static int GoldenCrossesAmount 
    { 
        get => _goldenCrossesAmount;
        set 
        {
            _goldenCrossesAmount = value;
            GoldenCrossesAmountChanged(value);
        }
    }
    public delegate void GoldenCrossesAmountChangedEventHandler(int value);
    public static event GoldenCrossesAmountChangedEventHandler GoldenCrossesAmountChanged = delegate { };
    public static void AddGoldenCrosses(int amount)
    {
        if (amount <= 0) return;

        GoldenCrossesAmount += amount;
        if (G.AdditionalGuiLayer != null)
        {
            G.AdditionalGuiLayer.OnGoldenCrossesRecieved(amount);
        }
    }
    public static Action CannotBuy;
    public static Action Bought;
    public static bool CanBuy(int cost)
    {
        bool canBuy = GoldenCrossesAmount >= cost;

        if (!canBuy)
        {
            CannotBuy?.Invoke();
        }

        return canBuy;
    }
    public static bool TryBuy(int cost)
    {
        bool result = CanBuy(cost);

        if (result)
        {
            GoldenCrossesAmount -= cost;
            Bought?.Invoke();
        }

        return result;
    }

    public static int[][] LevelRecords =
    {
        new int[G.LevelsInGameTotal],
        new int[G.LevelsInGameTotal],
        new int[G.LevelsInGameTotal]
    };
	public static int[] GetLevelRecords(int levelId)
	{
		int[] array =
		{
			LevelRecords[0][levelId],
			LevelRecords[1][levelId],
			LevelRecords[2][levelId]
		};
		return array;
	}

    //the values of the LevelsCompleteStatus array variables can be 0, 1, 2, 3. 0 means the level is not passed, 1 means it is passed at the minimum difficulty,
	//correspondingly 2 at the average and 3 at the maximum difficulty.
	public static int[] LevelCompleteStatus = new int[G.LevelsInGameTotal];
    public static int[] LevelPlayedStatus = new int[G.LevelsInGameTotal]; //I made it as byte[] because of retard Godot that can't save a boolean array >:( // UPD: On top of that, the JSON.Stringify method converts an array of bytes into a string. To avoid doing bdsm, I converted the bytes to int. Sorry.

    public static bool IsLanguageSetted = false, IsTutorialPlayed, IsLevel9PlatformSectionFirstTimeCompleted, IsLevel9PlatformSectionSkipAllowed, IsFakeLevel10SkipAllowed;
	

	public enum NotificationKeysEnum {RecycleBin, SkinsMenu, Additions};
    public static Dictionary<string, int> NotificationsAmount = new Dictionary<string, int>()
	{
		{"RecycleBin", 1},
		{"SkinsMenu", 0},
		{"Additions", 0},
	};

    public static bool DidModsCrushedTheGame = false;
    public static int[] HintsStatus = //1 - was showed. 0 - hasn't.
    {
        0, // id 0 is Level 2 standing penalty hint
    };
    public static long DeathsNumber = 0;

    public static void SaveRecords()
    {
        if (G.IsLevelVanilla)
            SaveVanillaRecords();
        else 
            SaveModMapRecords();

        if (G.Scores >= 50)
            Achievements.GetAchievement("You're getting somewhere");
        if (G.Scores >= G.LevelCompleteTime && Meta.Instance.Video.CameraZoom >= 2)
            Achievements.GetAchievement("I Have No Eyes, and I Must Oversee");
    }

    public static void SaveVanillaRecords()
    {
        int LastRecord = LevelRecords[Meta.Instance.Gameplay.Difficulty][G.CurrentLevel - 1];
        if ((int)G.Scores > LastRecord)
        {
            LevelRecords[Meta.Instance.Gameplay.Difficulty][G.CurrentLevel - 1] = (int)G.Scores;
            G.IsNewRecordReached = true;
            if (G.Scores >= G.LevelCompleteTime && Meta.Instance.Gameplay.Difficulty + 1 > LevelCompleteStatus[G.CurrentLevel - 1])
            {
                LevelCompleteStatus[G.CurrentLevel - 1] = Meta.Instance.Gameplay.Difficulty + 1;
                G.HasLevelBeenCompleted = true;
                Achievements.GetLevelAchievements();
            }
        }
    }
    public static void SaveModMapRecords()
    {
        string MapName = ModManager.CurrentModMapFolderName;

        if (MapName == "") return;

        if (!ModDataManager.ModMapRecords.ContainsKey(MapName))
            ModDataManager.ModMapRecords.Add(MapName, (int)G.Scores);
        else if ((int)G.Scores > ModDataManager.ModMapRecords[MapName].AsInt32()) // If scores larger than the record
        {
            G.IsNewRecordReached = true;

            ModDataManager.ModMapRecords[MapName] = (int)G.Scores;
            ModDataManager.SaveModMapRecords();
        }
    }

    public static Dictionary<string, object> GetJsonSave()
    {
        return new Dictionary<string, object>()
        {
            {"did_mods_crushed_the_game", DidModsCrushedTheGame},
            {"golden_crosses_amount", GoldenCrossesAmount},
            {"level_records0", LevelRecords[0]},
            {"level_records1", LevelRecords[1]},
            {"level_records2", LevelRecords[2]},
            {"level_complete_status", LevelCompleteStatus},
            {"level_played_status", LevelPlayedStatus},
            {"hints_status", HintsStatus},
            {"deaths_number", DeathsNumber},
            {"notifications_amount", NotificationsAmount},
            {"is_skin_bought_dic", Skins.IsSkinBoughtDic},

            {"is_language_setted", IsLanguageSetted},
            {"is_tutorial_played", IsTutorialPlayed },
            {"is_level9_platform_section_first_time_completed", IsLevel9PlatformSectionFirstTimeCompleted},
            {"is_level9_platform_section_skip_is_allowed", IsLevel9PlatformSectionSkipAllowed},
            {"is_fake_level10_skip_allowed", IsFakeLevel10SkipAllowed},
        };
    }

    private static Dictionary<string, object> _lastSave = new();
	/// <summary>
	/// If forceSave == false, saving is aborted if the data is the same as before except for some variables
	/// </summary>
	public static void SaveToFile(bool forceSave = false)
	{
		try
		{
			var SaveData = GetJsonSave();
			if (SaveData == null || SaveData.Count == 0)
			{
				GD.PrintErr("Error: Save data is empty! Aborting save.");
				goto skipSaving; // I was too lazy to figure out how to do it without goto. Yes, I’m a jerk, I’m a bad person. And what are you going to do? If you admit to someone that you have hacked into the code of my game, and also into this script for some reason, I think there will be more questions for you than for me
			}

			#region to avoid unnecessary overwriting of data unnecessarily (Yeah, guys, I'm worried about the kilobytes of overwriting your SSD.)
			if (!forceSave)
			{
				string[] exceptKeys = ["deaths_number"];
				if (AreDictionariesEqual(SaveData, _lastSave, exceptKeys))
				{
					GD.Print("There are no changes, the file has not been overwritten");
					goto skipSaving;
				}
			}
			_lastSave = SaveData.Copy();
			#endregion


			FileSystemExtension.SaveInJson(SaveData, "user://save.json");
			GD.Print("Save completed successfully.");
		}
		catch (Exception e)
		{
			GD.PrintErr("Error during save: " + e.Message);
		}
		skipSaving: 

		// Content data saving
		ContentManager.SaveToFile();
    }

    public static void LoadSave()
    {
        
        try
        {
            // Save file
            {
                var model = FileSystemExtension.GetSystemJsonModel("user://save.json");

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


                TryLoad<bool>("did_mods_crushed_the_game", value => DidModsCrushedTheGame = value);

                TryLoad<int>("golden_crosses_amount", value => GoldenCrossesAmount = value);

                for (int i = 0; i < LevelRecords.Length; i++)
                {
                    TryLoadArray($"level_records{i}", value => LevelRecords[i] = value, length => new int[length], LevelRecords[i].Length, $"level_records{i}");
                }

                TryLoadArray("level_complete_status", value => LevelCompleteStatus = value, length => new int[length], LevelCompleteStatus.Length);

                TryLoadArray("level_played_status", value => LevelPlayedStatus = value, length => new int[length], LevelPlayedStatus.Length);

                TryLoadArray("hints_status", value => HintsStatus = value, length => new int[length], HintsStatus.Length);

                TryLoadDictionary("notifications_amount", value => NotificationsAmount = value, () => NotificationsAmount, true, "notifications_amount");

                TryLoadDictionary("is_skin_bought_dic", value => Skins.IsSkinBoughtDic = value, () => Skins.IsSkinBoughtDic, true, "is_skin_bought_dic");


                TryLoad<long>("deaths_number", value => DeathsNumber = value);
                TryLoad<bool>("is_language_setted", value => IsLanguageSetted = value);
                TryLoad<bool>("is_tutorial_played", value => IsTutorialPlayed = value);
                TryLoad<bool>("is_level9_platform_section_first_time_completed", value => IsLevel9PlatformSectionFirstTimeCompleted = value);
                TryLoad<bool>("is_level9_platform_section_skip_is_allowed", value => IsLevel9PlatformSectionSkipAllowed = value);
                TryLoad<bool>("is_fake_level10_skip_allowed", value => IsFakeLevel10SkipAllowed = value);
            }
            //Achievements file
            Achievements.LoadAchievementStatuses();



        } catch (Exception e)
        { 
            GD.Print("Error during loading:" + e.Message); 
        }

    }


    // Saving the game when its closed through system
    public override void _Notification(int what)
    {
        if (what == NotificationWMCloseRequest)
        {
            if (!_enableAutoSaveWhenClosing) return;

            SaveToFile();
        }
    }

    // I did it to keep the bug I liked :P
    private static bool _enableAutoSaveWhenClosing = false; // Enabled in the initialization scene
    public static void SetAutoSaveWhenClosing(bool value)
    {
        _enableAutoSaveWhenClosing = value;
        GD.Print("_enableAutoSaveWhenClosing " + (value ? "enabled" : "disabled"));
    }
}
