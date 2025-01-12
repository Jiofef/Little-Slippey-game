using Godot;
using Godot.Collections;
using System;
using System.IO;
using System.Text.Json;

public partial class ModManager : Node
{
    public static string CurrentModMapFolderName = ""; // Equals "" if the player is not on a mod map

    // Directories
    public static readonly string DefaultModsPath = OS.GetUserDataDir() + @"/mods/";
    public static string[] GetModDirectories()
    {
        return Directory.GetDirectories(DefaultModsPath);
    }
    public static string[] GetModFolderNames()
    {
        string[] modDirectories = GetModDirectories();
        string[] folderNames = new string[modDirectories.Length];

        for (int i = 0; i < folderNames.Length; i++)
            folderNames[i] = Path.GetFileName(modDirectories[i]);
        return folderNames;
    }
    //


    // Mod types
    public enum ModType { map, localization, skin, content, resource }
    public static readonly ModType DefaultModType = ModType.map;

    public static ModType StringToModType(string value)
    {
        return (ModType)Enum.Parse(typeof(ModType), value);
    }

    public static int ModTypesCount => Enum.GetValues(typeof(ModType)).Length;
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
    public static bool IsStandartTimerLibLoaded = true;
    public static void CreateStandartTimerLib()
    {
        var modParams = new CreateModParams(ModType.resource, "StandartTimerLib", "StandartTimerLib", false);
        modParams.Description = "Purpose: Standard library for timer synchronization and processing. It is forbidden to disable.";
        string ModPath = CreateAMod(modParams);
        if (ModPath != "")
        {
            string ToSave = "596f7520736572696f75733f";
            FileSystemExtension.SaveInJson(ToSave, ModPath + @"\_maintimermodule.json");
            FileSystemExtension.CopyByteResourceFileTo("res://Content/Sprites/Interface/StandartTimerLibPreview.png", ModPath + @"\PreviewPicture.png");
            IsStandartTimerLibLoaded = true;
        }
    }
    public static bool IsStandartTimerLibExists()
    {
        string Path = DefaultModsPath + @"StandartTimerLib\_maintimermodule.json";

        bool IsFileExists = File.Exists(Path);
        if (!IsFileExists)
            return false;

        using Godot.FileAccess file = Godot.FileAccess.Open(Path, Godot.FileAccess.ModeFlags.Read);
        var FileText = file.GetAsText();
        file.Close();
        return FileText == "596f7520736572696f75733f";
    }
    public static void UpdateIsStandartTimerLibLoaded()
    {

        bool isExists = IsStandartTimerLibExists();

        if (!isExists || !ModDataManager.ModStatuses.ContainsKey("StandartTimerLib"))
        {
            IsStandartTimerLibLoaded = false;
            return;
        }

        bool IsModOn = ModDataManager.ModStatuses["StandartTimerLib"].AsBool();
        IsStandartTimerLibLoaded = IsModOn;
    }
    //


    // Mod creation

    public class CreateModParams
    {
        public CreateModParams(ModType _modType, string _folderName, string _modName, bool _createAdditionalFolders)
        {
            modType = _modType;
            FolderName = _folderName;
            ModName = _modName;
            CreateAdditionalFolders = _createAdditionalFolders;
        }
        public CreateModParams(ModType _modType, string _folderName, string _modName)
        {
            modType = _modType;
            FolderName = _folderName;
            ModName = _modName;
        }
        public CreateModParams() { }

        public ModType modType;
        public string FolderName;
        public string ModName;
        public bool CreateAdditionalFolders = true;
        public string Description = "Description";
    }
    public static string CreateAMod(CreateModParams @params)
    {
        var Path = DefaultModsPath + @params.FolderName;
        if (!Directory.Exists(Path))
        {
            try
            {
                switch (@params.modType)
                {
                    case ModType.map:
                        CreateAModBase(@params, Path);
                        FileSystemExtension.CopyResourceFileTo("res://Content/Scenes/Other/UserLevelLayout.tscn", Path + @"\MainScene.tscn");
                        break;
                    case ModType.localization:
                        CreateAModBase(@params, Path);
                        ModDataManager.ModStatuses.Add(@params.FolderName, true);
                        break;
                    case ModType.skin:
                        CreateAModBase(@params, Path);
                        ModDataManager.ModStatuses.Add(@params.FolderName, true);
                        break;
                    case ModType.resource:
                        CreateAModBase(@params, Path);
                        ModDataManager.ModStatuses.Add(@params.FolderName, true);
                        break;
                    case ModType.content:
                        CreateAModBase(@params, Path);
                        ModDataManager.ModStatuses.Add(@params.FolderName, true);
                        break;
                }
                return Path;

            }
            catch { return Directory.Exists(Path) ? Path : ""; }

        }
        else
            return "";
    }

    public static Dictionary<ModType, string> DefaultModTypeNames = new Dictionary<ModType, string>
    {
        {ModType.map, "CustomMap"},
        {ModType.skin, "CustomSkin"},
        {ModType.localization, "Localization"},
        {ModType.resource, "ResourceMod"},
        {ModType.content, "ContentMod"},
    };

    //public static System.Collections.Generic.Dictionary<string, ModDataManager.ModOptionData>[] GetNewModOptionsDictionaryArray()
    //{
    //    var array = new System.Collections.Generic.Dictionary<string, ModDataManager.ModOptionData>[1];
    //    array[0].Add("Options");
    //    return array;
    //}


    public static System.Collections.Generic.Dictionary<string, object> CreateModInfoJson(CreateModParams @params)
    {
        return new System.Collections.Generic.Dictionary<string, object>()
        {
            {"name", @params.ModName},
            {"mod_type", @params.modType.ToString()},
            {"description", @params.Description},
            {"mod_options", null},
            
        };
    }

    private static void CreateAModBase(CreateModParams @params, string path)
    {
        Directory.CreateDirectory(path + @"\");
        if (@params.CreateAdditionalFolders)
            foreach (string value in new string[] { "", @"\Other", @"\Scenes", @"\Scripts", @"\Sounds", @"\Sprites" })
                Directory.CreateDirectory(path + value);


        FileSystemExtension.CopyByteResourceFileTo("res://Content/Sprites/Interface/" + DefaultModTypeNames[@params.modType] + "DefaultPreview.png", path + @"\PreviewPicture.png");

        var modPreview = (ModPreview)ResourceLoader.Load<PackedScene>("res://Content/Scenes/Interface&Menu/ModPreviewLayout.tscn").Instantiate();
        modPreview._resourcePath = path + @"\PreviewPicture.png";


        var ToSave = new PackedScene();
        ToSave.Pack(modPreview);
        ResourceSaver.Save(ToSave, path + @"\ModPreview.tscn");

        FileSystemExtension.SaveInJson(CreateModInfoJson(@params), path + "/mod_info.json");


        // Creating directory keypairs
        ModDataManager.AddDefaultModData(@params.FolderName, @params.modType);

        ModDataManager.SaveModSettings();

        if (@params.modType == ModType.map)
            ModDataManager.SaveModMapRecords();
        else
            ModDataManager.SaveModStatuses();
    }
    //
}
