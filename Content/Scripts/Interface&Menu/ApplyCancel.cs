using Godot;

public partial class ApplyCancel : Control
{
    [Signal]
    public delegate void OptionsClosingEventHandler();
    public override void _Ready()
    {
        if (G.Scores != 0)
            Connect("OptionsClosing",new Callable(GetNode("../../.."),"OptionsClosing"));
        GetNode<TextureButton>("Cancel").GrabFocus();
    }
    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("Cancel"))
            Cancel();
    }
    public void Cancel()
    {
        Meta.Instance = Meta.OptionsReserve.Clone();
        Meta.Instance.ApplyOptions();
        if (G.Scores == 0)
            GetTree().ChangeSceneToFile("res://Content/Scenes/Interface&Menu/Menu.tscn");
        else
        {
            EmitSignal("OptionsClosing");
            GetParent().QueueFree();
        }
    }
    public void Apply()
    {
        Meta.Instance.SaveToFile();
        if (G.Scores == 0)
            GetTree().ChangeSceneToFile("res://Content/Scenes/Interface&Menu/Menu.tscn");
        else
        {
            EmitSignal("OptionsClosing");
            GetParent().QueueFree();
        }
    }
}