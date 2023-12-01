using Godot;

public partial class SkinsMenu : Control
{
	public override void _Ready()
	{
        int[] NeededAchievementIndexes = { 3, 12, 15, 18, 21, 24, 27, 30, 35, 40, 43, 49, 50};
        int SkinsUnlocked = 0;
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
			{
                GetNode<Sprite2D>("SkinButtons/Button" + (i + 2) + "/?").QueueFree();
				SkinsUnlocked++;
            }
        }
        
		GetNode<TextureButton>("SkinButtons/Button" + (Meta.Instance.ChosenSkinIndex + 1)).ButtonPressed = true;
		GetNode<Label>("SkinsUnlocked").Text = "Skins: " + (SkinsUnlocked + 1) + "/" + (NeededAchievementIndexes.Length + 1);
	}
	public void SetSkin(int index)
	{
		Meta.Instance.ChosenSkinIndex = index;
		Meta.Instance.SaveToFile();
	}
}
