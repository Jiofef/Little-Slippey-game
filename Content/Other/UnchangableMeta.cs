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

    public static bool DoFirstTimePlayed;

    public static void SaveRecords()
    {
        if ((int)G.Scores > LevelRecords[Meta.Instance.Dificulty][G.CurrentLevel - 1])
        {
            LevelRecords[Meta.Instance.Dificulty][G.CurrentLevel - 1] = (int)G.Scores;
            G.IsNewRecordReached = true;
            if (G.Scores > 150 && Meta.Instance.Dificulty + 1 > LevelCompleteStatus[G.CurrentLevel - 1])
                LevelCompleteStatus[G.CurrentLevel - 1] = Meta.Instance.Dificulty + 1;
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
            {"do_first_time_played", DoFirstTimePlayed}
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
                }
                catch { }

            Godot.Collections.Array LevelCompleteStatusArray = (Godot.Collections.Array)model["level_complete_status"];
            try
            {
                for (int i = 0; i < LevelCompleteStatus.Length; i++)
                {
                    LevelCompleteStatus[i] = Convert.ToInt32(LevelCompleteStatusArray[i].ToString());
                }
            }
            catch { }


            DoFirstTimePlayed = (bool)(model["do_first_time_played"]);

            file.Close();
        }
        catch{}
    }
}
