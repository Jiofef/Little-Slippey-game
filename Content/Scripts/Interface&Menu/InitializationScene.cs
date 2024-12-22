using Godot;
using Godot.Collections;
using GodotSteam;
using System.IO;
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

        
        Directory.CreateDirectory(ModLoader.DefaultModsPath); // Creating mod directory if it isn't exist

        Meta.Instance.LoadOptions();
        Meta.Instance.ApplyOptions();
        UnchangableMeta.LoadSave();


        if (!UnchangableMeta.IsLanguageSetted)
        {
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
        else
            GetTree().CallDeferred("change_scene_to_file", "res://Content/Scenes/Interface&Menu/WelcomeToGOS.tscn");


        //Mods loading
        if (!UnchangableMeta.DidModsCrushedTheGame)
        {
            UnchangableMeta.DidModsCrushedTheGame = true;
            UnchangableMeta.SaveToFile();

            ModLoader.LoadMods();

            UnchangableMeta.DidModsCrushedTheGame = false;
            UnchangableMeta.SaveToFile();
        }
        else
            ShowModErrorMessage();

    }

    public void SetLanguage(Language language)
    {
        Meta.Instance.Video.language = language;
        UnchangableMeta.IsLanguageSetted = true;
        Meta.Instance.ApplyOptions();
        Meta.Instance.SaveToFile();
        UnchangableMeta.SaveToFile();
        GetTree().ChangeSceneToFile("res://Content/Scenes/Interface&Menu/WelcomeToGOS.tscn");
    }

    public void ShowModErrorMessage()
    {
        //
    }
}
