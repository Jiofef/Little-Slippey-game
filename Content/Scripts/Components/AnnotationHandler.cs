using Godot;
using System;

public partial class AnnotationHandler : Node
{
    [Export] public bool AnnotationEnabled = false;
    [Export(PropertyHint.MultilineText)] public string AnnotationDefaultText;
    [Export] public Vector2 AnnotationBoxSizeOverride = Vector2.Zero;
    [Export] public float AnnotationAppearingDelay = 0.5f;

    private Control _parentControl;
    private float _mouseTimer;
    private float _focusTimer;
    private bool _isMouseOver;
    private bool _hasFocus;
    private bool _isAnnotationVisible = false;

    public override void _Ready()
    {
        _parentControl = GetParentOrNull<Control>();
        if (_parentControl == null)
        {
            GD.PrintErr($"AnnotationHandler must be a child of a Control node. Check {GetParent().Name}");
            return;
        }

        _parentControl.MouseEntered += OnMouseEntered;
        _parentControl.MouseExited += OnMouseExited;
        _parentControl.FocusEntered += OnFocusEntered;
        _parentControl.FocusExited += OnFocusExited;
    }

    public override void _Process(double delta)
    {
        if (!AnnotationEnabled)
            return;

        if (_isMouseOver && !_isAnnotationVisible && _mouseTimer > 0)
        {
            _mouseTimer -= (float)delta;
            if (_mouseTimer <= 0 && _isMouseOver)
            {
                ShowAnnotationFollowingMouse();
            }
        }

        if (_hasFocus && !_isAnnotationVisible && _focusTimer > 0)
        {
            _focusTimer -= (float)delta;
            if (_focusTimer <= 0 && _hasFocus && !_isMouseOver)
            {
                ShowAnnotationPinned();
            }
        }


        if (_isAnnotationVisible && _isMouseOver && _parentControl.HasFocus())
        {
            G.AdditionalGuiLayer.AnnotationBox.UnPin();
        }
    }

    private void OnMouseEntered()
    {
        _isMouseOver = true;
        if (!_isAnnotationVisible)
        {
            _mouseTimer = AnnotationAppearingDelay;
        }
    }

    private void OnMouseExited()
    {
        _isMouseOver = false;
        if (_isAnnotationVisible)
        {
            HideAnnotation();
        }
    }

	private void OnFocusEntered()
	{
		_hasFocus = true;
		if (!_isMouseOver)
		{
			if (!_isAnnotationVisible)
			{
				_focusTimer = AnnotationAppearingDelay;
			}
		}
		else
			OnMouseEntered();
    }

    private void OnFocusExited()
    {
        _hasFocus = false;
        if (_isAnnotationVisible)
        {
            HideAnnotation();
        }
    }

    private void ShowAnnotationFollowingMouse()
    {
        if (_isAnnotationVisible) return;
        G.AdditionalGuiLayer.AnnotationBox.PopupWithText(AnnotationDefaultText, AnnotationBoxSizeOverride != Vector2.Zero ? AnnotationBoxSizeOverride : null);
        _isAnnotationVisible = true;
    }

    private void ShowAnnotationPinned()
    {
        if (_isAnnotationVisible) return;
        Vector2 centerPos = _parentControl.GlobalPosition + (_parentControl.Size * _parentControl.Scale) / 2;
        G.AdditionalGuiLayer.AnnotationBox.PopupWithText(AnnotationDefaultText, AnnotationBoxSizeOverride != Vector2.Zero ? AnnotationBoxSizeOverride : null);
        G.AdditionalGuiLayer.AnnotationBox.PinTo(centerPos);
        _isAnnotationVisible = true;
    }

    private void HideAnnotation()
    {
        G.AdditionalGuiLayer.AnnotationBox.Hide();
        _isAnnotationVisible = false;
    }
}