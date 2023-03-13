using Godot;
using System;

public partial class ButtonScript : TextureButton
{
    Texture2D _textureFocused;
    AudioStreamPlayer _buttonDown;
    AudioStreamPlayer _buttonUp;
    public override void _Ready()
    {
        _textureFocused = TextureFocused;
        _buttonDown = GetNode<AudioStreamPlayer>("ButtonDownSound");
        _buttonUp = GetNode<AudioStreamPlayer>("ButtonUpSound");
    }
    public void ThisMouseEntered()
    {
        GrabFocus();
    }
    public void ThisButtonDown()
    {
        _buttonDown.Play();
        TextureFocused = null;
        ButtonPressed = true;
    }
    public void ThisButtonUp()
    {
        _buttonUp.Play();
        TextureFocused = _textureFocused;
    }
}
