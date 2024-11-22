using Godot;
using Godot.Collections;
using GodotSteam;
using System.IO;

public partial class InitializationScene : Control
{
    private const uint _appId = 3288650;
    private readonly string _country = Steam.GetIPCountry();
    public override void _Ready()
	{
        OS.SetEnvironment("SteamAppId", _appId.ToString());
        OS.SetEnvironment("SteamGameId", _appId.ToString());

        Steam.RestartAppIfNecessary(_appId);
        Steam.SteamInit();



        Directory.CreateDirectory(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + @"\Godot\app_userdata\Little Slippey\mods\");
        Meta.Instance.LoadOptions();
        Meta.Instance.ApplyOptions();
        UnchangableMeta.LoadSave();
        if (!UnchangableMeta.IsLanguageSetted)
        {
            //GetNode<Control>("ChooseYourLanguage").Visible = true;
            //GetNode<TextureButton>("ChooseYourLanguage/ChooseYourLanguageEng").GrabFocus();
            Dictionary<string, int> CountryCodes = new Dictionary <string, int>()
            {
                {"RU", 1},
                {"BY", 1},
                {"UA", 1},
                {"AM", 1},
                {"MD", 1},
                {"KG", 1},
                {"KZ", 1},
                {"GE", 1},
                {"UZ", 1},
            };
            if (CountryCodes.ContainsKey(_country))
                SetLanguage(CountryCodes[_country]);
            else 
                SetLanguage(0);
        }
        else
            GetTree().CallDeferred("change_scene_to_file", "res://Content/Scenes/Interface&Menu/WelcomeToGOS.tscn");
    }

    public void SetLanguage(int languageNumber)
    {
        Meta.Instance.language = (Meta.Language)languageNumber;
        UnchangableMeta.IsLanguageSetted = true;
        Meta.Instance.ApplyOptions();
        Meta.Instance.SaveToFile();
        UnchangableMeta.SaveToFile();
        GetTree().ChangeSceneToFile("res://Content/Scenes/Interface&Menu/WelcomeToGOS.tscn");
    }
}
