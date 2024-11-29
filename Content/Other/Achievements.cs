using Godot;
using System;

public partial class Achievements : Node
{
    //Achievements singleton. WARNING. BEING HERE CAN CAUSE HEAD ACHE, DIZZINESS, VOMITING, AND ALSO CAN PROVOKE AIDS AND ACUTE FORM OF PROSTATE CANCER. You have been warned.
    public static readonly int[][] LevelCompletionAchievementNumbers =
    {
        new int[] {10, 13, 16, 19, 22, 25, 28, 33, 38, 41},
        new int[] {11, 14, 17, 20, 23, 26, 29, 34, 39, 42},
        new int[] {12, 15, 18, 21, 24, 27, 30, 35, 40, 43}
    };
    public static readonly bool[] IsAchievementHiden = { false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, true, false, false, false, false, false, false, false, true, true, true, true, true, true, false, false };
    public static CanvasLayer CurrentPopupAchievementsLayer;
    public static int AchievementPopupTimerMultiplier = 0;
    public static void GetAchievement(int index) // Do not ruin someone else's experience and do not give away game achievements for nothing. If you are making a cheat map or mod, mark it in the title
    {
        if (UnchangableMeta.AchievementStatuses[index] == 1) return;
        var achievement = (Control)ResourceLoader.Load<PackedScene>("res://Content/Scenes/Achievements/Achievement" + (index + 1) + ".tscn").Instantiate();
        CurrentPopupAchievementsLayer.AddChild(achievement);
        achievement.GetNode<Timer>("PopupVersionPart/PopupTimer").Start(0.05f + 0.3f * AchievementPopupTimerMultiplier);
        achievement.FocusMode = Control.FocusModeEnum.None;
        achievement.MouseFilter = Control.MouseFilterEnum.Ignore;
        UnchangableMeta.AchievementStatuses[index] = 1;
        UnchangableMeta.SaveToFile();
        AchievementPopupTimerMultiplier++;

        if (UnchangableMeta.AchievementsCount() == (UnchangableMeta.AchievementStatuses.Length - 1))
            GetAchievement(51);
    } 
}
