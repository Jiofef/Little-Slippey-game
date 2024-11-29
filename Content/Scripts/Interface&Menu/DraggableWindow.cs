using Godot;
using System;
using System.Linq;

[Tool]
public partial class DraggableWindow : FlexibleWindow
{
    private string _windowTitle = "Title";
    [Export] public string WindowTitles
    {
        get => _windowTitle;
        set 
        {
            _windowTitle = value;
            CallDeferred("UpdateWindowTitle");
        }

    }
    private void UpdateWindowTitle()
    {
        GetNode<Label>("MarginContainer/VBoxContainer/BlueBox/WindowTitle").Text = _windowTitle;
    }


    private bool _isBlueBoxHovered = false, _isDragging = false;
    public Vector2 Velocity;
    private Vector2 _savedMousePos;

    public void OnBlueBoxInput(InputEvent @event)
    {
        if (@event.IsActionPressed("MouseLeftClick"))
        {
            _isDragging = true;
            SetProcess(true);
            _savedMousePos = GetLocalMousePosition();
        }
        if (@event is InputEventMouseMotion)
            _mouseVelocity = ((InputEventMouseMotion)@event).ScreenRelative;
    }

    private Vector2 _mouseVelocity;

    public override void _Process(double delta)
    {
        if (_isDragging)
        {
            Vector2 LocalMousePos = GetLocalMousePosition();
            Velocity = _mouseVelocity;
            GlobalPosition = GetGlobalMousePosition() - _savedMousePos;
            Rotation += _mouseVelocity.X / 8 / Size.X;
            Rotation = Mathf.Clamp(Rotation, -1.5f, 1.5f);
            if (Input.IsActionJustReleased("MouseLeftClick"))
            {
                _isDragging = false;
            }
        }
        else
        {
            GlobalPosition += Velocity;

            if (Velocity.LengthSquared() < 1 && -0.05f < RotationDegrees && RotationDegrees < 0.05f)
            {
                SetProcess(false);
                Velocity = Vector2.Zero;
                Rotation = 0;
            }

        }
        Position = new Vector2(Math.Clamp(Position.X, 0, 1280 - Size.X), Math.Clamp(Position.Y, 0, 720 - Size.Y));
        Velocity /= 1.25f;
        Rotation /= 1.1f;

        _mouseVelocity = Vector2.Zero;
    }
}
