using Godot;
using System;

public class ApplyCancel : Control
{
    [Signal]
    public delegate void OptionsClosing();
    public override void _Ready()
    {
        if (G.Scores != 0)
            Connect("OptionsClosing", GetParent().GetParent().GetParent(), "OptionsClosing");
    }
    public override void _Process(float delta)
    {
        if (Input.IsActionJustPressed("Cancel"))
        {
            Cancel();
        }
    }
    public void Cancel()
    {
        Meta.Instance = Meta.OptionsReserve.Clone();
        Meta.Instance.ApplyOptions();
        if (G.Scores == 0)
            GetTree().ChangeScene("res://Content/Scenes/Interface&Menu/Menu.tscn");
        
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
            GetTree().ChangeScene("res://Content/Scenes/Interface&Menu/Menu.tscn");
        else
        {
            EmitSignal("OptionsClosing");
            GetParent().QueueFree();
        }
    }
}
