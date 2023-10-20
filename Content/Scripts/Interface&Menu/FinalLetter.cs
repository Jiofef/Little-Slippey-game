using Godot;

public partial class FinalLetter : Node2D
{
    public override void _Ready()
    {
        Input.MouseMode = Input.MouseModeEnum.Visible;
        UnchangableMeta.SaveRecords();
        UnchangableMeta.SaveToFile();
        G.CompletelyResetValues();
        GetTree().ChangeSceneToFile("res://Content/Scenes/Interface&Menu/Menu.tscn");
    }
    public void Close()
    {
        G.CompletelyResetValues();
        GetTree().ChangeSceneToFile("res://Content/Scenes/Interface&Menu/Menu.tscn");
    }
}
