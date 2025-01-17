using Godot;
using Godot.Collections;
using GodotSteam;
using System.IO;
using System.Linq;
using static Meta.VideoClass;

public partial class InitializationScene : Control
{
    private const uint _appId = 3288650;
    public override void _Ready()
	{
        OS.SetEnvironment("SteamAppId", _appId.ToString());
        OS.SetEnvironment("SteamGameId", _appId.ToString());

        Steam.RestartAppIfNecessary(_appId);
        Steam.SteamInit();

        string CountryCode = Steam.GetIPCountry();


        
        Directory.CreateDirectory(ModManager.DefaultModsPath); // Creating mod directory if it isn't exist
        Directory.CreateDirectory(ModDataManager.DefaultModLocalDataPath); // Creating mod local data directory if it isn't exist

        ModDataManager.CleanDeletedModsData();

        Meta.Instance.LoadOptions();
        Meta.Instance.ApplyOptions();
        UnchangableMeta.LoadSave();


        if (!UnchangableMeta.IsLanguageSetted)
        {
            ModManager.CreateStandartTimerLib();
            //GetNode<Control>("ChooseYourLanguage").Visible = true;
            //GetNode<TextureButton>("ChooseYourLanguage/ChooseYourLanguageEng").GrabFocus();
            Dictionary<string, Language> CountryCodes = new Dictionary <string, Language>()
            {
                {"RU", Language.ru},
                {"BY", Language.ru},
                {"UA", Language.ru},
                {"AM", Language.ru},
                {"MD", Language.ru},
                {"KG", Language.ru},
                {"KZ", Language.ru},
                {"GE", Language.ru},
                {"UZ", Language.ru},
            };
            if (CountryCodes.ContainsKey(CountryCode))
                SetLanguage(CountryCodes[CountryCode]);
            else 
                SetLanguage(Language.en);
        }


        //Mods loading
        if (!UnchangableMeta.DidModsCrushedTheGame && !ModManager.IsModsDisabled) // temporarily disabled
        {
            UnchangableMeta.DidModsCrushedTheGame = true;
            UnchangableMeta.SaveToFile();

            ModDataManager.LoadAllModDataSafely();
            ModDataManager.CleanDeletedModsData();
            ModDataManager.AddMissingDefaultModData();


            ModManager.LoadMods();

            UnchangableMeta.DidModsCrushedTheGame = false;
            UnchangableMeta.SaveToFile();
        }

        GetTree().CallDeferred("change_scene_to_file", "res://Content/Scenes/Interface&Menu/WelcomeToGOS.tscn");
    }

    public void SetLanguage(Language language)
    {
        Meta.Instance.Video.language = language;
        UnchangableMeta.IsLanguageSetted = true;
        Meta.Instance.ApplyOptions();
        Meta.Instance.SaveToFile();
        UnchangableMeta.SaveToFile();
    }
}
