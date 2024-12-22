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
        return (ModType)Enum.Parse(typeof(ModType), value);
    }
    //


    // Mods loading
    public static void LoadMods()
    {
        foreach (string directory in GetModDirectories())
        {
            if (!File.Exists(directory + @"/mod_info.json")) continue;

            var model = FileSystemExtension.GetJsonModel(directory + @"/mod_info.json");


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

    public class CreateModParams
    {
        public ModType modType;
        public string FolderName;
        public string ModName;
        public bool CreateAdditionalFolders = true;
    }
    public static string CreateAMod(CreateModParams @params)
    {
        var Path = DefaultModsPath + @params.FolderName;
        if (!Directory.Exists(Path))
        {
            switch (@params.modType)
            {
                case ModType.map:
                    CreateAModBase(@params, Path);
                    FileSystemExtension.CopyResourceFileTo("res://Content/Scenes/Other/UserLevelLayout.tscn", Path + @"\MainScene.tscn");
                    break;
                case ModType.localization:
                    CreateAModBase(@params, Path);
                    break;
                case ModType.skin:
                    CreateAModBase(@params, Path);
                    break;
                case ModType.resource:
                    CreateAModBase(@params, Path);
                    break;
                case ModType.content:
                    CreateAModBase(@params, Path);
                    break;
            }
            return Path;
        }
        else
            return "";
    }

    private static void CreateAModBase(CreateModParams @params, string path)
    {
        Directory.CreateDirectory(path + @"\");
        if (@params.CreateAdditionalFolders)
            foreach (string value in new string[] { "", @"\Other", @"\Scenes", @"\Scripts", @"\Sounds", @"\Sprites" })
                Directory.CreateDirectory(path + value);


        FileSystemExtension.CopyByteResourceFileTo("res://Content/Sprites/Interface/CustomMapDefaultPreview.png", path + @"\PreviewPicture.png");

        var modPreview = (ModPreview)ResourceLoader.Load<PackedScene>("res://Content/Scenes/Interface&Menu/ModPreviewLayout.tscn").Instantiate();
        modPreview._resourcePath = path + @"\PreviewPicture.png";


        var ToSave = new PackedScene();
        ToSave.Pack(modPreview);
        ResourceSaver.Save(ToSave, path + @"\ModPreview.tscn");

        var jsonText = "{\r\n  " +
            "\"name\": \"" + @params.ModName + "\",\r\n  " +
            "\"mod_type\":  \"" + @params.modType.ToString() + "\",\r\n  " +
            "\"description\": \"\"\r\n}";
        FileSystemExtension.SaveInJson(jsonText, path + @"\mod_info.json");
    }
    //
}
