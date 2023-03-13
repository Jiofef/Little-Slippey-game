using Godot;
using System;

public partial class Titles : Control
{
    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("Cancel"))
            Cancel();
    }
    public void Cancel()
    {
        GetTree().ChangeSceneToFile("res://Content/Scenes/Interface&Menu/Menu.tscn");
    }
}
