using Godot;
using GodotSteam;

[Tool]
public partial class TurnOffMenu : ConfirmationWindow
{
    public override void Accept()
    {
        UnchangableMeta.SaveToFile(true);
        Meta.Instance.SaveToFile();
        Steam.SteamShutdown();
        GetTree().Quit();
    }
}
