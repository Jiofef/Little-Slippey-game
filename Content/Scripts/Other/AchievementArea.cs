using Godot;

public partial class AchievementArea : Area2D
{
    [Export] int _achievementIndex;

    public void AreaEntered()
    {
        Achievements.GetAchievement(_achievementIndex);
    }
}
