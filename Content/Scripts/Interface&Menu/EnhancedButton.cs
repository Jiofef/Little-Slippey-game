using Godot;
using System;

public partial class EnhancedButton : TextureButton
{
	Vector2 _previousFrameMousePos;
	ColorRect _focusRect;
	public override void _Ready()
	{
        _focusRect = GetNode<ColorRect>("FocusRect");
        _focusRect.Size = Size;

		var downSoundCallable = new Callable(GetNode<AudioStreamPlayer>("DownSound"), "play");
        if (!IsConnected("button_down", downSoundCallable))
		Connect("button_down", downSoundCallable);

        var upSoundCallable = new Callable(GetNode<AudioStreamPlayer>("UpSound"), "play");
        if (!IsConnected("button_up", upSoundCallable))
            Connect("button_up", upSoundCallable);
    }

	public override void _PhysicsProcess(double delta)
	{
		Vector2 MousePos = GetLocalMousePosition();

        if (IsHovered() && !HasFocus() && !Disabled && _previousFrameMousePos != MousePos)
            GrabFocus();

		_previousFrameMousePos = MousePos;

        _focusRect.Modulate = new Color(
		_focusRect.Modulate.R, _focusRect.Modulate.G, _focusRect.Modulate.B, 
		Mathf.Clamp(HasFocus() ? _focusRect.Modulate.A + 0.2f : _focusRect.Modulate.A - 0.2f, 0, !Disabled ? 1 : 0.33f));
		float Brightness = ButtonPressed ? 0.5f : 1;
		Modulate = new Color(Brightness, Brightness, Brightness);
	}
}
