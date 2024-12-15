using Godot;

public partial class AchievementArea : Area2D
{
    [Export] string _achievementName;

    new public void AreaEntered()
    {
        Achievements.GetAchievement(_achievementName);
    }
}
