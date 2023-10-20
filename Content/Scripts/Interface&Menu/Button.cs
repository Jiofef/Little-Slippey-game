using Godot;

public partial class Button : TextureButton
{
	[Export] Rect2 PossibleGrabFocusRect = new Rect2 (new Vector2(0, 0), new Vector2(1280, 720));

	enum ButtonMode {Modern, Old, Default}

	[Export] ButtonMode _mode = ButtonMode.Default;


	Vector2 _previousFrameMousePos;
	ColorRect _focusRect;
	public override void _Ready()
	{
		if (_mode != ButtonMode.Default) 
		{
            _focusRect = GetNode<ColorRect>("FocusRect");
            _focusRect.Size = Size;

			if (_mode == ButtonMode.Old) 
				_focusRect.ShowBehindParent = false;
        }
		else
            GetNode<ColorRect>("FocusRect").QueueFree();

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

		if (_mode != ButtonMode.Default)
		{
            _focusRect.Modulate = new Color(
				_focusRect.Modulate.R, _focusRect.Modulate.G, _focusRect.Modulate.B, 
				Mathf.Clamp(HasFocus() ? _focusRect.Modulate.A + 0.2f : _focusRect.Modulate.A - 0.2f, 0, 1));
            if (_mode == ButtonMode.Old)
            {
				float Brightness = ButtonPressed ? 0.5f : 1;
				Modulate = new Color(Brightness, Brightness, Brightness);
            }
        }

	}
}
