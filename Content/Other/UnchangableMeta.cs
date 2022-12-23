using Godot;
using System;
using Godot.Collections;
using static Meta;

public class UnchangableMeta : Node
{
    // Unchangable Meta is the saving singleton with the data which the player cannot directly change (not the settings, simply put)
    public static int[][] _levelrecords =
    {
        new int[G._levelstotal],
        new int[G._levelstotal],
        new int[G._levelstotal]
    };
    public static Dictionary<string, object> GetJson()
    {
        int[][] sus = { new int[] { 3 } };
        return new Dictionary<string, object>()
        {
            {"_levelrecords1", _levelrecords[0]},
            {"_levelrecords2", _levelrecords[1]},
            { "_levelrecords3", _levelrecords[2]}
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
            const string ReadPath = "user://save.json";

            File file = new File();
            if (!file.FileExists(ReadPath)) return;
            file.Open(ReadPath, File.ModeFlags.Read);

            var text = file.GetAsText();
            var model = JSON.Parse(text).Result as Dictionary;

            var LevelRecordsArray1 = model["_levelrecords1"] as Godot.Collections.Array;
            for (int i = 0; i < LevelRecordsArray1.Count; i++)
                _levelrecords[0][i] = Convert.ToInt32(LevelRecordsArray1[i]);
            var LevelRecordsArray2 = model["_levelrecords2"] as Godot.Collections.Array;
            for (int i = 0; i < LevelRecordsArray2.Count; i++)
                _levelrecords[1][i] = Convert.ToInt32(LevelRecordsArray2[i]);
            var LevelRecordsArray3 = model["_levelrecords3"] as Godot.Collections.Array;
            for (int i = 0; i < LevelRecordsArray3.Count; i++)
                _levelrecords[2][i] = Convert.ToInt32(LevelRecordsArray3[i]);

            file.Close();
    }
}
