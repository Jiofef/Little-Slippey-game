using Godot;
using System;

public partial class TutorialTvScreen : Control
{
    [Export] bool _isThereASecondSpriteForGamepad = true;
    public void UpdateScreen()
    {
        GD.Print("s");
        switch (G.TypeOfUsedController)
        {
            case "Keyboard":
                GetNode<Label>("Text").Visible = true;
                GetNode<Label>("GamepadText").Visible = false;
                break;
            case "PS Gamepad":
                GetNode<Label>("Text").Visible = false;
                GetNode<Label>("GamepadText").Visible = true;
                if (_isThereASecondSpriteForGamepad)
                {
                    GetNode<Sprite2D>("GamepadText/XBoxButton").Visible = false;
                    GetNode<Sprite2D>("GamepadText/PSButton").Visible = true;
                }
                break;
            case "XInput Gamepad":
                GetNode<Label>("Text").Visible = false;
                GetNode<Label>("GamepadText").Visible = true;
                if (_isThereASecondSpriteForGamepad)
                {
                    GetNode<Sprite2D>("GamepadText/XBoxButton").Visible = true;
                    GetNode<Sprite2D>("GamepadText/PSButton").Visible = false;
                }
                break;
        }
    }
}
