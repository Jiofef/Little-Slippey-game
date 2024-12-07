using Godot;

public partial class AchievementArea : Area2D
{
    [Export] int _achievementIndex;

    new public void AreaEntered()
    {
        Achievements.GetAchievement(_achievementIndex);
    }
}
