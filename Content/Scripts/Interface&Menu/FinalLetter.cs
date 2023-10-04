using Godot;

public partial class FinalLetter : Node2D
{
    public override void _Ready()
    {
        Input.MouseMode = Input.MouseModeEnum.Visible;
    }
    public void Close()
    {
        G.IsCrossesEnabled = true;
        G.IsProgressPaused = false;
        G.CrossSpawnMultiplier = 1;
        G.ResetValues();
        G.CurrentLevel = 0;
        GetTree().ChangeSceneToFile("res://Content/Scenes/Interface&Menu/Menu.tscn");
    }
}
