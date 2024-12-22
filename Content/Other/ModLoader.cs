using Godot;
using System;
using System.IO;
using static ModLoader;

public partial class ModLoader : Node
{
    bool IsStandartTimerLibLoaded = false, IsStandartTimerLibExists;
    // Directories
    public static readonly string DefaultModsPath = OS.GetUserDataDir() + @"/mods/";
    public static string[] GetModDirectories()
    {
        return Directory.GetDirectories(DefaultModsPath);
    }
    //


    // Mod types
    public enum ModType { map, localization, skin, content, resource }

    public static ModType StringToModType(string value)
    {
        return ModType.resource;
    }
    //


    // Mods loading
    public static void LoadMods()
    {
        foreach (string directory in GetModDirectories())
        {
            if (!File.Exists(directory + @"/mod_info.json")) continue;

            var model = FileSystemExtension.GetJsonModel(directory + @"/mod_info.json");

            GD.Print(directory);

            var modType = StringToModType((string)model["mod_type"]);
            switch (modType)
            {
                case ModType.map:
                    // nothing literly :L
                    break;
                case ModType.localization:
                    break;
                case ModType.skin:
                    break;
                case ModType.resource:
                    break;
                case ModType.content:
                    break;
            }
        }
    }

    public static Node BuildNode()
    {
        // I'm afraid if I don't find an adequate way to load scenes created in runtime from a file, I'm going to have to commit some code crimes.
        return new Node();
    }
    //


    //To create StandartTimerLib mod if it doesn't exist
    public static void CreateStandartTimerLib()
    {
        Directory.CreateDirectory(DefaultModsPath + @"\");
    }
    //


    // Mod creation
    public static string CreateAMod(ModType modType, string folderName, string modName)
    {
        var Path = DefaultModsPath + folderName;
        if (!Directory.Exists(Path))
        {
            switch (modType)
            {
                case ModType.map:
                    CreateAMap(Path, modName);
                    break;
                case ModType.localization:
                    break;
                case ModType.skin:
                    break;
                case ModType.resource:
                    break;
                case ModType.content:
                    break;
            }
            return Path;
        }
        else
            return "";
    }

    private static void CreateAMap(string path, string modName)
    {
        foreach (string value in new string[] { "", @"\Other", @"\Scenes", @"\Scripts", @"\Sounds", @"\Sprites" })
            Directory.CreateDirectory(path + value);
        ResourceSaver.Save(ResourceLoader.Load("res://Content/Scenes/Other/UserLevelLayout.tscn"), path + @"\MainScene.tscn");
        FileSystemExtension.CopyResourceFileTo("res://Content/Scenes/Other/UserLevelLayout.tscn", path + @"\MainScene.tscn");
        FileSystemExtension.CopyByteResourceFileTo("res://Content/Sprites/Interface/CustomMapDefaultPreview.png", path + @"\PreviewPicture.png");


        var modPreview = (ModPreview)ResourceLoader.Load<PackedScene>("res://Content/Scenes/Interface&Menu/ModPreviewLayout.tscn").Instantiate();
        modPreview._resourcePath = path + @"\PreviewPicture.png";


        var ToSave = new PackedScene();
        ToSave.Pack(modPreview);
        ResourceSaver.Save(ToSave, path + @"\ModPreview.tscn");

        var jsonText = "{\r\n  \"name\": \"" + modName + "\", \"mod_type\":  \"map\",\r\n  \"description\": \"\"\r\n}"; // TO FIX!!!
        FileSystemExtension.SaveInJson(jsonText, path + @"\mod_info.json");
        GD.Print(path);
    }
    //
}
