using Godot;

public partial class MovementTutorial : CanvasLayer
{
    [Signal]
    public delegate void MovementTutorialClosingEventHandler();
    public override void _Ready()
    {
        Connect("MovementTutorialClosing", new Callable(GetParent(), "MovementTutorialClosed"));
        GetNode<TextureButton>("Cancel").GrabFocus();
    }
    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("Cancel"))
        {
            Cancel();
        }
    }
    public void Cancel()
    {
        if (!UnchangableMeta.IsFirstTimePlayed)
        {
            Input.MouseMode = Input.MouseModeEnum.Hidden;
            UnchangableMeta.IsFirstTimePlayed = true;
            GetTree().Paused = false;
        }
        EmitSignal("MovementTutorialClosing");
        QueueFree();
    }
}
