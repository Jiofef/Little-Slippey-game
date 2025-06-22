using Godot;

[Tool]
public partial class AchievementsMenu : DraggableWindow
{
	public override void _Ready()
	{
		base._Ready();

        WindowTitle = Tr("Achievements") + " (" + (Achievements.AchievementsCount()) + " " + Tr("of") + " " + Achievements.AllTheAchievements.Count + ")";
        var achievementsContainer = GetNode<GridContainer>("MarginContainer/VBoxContainer/MarginContainer/AchievementsContainer/GridContainer");

		// Spawning achievements
		foreach (var achievement in Achievements.AllTheAchievements)
		{
			var achievementNode = (Achievement)ResourceLoader.Load<PackedScene>("res://Content/Scenes/Achievements/" + achievement.Key + ".tscn").Instantiate();
			achievementNode.GetNode<Timer>("PopupVersionPart/PopupTimer").Disconnect("tree_exiting", new Callable(achievementNode, "TimerDeleted"));
			achievementNode.GetNode<Node2D>("PopupVersionPart").QueueFree();
            if (!achievement.Value.IsReceived)
			{
				achievementNode.SetRecieved(false);

				achievementNode.SetHiddenIfIsntRecieved(achievement.Value.IsHidden);
            }
			achievementsContainer.AddChild(achievementNode);
		}
	}
}
