using Godot;
using System;

public class Titles : Control
{
    public void Cancel()
    {
        GetTree().ChangeScene("res://Content/Scenes/Interface&Menu/Menu.tscn");
    }
}
