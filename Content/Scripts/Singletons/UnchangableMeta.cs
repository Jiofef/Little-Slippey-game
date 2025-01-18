using Godot;
using System;
using Godot.Collections;
using System.Text.Json;

public partial class UnchangableMeta : Node
{
    // Unchangable Meta is the saving singleton with the data which the player cannot directly change (not the settings, simply put)
    public static int GoldenCrossesAmount = 0;

    public static int[][] LevelRecords =
    {
        new int[G.LevelsInGameTotal],
        new int[G.LevelsInGameTotal],
        new int[G.LevelsInGameTotal]
    };

    //the values of the LevelsCompleteStatus array variables can be 0, 1, 2, 3. 0 means the level is not passed, 1 means it is passed at the minimum difficulty,
    //correspondingly 2 at the average and 3 at the maximum difficulty.
    public static int[] LevelCompleteStatus = new int[G.LevelsInGameTotal];
    public static int[] LevelPlayedStatus = new int[G.LevelsInGameTotal]; //I made it as byte[] because of retard Godot that can't save a boolean array >:( // UPD: On top of that, the JSON.Stringify method converts an array of bytes into a string. To avoid doing bdsm, I converted the bytes to int. Sorry.

    public static bool IsLanguageSetted = false, IsTutorialPlayed, IsLevel9PlatformSectionFirstTimeCompleted, IsLevel9PlatformSectionSkipAllowed, IsFakeLevel10SkipAllowed, IsThereNewContentInRecycleBin = true;
    public static bool DidModsCrushedTheGame = false;
    public static int[] HintsStatus = //1 - was showed. 0 - hasn't.
    {
        0, // id 0 is Level 2 standing penalty hint
    };
    public static float DeathsNumber = 0;

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
        int LastRecord = LevelRecords[Meta.Instance.Gameplay.Dificulty][G.CurrentLevel - 1];
        if ((int)G.Scores > LastRecord)
        {
            LevelRecords[Meta.Instance.Gameplay.Dificulty][G.CurrentLevel - 1] = (int)G.Scores;
            G.IsNewRecordReached = true;
            if (G.Scores >= G.LevelCompleteTime && Meta.Instance.Gameplay.Dificulty + 1 > LevelCompleteStatus[G.CurrentLevel - 1])
            {
                LevelCompleteStatus[G.CurrentLevel - 1] = Meta.Instance.Gameplay.Dificulty + 1;
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

    public static Dictionary<string, Variant> GetJsonSave()
    {
        return new Dictionary<string, Variant>()
        {
            {"level_records0", LevelRecords[0]},
            {"level_records1", LevelRecords[1]},
            {"level_records2", LevelRecords[2]},
            {"level_complete_status", LevelCompleteStatus},
            {"is_language_setted", IsLanguageSetted},
            {"is_tutorial_played", IsTutorialPlayed },
            {"is_level9_platform_section_first_time_completed", IsLevel9PlatformSectionFirstTimeCompleted},
            {"is_level9_platform_section_skip_is_allowed", IsLevel9PlatformSectionSkipAllowed},
            {"is_fake_level10_skip_allowed", IsFakeLevel10SkipAllowed},
            {"is_there_new_content_in_recycle_bin", IsThereNewContentInRecycleBin},
            {"level_played_status", LevelPlayedStatus},
            {"hints_status", HintsStatus},
            {"did_mods_crushed_the_game", DidModsCrushedTheGame},
        };
    }
    public static void SaveToFile()
    {
        try
        {
            var SaveData = GetJsonSave();
            FileSystemExtension.SaveInJson(SaveData, "user://save.json");
        }
        catch { }
    }
    public static void LoadSave()
    {

        try
        {
            // Save file
            {
                var model = FileSystemExtension.GetJsonModel("user://save.json");

                try
                {
                    GoldenCrossesAmount = model["golden_crosses_amount"].AsInt32();
                } catch { }

                Godot.Collections.Array[] LevelRecordsArrays = new Godot.Collections.Array[3];
                for (int i = 0; i < LevelRecordsArrays.Length; i++)
                    LevelRecordsArrays[i] = (Godot.Collections.Array)model["level_records" + i];
                for (int i = 0; i < LevelRecordsArrays.Length; i++)
                    try
                    {
                        for (int j = 0; j < G.LevelsInGameTotal; j++)
                        {
                            LevelRecords[i][j] = Convert.ToInt32(LevelRecordsArrays[i][j].ToString());
                        }
                    }
                    catch { }

                Godot.Collections.Array LevelCompleteStatusArray = (Godot.Collections.Array)model["level_complete_status"];
                try
                {
                    for (int i = 0; i < LevelCompleteStatus.Length; i++)
                        LevelCompleteStatus[i] = Convert.ToInt32(LevelCompleteStatusArray[i].ToString());
                }
                catch { }

                Godot.Collections.Array LevelPlayedStatusArray = (Godot.Collections.Array)model["level_played_status"];
                try
                {
                    for (int i = 0; i < LevelPlayedStatus.Length; i++)
                        LevelPlayedStatus[i] = Convert.ToInt32(LevelPlayedStatusArray[i].ToString());
                }
                catch { }

                Godot.Collections.Array HintsStatusArray = (Godot.Collections.Array)model["hints_status"];
                try
                {
                    for (int i = 0; i < HintsStatus.Length; i++)
                        HintsStatus[i] = Convert.ToInt32(HintsStatusArray[i].ToString());
                }
                catch { }

                IsLanguageSetted = (bool)model["is_language_setted"];
                IsTutorialPlayed = (bool)model["is_tutorial_played"];
                IsLevel9PlatformSectionFirstTimeCompleted = (bool)model["is_level9_platform_section_first_time_completed"];
                IsLevel9PlatformSectionSkipAllowed = (bool)model["is_level9_platform_section_skip_is_allowed"];
                IsFakeLevel10SkipAllowed = (bool)model["is_fake_level10_skip_allowed"];
                IsThereNewContentInRecycleBin = (bool)model["is_there_new_content_in_recycle_bin"];
                DidModsCrushedTheGame = (bool)model["did_mods_crushed_the_game"];
            }


            //Achievements file
            {
                using FileAccess achievements = FileAccess.Open("user://achievements.json", FileAccess.ModeFlags.Read);

                var dictionary = JsonSerializer.Deserialize<System.Collections.Generic.Dictionary<string, Achievements.Data>>(achievements.GetAsText());

                foreach (var achievement in dictionary)
                {
                    try
                    {
                        Achievements.AllTheAchievements[achievement.Key].IsReceived = achievement.Value.IsReceived;
                    }
                    catch { }
                }
            }



        } catch{}

    }
}
