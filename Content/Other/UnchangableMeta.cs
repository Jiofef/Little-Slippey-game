using Godot;
using System;
using Godot.Collections;
using static Meta;

public class UnchangableMeta : Node
{
    // Unchangable Meta is the saving singleton with the data which the player cannot directly change (not the settings, simply put)
    public static int[][] LevelRecords =
    {
        new int[G.LevelsInGameTotal],
        new int[G.LevelsInGameTotal],
        new int[G.LevelsInGameTotal]
    };
    public static Dictionary<string, object> GetJson()
    {
        int[][] sus = { new int[] { 3 } };
        return new Dictionary<string, object>()
        {
            {"level_records0", LevelRecords[0]},
            {"level_records1", LevelRecords[1]},
            {"level_records2", LevelRecords[2]}
        };
    }
    public static void SaveToFile()
    {
        const string SavePath = "user://save.json";
        File file = new File();
        var data = GetJson();
        var jsondata = JSON.Print(data);
        file.Open(SavePath, File.ModeFlags.Write);
        file.StoreString(jsondata);
        file.Close();
    }
    public static void LoadSave()
    {
        try
        {
            const string ReadPath = "user://save.json";

            File file = new File();
            if (!file.FileExists(ReadPath)) return;
            file.Open(ReadPath, File.ModeFlags.Read);

            var text = file.GetAsText();
            var model = JSON.Parse(text).Result as Dictionary;

            Godot.Collections.Array[] LevelRecordsArrays = new Godot.Collections.Array[3];
            for (int i = 0; i < LevelRecordsArrays.Length; i++)
                LevelRecordsArrays[i] = model["level_records" + i] as Godot.Collections.Array;
            for (int i = 0; i < LevelRecordsArrays.Length; i++)
                for (int j = 0; j < G.LevelsInGameTotal; j++)
                    LevelRecords[i][j] = Convert.ToInt32(LevelRecordsArrays[i][j]);
            file.Close();
        }
        catch {}
    }
}
