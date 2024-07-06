using Godot;
using System;

public partial class TutorialTvScreen : Control
{
    [Export] bool _isThereASecondSpriteForGamepad = true;
    public void UpdateScreen()
    {
        GetNode<RichTextLabel>("Text").Visible = G.TypeOfUsedController == "Keyboard";
        if (_isThereASecondSpriteForGamepad)
        {
            GetNode<RichTextLabel>("XBoxText").Visible = G.TypeOfUsedController == "XInput Gamepad";
            GetNode<RichTextLabel>("PSText").Visible = G.TypeOfUsedController == "PS Gamepad";
        }
        else
            GetNode<RichTextLabel>("GamepadText").Visible = G.TypeOfUsedController == "XInput Gamepad" || G.TypeOfUsedController == "PS Gamepad";
        if (G.TypeOfUsedController != "Keyboard" && G.TypeOfUsedController != "XInput Gamepad" && G.TypeOfUsedController != "PS Gamepad")
            GetNode<RichTextLabel>(_isThereASecondSpriteForGamepad ? "XBoxText" : "GamepadText").Visible = true;
    }
}
