using Godot;

public partial class FinalLetter : Node2D
{
    public override void _Ready()
    {
        Input.MouseMode = Input.MouseModeEnum.Visible;
        UnchangableMeta.SaveRecords();
        UnchangableMeta.SaveToFile();
        G.CompletelyResetValues();
        Achievements.CurrentPopupAchievementsLayer = GetNode<CanvasLayer>("PopupAchievementsLayer");
    }
    public void Close()
    {
        G.CompletelyResetValues();
        GetTree().ChangeSceneToFile("res://Content/Scenes/Interface&Menu/WelcomeToGOS.tscn");
    }
    public void ACriminalAgainstHumanity()
    {
        Achievements.GetAchievement(49);
    }
}
