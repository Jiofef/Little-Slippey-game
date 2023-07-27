using Godot;
using System;

public partial class Button : TextureButton
{
	ColorRect _focusRect;
	public override void _Ready()
	{
		_focusRect = GetNode<ColorRect>("FocusRect");
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 MousePos = GetLocalMousePosition();
		if (!HasFocus() && MousePos.X > 0 && MousePos.X < 96 && MousePos.Y > 0 && MousePos.Y < 15)
			GrabFocus();

		float NewFocusRectModulate = _focusRect.Modulate.A;
		if (HasFocus() && _focusRect.Modulate.A < 1)
			NewFocusRectModulate += 0.2f;
		else if (!HasFocus() && _focusRect.Modulate.A > 0)
			NewFocusRectModulate -= 0.2f;
        _focusRect.Modulate = new Color(_focusRect.Modulate.R, _focusRect.Modulate.G, _focusRect.Modulate.B, NewFocusRectModulate);
	}
}
