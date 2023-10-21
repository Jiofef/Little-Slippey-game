using Godot;

public partial class Button : TextureButton
{
	[Export] Rect2 PossibleGrabFocusRect = new Rect2 (new Vector2(0, 0), new Vector2(1280, 720));

	Vector2 _previousFrameMousePos;
	ColorRect _focusRect;
	public override void _Ready()
	{
        _focusRect = GetNode<ColorRect>("FocusRect");
        _focusRect.Size = Size;
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 MousePos = GetLocalMousePosition();
		Vector2 MouseGlobalPos = GetGlobalMousePosition();
		if (!HasFocus() && !Disabled && _previousFrameMousePos != MousePos &&
            MouseGlobalPos >= PossibleGrabFocusRect.Position && MouseGlobalPos <= PossibleGrabFocusRect.Position + PossibleGrabFocusRect.Size &&
			MousePos.X > 0 && MousePos.X < Size.X && MousePos.Y > 0 && MousePos.Y < Size.Y)
			GrabFocus();
		_previousFrameMousePos = MousePos;

        _focusRect.Modulate = new Color(
		_focusRect.Modulate.R, _focusRect.Modulate.G, _focusRect.Modulate.B, 
		Mathf.Clamp(HasFocus() ? _focusRect.Modulate.A + 0.2f : _focusRect.Modulate.A - 0.2f, 0, !Disabled ? 1 : 0.33f));
		float Brightness = ButtonPressed ? 0.5f : 1;
		Modulate = new Color(Brightness, Brightness, Brightness);
	}
}
