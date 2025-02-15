using Godot;
using System;

[Tool]
public partial class SkinsMenu : DraggableWindow
{
    private int _boughtSkins = 0;
    private SkinButton _selectedButton;

	public override void _Ready()
	{
        _boughtSkins = Skins.GetBoughtSkinsCount();

        var skinButtonsContainer = GetNode<FlowContainer>("MarginContainer/VBoxContainer/ScrollContainer/SkinButtons");
        int buttonIndex = Array.IndexOf(Skins.VanillaSkinNames, Meta.Instance.Gameplay.ChosenSkinKey) + 1;

        _selectedButton = skinButtonsContainer.GetNode<SkinButton>("Button" + buttonIndex);
        _selectedButton.ButtonPressed = true;

        WindowTitle = Tr("Skins") + " (" + (_boughtSkins + 1) + " " + Tr("of") + " " + Skins.GetSkinsCount() + ")";

        foreach (var child in skinButtonsContainer.GetChildren())
        {
            if (child is SkinButton sB)
            {
                sB.ButtonDown += () => OnSkinButtonDown(sB);
            }
        }

        // I did it to keep the bug I liked :P
        UnchangableMeta.SetAutoSaveWhenClosing(false);
        TreeExited += () => UnchangableMeta.SetAutoSaveWhenClosing(true);
    }

    private async void OnSkinButtonDown(SkinButton button)
    {
        if (!button.IsSkinBought())
        {
            button.ButtonPressed = false;

            // This prevents the situation when buying a new skin, the previous skin is deselected and the new one is not selected. Now the selection remains on the previous skin
            await ToSignal(button, "pressed");
            _selectedButton.ButtonPressed = true;
        }
        else
        {
            _selectedButton = button;
        }
    }
}
