using Godot;

[Tool]
public partial class TimerLibIsDisabled : ConfirmationWindow
{
    public override void Accept()
    {
        ModDataManager.ModStatuses["StandartTimerLib"] = true;
        ModDataManager.SaveModStatuses();
        QueueFree();
    }
}
