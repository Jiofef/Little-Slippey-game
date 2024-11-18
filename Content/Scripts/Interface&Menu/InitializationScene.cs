using Godot;
using GodotSteam;
using System.IO;

public partial class InitializationScene : Control
{
    private const uint AppId = 3288650;
    public override void _Ready()
	{
        OS.SetEnvironment("SteamAppId", AppId.ToString());
        OS.SetEnvironment("SteamGameId", AppId.ToString());

        Steam.RestartAppIfNecessary(AppId);
        Steam.SteamInit();


        var isSteamRunning = Steam.IsSteamRunning();
        if (!isSteamRunning)
        {
            GD.Print("Steam is not running.");
            return;
        }
        else
        {
            var steamId = Steam.GetSteamID();
            var name = Steam.GetFriendPersonaName(steamId);

            GD.Print("Your Steam Name: " + name);
        }



        Directory.CreateDirectory(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + @"\Godot\app_userdata\Little Slippey\mods\");
        Meta.Instance.LoadOptions();
        Meta.Instance.ApplyOptions();
        UnchangableMeta.LoadSave();
        if (!UnchangableMeta.IsLanguageSetted)
        {
            GetNode<Control>("ChooseYourLanguage").Visible = true;
            GetNode<TextureButton>("ChooseYourLanguage/ChooseYourLanguageEng").GrabFocus();
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
