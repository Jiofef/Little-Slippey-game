using Godot;

[Tool]
public partial class AchievementsMenu : DraggableWindow
{
	public override void _Ready()
	{
		base._Ready();
        WindowTitle = Tr("Achievements") + " (" + (Achievements.AchievementsCount()) + " " + Tr("of") + " " + Achievements.AllTheAchievements.Count + ")";
        var achievementsContainer = GetNode<GridContainer>("MarginContainer/VBoxContainer/MarginContainer/AchievementsContainer/GridContainer");
		foreach (var achievement in Achievements.AllTheAchievements)
		{
			var achievementNode = (Control)ResourceLoader.Load<PackedScene>("res://Content/Scenes/Achievements/" + achievement.Key + ".tscn").Instantiate();
			achievementNode.GetNode<Timer>("PopupVersionPart/PopupTimer").Disconnect("tree_exiting", new Callable(achievementNode, "TimerDeleted"));
			achievementNode.GetNode<Node2D>("PopupVersionPart").QueueFree();
            if (!achievement.Value.IsReceived)
			{
                achievementNode.GetNode<Label>("Name").QueueFree();
                achievementNode.GetNode<Node2D>("Sprite2D/Ratings").QueueFree();
                achievementNode.GetNode<Sprite2D>("Sprite2D/RewardBox1/RewardSprite").Modulate = new Color(0, 0, 0, 0.5f);
                achievementNode.GetNode<Sprite2D>("Sprite2D/RewardBox2/RewardSprite").Modulate = new Color(0, 0, 0, 0.5f);
                achievementNode.Modulate = new Color(0.5f, 0.5f, 0.5f);
				if (achievement.Value.IsHidden)
                    achievementNode.GetNode<RichTextLabel>("Text").Text = "[HIDDEN]";
            }
			achievementsContainer.AddChild(achievementNode);
		}
	}
}
