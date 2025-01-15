using Godot;
using Godot.Collections;
using System;
using System.IO;
using System.Linq;
using static FileSystemExtension;
using static ModManager;

public partial class ModDataManager : Node
{
    public static readonly string DefaultModLocalDataPath = OS.GetUserDataDir() + @"/mods_local_data/";
    /// <summary>
    /// EVERY mod must have ModSettings. If you want to get a list of folder names of all mods, you can do it via keys in ModSettings. (However, GOS must be initialized to do this)
    /// </summary>
    public static Dictionary ModSettings = [];
    public static Dictionary ModMapRecords = [];
    public static Dictionary ModStatuses = [];

    #region Saving
    public static void SaveModMapRecords()
    {
        SaveInJson(ModMapRecords, DefaultModLocalDataPath + "mod_map_records.json");
    }

    public static void SaveModSettings()
    {
        SaveInJson(ModSettings, DefaultModLocalDataPath + "mod_settings.json");
    }

    public static void SaveModStatuses()
    {
        SaveInJson(ModStatuses, DefaultModLocalDataPath + "mod_statuses.json");
    }

    public static void SaveAllModData()
    {
        SaveModMapRecords();
        SaveModSettings();
        SaveModStatuses();
    }
    #endregion

    #region Loading
    public static void LoadModMapRecords()
    {
        var model = GetJsonModel(DefaultModLocalDataPath + "mod_map_records.json");
        ModMapRecords = model;
    }

    public static void LoadModSettings()
    {
        var model = GetJsonModel(DefaultModLocalDataPath + "mod_settings.json");
        ModSettings = model;
    }

    public static void LoadModStatuses()
    {
        var model = GetJsonModel(DefaultModLocalDataPath + "mod_statuses.json");
        ModStatuses = model;
    }

    public static void LoadAllModData()
    {
        LoadModMapRecords();
        LoadModSettings();
        LoadModStatuses();
    }
    public static void LoadAllModDataSafely()
    {
        try { LoadModMapRecords(); } catch { }
        try { LoadModSettings(); } catch { }
        try { LoadModStatuses(); } catch { }
    }
    #endregion

    public static void CleanDeletedModsData()
    {
        var folderNames = GetModFolderNames();

        foreach (string modName in ModSettings.Keys.ToList())
        {
            if (!folderNames.Contains(modName))
            {
                if (ModSettings.ContainsKey(modName))
                {
                    ModSettings.Remove(modName);
                    SaveModSettings();
                }
                if (ModStatuses.ContainsKey(modName))
                {
                    ModStatuses.Remove(modName);
                    SaveModStatuses();
                }
                if (ModMapRecords.ContainsKey(modName))
                {
                    ModMapRecords.Remove(modName);
                    SaveModMapRecords();
                }
            }
        }
    }

    /// <summary>
    /// Returns true if the data has been updated and false if the key was already in the dictionary
    /// </summary>
    public static bool AddDefaultModData(string folderName, ModType modType)
    {
        if (ModSettings.ContainsKey(folderName)) return false; // To avoid overwriting the values
        ModSettings.Add(folderName, new Dictionary { });

        if (modType == ModType.map)
            ModMapRecords.Add(folderName, 0);
        else
            ModStatuses.Add(folderName, true); // The mod is enabled by default

        return true;
    }

    public static void AddMissingDefaultModData()
    {
        Dictionary model;
        ModType modType;
        bool[] UpdatedModTypes = new bool[ModTypesCount]; // :p
        foreach (string modDirectory in GetModDirectories())
        {
            model = GetJsonModel(modDirectory + "/mod_info.json");

            modType = StringToModType(model["mod_type"].ToString());

            if(AddDefaultModData(Path.GetFileName(modDirectory), modType))
                UpdatedModTypes[(int)modType] = true;
        }
        foreach (bool v in UpdatedModTypes)
        if (UpdatedModTypes.Contains(true)) // If any mod data was updated
        {
            SaveModSettings();

            if (UpdatedModTypes[(int)ModType.map])
                SaveModMapRecords();

            SaveModStatuses();
        }
    }

    #region Mod Settings
    /*
     * Options tree must look like this
     * 
     * -OptionsTree (value is null)
     *      -OptionsGroup2 (value is name, string)
     *          -Option1 (value is ModOptionData)
     *          -Option2
     *      -OptionsGroup2
     *          -Option1
    */
    public class ModOptionGroup : ICloneable
    {
        // I might make groups with groups later, so I won't make ModOptionData a mandatory value here
        public string Name;
        public ModOptionGroup(string name)
        {
            Name = name;
        }

        public object Clone()
        {
            ModOptionGroup clone = new(Name);
            return clone;
        }
    }
    public static OtherExtension.TreeNode<object> CreateDefaultOptionTree()
    {
        return new OtherExtension.TreeNode<object>(new ModOptionGroup("Options"));
    }

    public class ModOptionData() : ICloneable
    {
        public string Key = "key";
        public string Name = "Name";
        public string Description = "";
        public Variant DefaultValue;

        public enum ValueTypeEnum {
            Bool,
            Int, Float, Double,
            String, Char,
            Color,
            Enum}
        public enum OptionMethodEnum {
            CheckBox, CheckButton,
            SpinBox, HSlider,
            LineEdit, TextEdit,
            ColorPickerButton,
            OptionButton}
        public ValueTypeEnum ValueType = ValueTypeEnum.Bool;
        public OptionMethodEnum OptionMethod = OptionMethodEnum.SpinBox;

        public ModOptionTypeData modOptionTypeData;

        public object Clone()
        {
            ModOptionData Clone = new();

            Clone.Key = Key;
            Clone.Name = Name;
            
            Clone.Description = Description;
            Clone.ValueType = ValueType;
            Clone.OptionMethod = OptionMethod;

            Clone.modOptionTypeData = (ModOptionTypeData)modOptionTypeData?.Clone();

            return Clone;
        }
        private Dictionary<int, string[]> RelatedTypeMethods = new Dictionary<int, string[]>()
        {
            {0, ["CheckBox", "CheckButton"]},
            {1, ["SpinBox", "HSlider"]},
            {2, ["SpinBox", "HSlider"]},
            {3, ["SpinBox", "HSlider"]},
            {4, ["LineEdit", "TextEdit"]},
            {5, ["LineEdit", "TextEdit"]},
            {6, ["ColorPickerButton"]},
            {7, ["OptionButton"]},
        };
        public string[] GetRelatedTypeMethods()
        {
            return RelatedTypeMethods[(int)ValueType];
        }
    }

    public class ModOptionTypeData() : ICloneable
    {
        public object Clone()
        {
            ModOptionTypeData Clone = new();

            if (this is ModStringOptionData data)
            {
                ModStringOptionData stringTypeClone = (ModStringOptionData)Clone;

                stringTypeClone.MaxStringLength = data.MaxStringLength;
                stringTypeClone.PlaceholderText = data.PlaceholderText;
            }

            if (this is ModNumberOptionData data1)
            {
                ModNumberOptionData numberTypeClone = (ModNumberOptionData)Clone;

                numberTypeClone.MinNumValue = data1.MinNumValue;
                numberTypeClone.MaxNumValue = data1.MaxNumValue;

                numberTypeClone.RoundTo = data1.RoundTo;

                numberTypeClone.AllowGreater = data1.AllowGreater;
                numberTypeClone.AllowLesser = data1.AllowLesser;

            }

            if (this is ModEnumOptionData data2)
            {
                ModEnumOptionData enumTypeClone = (ModEnumOptionData)Clone;
            }

            return Clone;
        }
    }
    public class ModStringOptionData() : ModOptionTypeData
    {
        public int MaxStringLength = 256;
        public string PlaceholderText = "";
    }
    public class ModNumberOptionData() : ModOptionTypeData
    {
        public double MinNumValue = 0, MaxNumValue = 100;

        public double RoundTo = 0;

        public bool AllowLesser = false, AllowGreater = false;
    }
    public class ModEnumOptionData() : ModOptionTypeData
    {
        public string[] Values = new string[0];
    }
    #endregion
}
