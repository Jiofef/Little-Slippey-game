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
    public static bool DoFirstTimePlayed;
    public static Dictionary<string, Variant> GetJson()
    {
        return new Dictionary<string, Variant>()
        {
            {"level_records0", LevelRecords[0]},
            {"level_records1", LevelRecords[1]},
            {"level_records2", LevelRecords[2]},
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
                for (int j = 0; j < G.LevelsInGameTotal; j++)
                    LevelRecords[i][j] = Convert.ToInt32(LevelRecordsArrays[i][j].ToString());
            DoFirstTimePlayed = (bool)(model["do_first_time_played"]);

            file.Close();
        }
        catch{}
    }
}
