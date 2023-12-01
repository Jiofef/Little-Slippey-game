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
        G.CurrentPopupAchievementsLayer = GetNode<CanvasLayer>("PopupAchievementsLayer");
    }
    public void Close()
    {
        G.CompletelyResetValues();
        GetTree().ChangeSceneToFile("res://Content/Scenes/Interface&Menu/WelcomeToGOS.tscn");
    }
    public void ACriminalAgainstHumanity()
    {
        G.GetAchievement(48);
    }
}
