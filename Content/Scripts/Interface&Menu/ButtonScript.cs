using Godot;
using System;

public class ButtonScript : TextureButton
{
    Texture _texturefocused;
    AudioStreamPlayer buttondown;
    AudioStreamPlayer buttonup;
    public override void _Ready()
    {
        _texturefocused = TextureFocused;
        buttondown = GetNode<AudioStreamPlayer>("ButtonDownSound");
        buttonup = GetNode<AudioStreamPlayer>("ButtonUpSound");
    }
    public void MouseEntered()
    {
        GrabFocus();
    }
    public void ButtonDown()
    {
        buttondown.Play();
        TextureFocused = null;
        Pressed = true;
    }
    public void ButtonUp()
    {
        buttonup.Play();
        TextureFocused = _texturefocused;
    }
}
