using Godot;

[Tool]
public partial class AchievementsMenu : DraggableWindow
{
	public override void _Ready()
	{
		base._Ready();
        WindowTitle = Tr("Achievements") + " (" + (UnchangableMeta.AchievementsCount()) + " " + Tr("of") + " " + UnchangableMeta.AchievementStatuses.Length + ")";
        var achievementsContainer = GetNode<GridContainer>("MarginContainer/VBoxContainer/MarginContainer/AchievementsContainer/GridContainer");
		for (int i = 0; i < UnchangableMeta.AchievementStatuses.Length; i++)
		{
			var achievement = (Control)ResourceLoader.Load<PackedScene>("res://Content/Scenes/Achievements/Achievement" + (i + 1) + ".tscn").Instantiate();
			achievement.GetNode<Timer>("PopupVersionPart/PopupTimer").Disconnect("tree_exiting", new Callable(achievement, "TimerDeleted"));
			achievement.GetNode<Node2D>("PopupVersionPart").QueueFree();
            if (UnchangableMeta.AchievementStatuses[i] == 0)
			{
                achievement.GetNode<Label>("Name").QueueFree();
                achievement.GetNode<Node2D>("Sprite2D/Ratings").QueueFree();
                achievement.GetNode<Sprite2D>("Sprite2D/RewardBox1/RewardSprite").Modulate = new Color(0, 0, 0, 0.5f);
                achievement.GetNode<Sprite2D>("Sprite2D/RewardBox2/RewardSprite").Modulate = new Color(0, 0, 0, 0.5f);
                achievement.Modulate = new Color(0.5f, 0.5f, 0.5f);
				if (Achievements.IsAchievementHiden[i])
                    achievement.GetNode<RichTextLabel>("Text").Text = "[HIDDEN]";
            }
			achievementsContainer.AddChild(achievement);
		}
	}
}
