using Godot;
using System;
public class StartupScene : Node2D
{
    public override void _Ready()
    {
        Meta.Instance.LoadOptions();
        Meta.Instance.ApplyOptions();
        UnchangableMeta.LoadSave();
        GetTree().ChangeScene("res://Content/Scenes/Interface&Menu/Menu.tscn");
    }
}