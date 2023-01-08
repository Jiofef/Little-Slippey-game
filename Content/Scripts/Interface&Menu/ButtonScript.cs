using Godot;
using System;

public class ButtonScript : TextureButton
{
    Texture _textureFocused;
    AudioStreamPlayer _buttonDown;
    AudioStreamPlayer _buttonUp;
    public override void _Ready()
    {
        _textureFocused = TextureFocused;
        _buttonDown = GetNode<AudioStreamPlayer>("ButtonDownSound");
        _buttonUp = GetNode<AudioStreamPlayer>("ButtonUpSound");
    }
    public void MouseEntered()
    {
        GrabFocus();
    }
    public void ButtonDown()
    {
        _buttonDown.Play();
        TextureFocused = null;
        Pressed = true;
    }
    public void ButtonUp()
    {
        _buttonUp.Play();
        TextureFocused = _textureFocused;
    }
}
