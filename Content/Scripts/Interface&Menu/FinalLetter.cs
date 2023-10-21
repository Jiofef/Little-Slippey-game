using Godot;

public partial class FinalLetter : Node2D
{
    public override void _Ready()
    {
        G._isLevel10Finaling = false;
        Input.MouseMode = Input.MouseModeEnum.Visible;
        UnchangableMeta.SaveRecords();
        UnchangableMeta.SaveToFile();
        G.CompletelyResetValues();
    }
    public void Close()
    {
        G.CompletelyResetValues();
        GetTree().ChangeSceneToFile("res://Content/Scenes/Interface&Menu/WelcomeToGOS.tscn");
    }
}
