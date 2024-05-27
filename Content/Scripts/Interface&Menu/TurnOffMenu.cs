using Godot;
using System;

public partial class TurnOffMenu : Control
{
    [Signal] public delegate void ClosingEventHandler();
    public void Accept()
    {
        UnchangableMeta.SaveToFile();
        Meta.Instance.SaveToFile();
        GetTree().Quit();
    }
    public void Cancel()
    {
        Connect("Closing", new Callable(GetParent(), "OpenedMenuClosed"));
        EmitSignal("Closing");
        QueueFree();
    }
}
