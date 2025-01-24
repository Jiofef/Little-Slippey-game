using Godot;
using System;

[Tool]
public partial class SkinsMenu : DraggableWindow
{
    private int _boughtSkins = 0;

	public override void _Ready()
	{
        _boughtSkins = Skins.GetBoughtSkinsCount();

        var skinButtonsContainer = GetNode<FlowContainer>("MarginContainer/VBoxContainer/ScrollContainer/SkinButtons");
        int buttonIndex = Array.IndexOf(Skins.VanillaSkinNames, Meta.Instance.Gameplay.ChosenSkinKey) + 1;

        skinButtonsContainer.GetNode<SkinButton>("Button" + buttonIndex).ButtonPressed = true;
        WindowTitle = Tr("Skins") + " (" + (_boughtSkins + 1) + " " + Tr("of") + " " + Skins.GetSkinsCount() + ")";
    }
}
