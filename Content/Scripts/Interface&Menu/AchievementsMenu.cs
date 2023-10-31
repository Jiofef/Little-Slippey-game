using Godot;

public partial class AchievementsMenu : Control
{
	public override void _Ready()
	{
		var achievementsContainer = GetNode<GridContainer>("AchievementsContainer/GridContainer");
		for (int i = 0; i < UnchangableMeta.AchievementStatuses.Length; i++)
		{
			var achievement = (Control)ResourceLoader.Load<PackedScene>("res://Content/Scenes/Achievements/Achievement" + (i + 1) + ".tscn").Instantiate();
			if (UnchangableMeta.AchievementStatuses[i] == 0)
			{
                achievement.GetNode<Label>("Name").QueueFree();
                achievement.GetNode<Sprite2D>("RewardSprite").Modulate = new Color(0, 0, 0, 0.5f);
				achievement.Modulate = new Color(0.5f, 0.5f, 0.5f);
				if (G.IsAchievementHiden[i])
                    achievement.GetNode<RichTextLabel>("Text").Text = "[HIDDEN]";
            }
			achievementsContainer.AddChild(achievement);
		}
	}
}
