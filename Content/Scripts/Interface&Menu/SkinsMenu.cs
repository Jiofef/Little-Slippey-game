using Godot;

[Tool]
public partial class SkinsMenu : DraggableWindow
{
	public override void _Ready()
	{
        int[] NeededAchievementIndexes = { 3, 12, 15, 18, 21, 24, 27, 30, 35, 40, 43, 50, 51};
        int SkinsUnlocked = 0;

        string link = "MarginContainer/VBoxContainer/ScrollContainer/SkinButtons";

        var allTheSkinButtons = GetNode(link).GetChildren();
        {

            int i = 0;
            foreach (EnhancedButton button in allTheSkinButtons)
            {
                int i1 = i;
                button.Pressed += () => SetSkin(i1);
                i++;
            }
		}



        for (int i = 0; i < NeededAchievementIndexes.Length; i++)
        {
            if (UnchangableMeta.AchievementStatuses[NeededAchievementIndexes[i]] != 1)
            {
                GetNode<TextureButton>(link + "/Button" + (i + 2)).Disabled = true;
                GetNode<AnimatedSprite2D>(link + "/Button" + (i + 2) + "/AnimatedSprite2D").QueueFree();
				GetNode<Label>(link + "/Button" + (i + 2) + "/SkinName").Text = "???";
				GetNode<Sprite2D>(link + "/Button" + (i + 2) + "/?").Visible = true;
            }
			else
			{
                GetNode<Sprite2D>(link + "/Button" + (i + 2) + "/?").QueueFree();
				SkinsUnlocked++;
            }
        }
        
		GetNode<TextureButton>(link + "/Button" + (Meta.Instance.Gameplay.ChosenSkinIndex + 1)).ButtonPressed = true;
        WindowTitle = Tr("Skins") + " (" + (SkinsUnlocked + 1) + " " + Tr("of") + " " + allTheSkinButtons.Count + ")";

    }
	public void SetSkin(int index)
	{
		Meta.Instance.Gameplay.ChosenSkinIndex = index;
		Meta.Instance.SaveToFile();
	}
}
