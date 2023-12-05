using Godot;
using System;
using Godot.Collections;

public partial class UnchangableMeta : Node
{
    // Unchangable Meta is the saving singleton with the data which the player cannot directly change (not the settings, simply put)
    public static int[][] LevelRecords =
    {
        new int[G.LevelsInGameTotal],
        new int[G.LevelsInGameTotal],
        new int[G.LevelsInGameTotal]
    };

    //the values of the LevelsCompleteStatus array variables can be 0, 1, 2, 3. 0 means the level is not passed, 1 means it is passed at the minimum difficulty,
    //correspondingly 2 at the average and 3 at the maximum difficulty.
    public static int[] LevelCompleteStatus = new int[G.LevelsInGameTotal];
    public static byte[] LevelPlayedStatus = new byte[G.LevelsInGameTotal]; //I made it as byte[] because of retard Godot that can't save a boolean array >:(

    public static bool IsLanguageSetted = false, IsTutorialPlayed, IsLevel9PlatformSectionFirstTimeCompleted, IsLevel9PlatformSectionSkipAllowed, IsFakeLevel10SkipAllowed;
    public static float DeathsNumber = 0;
    public static byte[] AchievementStatuses = new byte[51]; // A G A I N

    public static int AchievementsCount()
    {
        int AchievementsCount = 0;
        for (int i = 0; i < AchievementStatuses.Length; i++)
            AchievementsCount += AchievementStatuses[i];
        return AchievementsCount;
    }

    public static void SaveRecords()
    {
        if ((int)G.Scores > LevelRecords[Meta.Instance.Dificulty][G.CurrentLevel - 1])
        {
            LevelRecords[Meta.Instance.Dificulty][G.CurrentLevel - 1] = (int)G.Scores;
            G.IsNewRecordReached = true;
            if (G.Scores >= 150 && Meta.Instance.Dificulty + 1 > LevelCompleteStatus[G.CurrentLevel - 1])
            {
                LevelCompleteStatus[G.CurrentLevel - 1] = Meta.Instance.Dificulty + 1;
                for (int i = 0; i <= Meta.Instance.Dificulty; i++)
                    G.GetAchievement(G.LevelCompletionAchievementNumbers[i][G.CurrentLevel - 1]);
            }
        }
        if (G.Scores >= 50)
            G.GetAchievement(7);
        if (G.Scores >= 150 && Meta.Instance.CameraZoom >= 2)
            G.GetAchievement(8);
        if (G.Scores >= 150 && G.CurrentLevel == 10)
        {
            G.GetAchievement(45);
            G.GetAchievement(46);
            G.GetAchievement(47);
        }
    }

    public static Dictionary<string, Variant> GetJson()
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
            {"level_played_status", LevelPlayedStatus},
            {"achievement_statuses", AchievementStatuses},
        };
    }
    public static void SaveToFile()
    {
        using FileAccess file = FileAccess.Open("user://save.json", FileAccess.ModeFlags.Write);
        file.StoreString(GetJson().ToString());
        file.Close();
    }
    public static void LoadSave()
    {
        try
        {
            using FileAccess file = FileAccess.Open("user://save.json", FileAccess.ModeFlags.Read);
            var model = Json.ParseString(file.GetAsText()).Obj as Dictionary;

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
                } catch {}

            Godot.Collections.Array LevelCompleteStatusArray = (Godot.Collections.Array)model["level_complete_status"];
            try
            {
                for (int i = 0; i < LevelCompleteStatus.Length; i++)
                    LevelCompleteStatus[i] = Convert.ToInt32(LevelCompleteStatusArray[i].ToString());
            } catch {}

            Godot.Collections.Array LevelPlayedStatusArray = (Godot.Collections.Array)model["level_played_status"];
            try
            {
                for (int i = 0; i < LevelPlayedStatus.Length; i++)
                    LevelPlayedStatus[i] = Convert.ToByte(LevelPlayedStatusArray[i].ToString());
            } catch {}

            IsLanguageSetted = (bool)model["is_language_setted"];
            IsTutorialPlayed = (bool)model["is_tutorial_played"];
            IsLevel9PlatformSectionFirstTimeCompleted = (bool)model["is_level9_platform_section_first_time_completed"];
            IsLevel9PlatformSectionSkipAllowed = (bool)model["is_level9_platform_section_skip_is_allowed"];
            IsFakeLevel10SkipAllowed = (bool)model["is_fake_level10_skip_allowed"];

            Godot.Collections.Array AchievementStatusesArray = (Godot.Collections.Array)model["achievement_statuses"];
            try
            {
                for (int i = 0; i < AchievementStatuses.Length; i++)
                    AchievementStatuses[i] = Convert.ToByte(AchievementStatusesArray[i].ToString());
            }
            catch {}

            file.Close();
        } catch{}
    }
}
