
using Godot;

[Tool]
public partial class TimerLibNotFound : ConfirmationWindow
{
    public void DownloadFinished()
    {
        ModManager.CreateStandartTimerLib();
        QueueFree();
    }
}
