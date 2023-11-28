using Godot;

public partial class SkinsMenu : Control
{
	public override void _Ready()
	{
        int[] NeededAchievementIndexes = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12};
        for (int i = 0; i < NeededAchievementIndexes.Length; i++)
        {
            if (UnchangableMeta.AchievementStatuses[NeededAchievementIndexes[i]] != 1)
            {
                GetNode<TextureButton>("SkinButtons/Button" + (i + 2)).Disabled = true;
                GetNode<AnimatedSprite2D>("SkinButtons/Button" + (i + 2) + "/AnimatedSprite2D").QueueFree();
				GetNode<Label>("SkinButtons/Button" + (i + 2) + "/SkinName").Text = "???";
				GetNode<Sprite2D>("SkinButtons/Button" + (i + 2) + "/?").Visible = true;
            }
			else
                GetNode<Sprite2D>("SkinButtons/Button" + (i + 2) + "/?").QueueFree();
        }
        
		GetNode<TextureButton>("SkinButtons/Button" + (Meta.Instance.ChosenSkinIndex + 1)).ButtonPressed = true;
		int SkinsUnlocked = 0;
		for (int i = 0; i < UnchangableMeta.AchievementStatuses.Length; i++)
			SkinsUnlocked += UnchangableMeta.AchievementStatuses[i];
		GetNode<Label>("SkinsUnlocked").Text = "Skins: " + (SkinsUnlocked + 1) + "/" + (NeededAchievementIndexes.Length + 1);
	}
	public void SetSkin(int index)
	{
		Meta.Instance.ChosenSkinIndex = index;
		Meta.Instance.SaveToFile();
	}
}
