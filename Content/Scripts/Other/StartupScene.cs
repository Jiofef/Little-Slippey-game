using Godot;

public class StartupScene : Node2D
{
    public override void _Ready()
    {
        OS.WindowSize = new Vector2(1280, 720);
        Meta.Instance.LoadOptions();
        Meta.Instance.ApplyOptions();
        UnchangableMeta.LoadSave();
        GetTree().ChangeScene("res://Content/Scenes/Interface&Menu/Menu.tscn");
    }
}