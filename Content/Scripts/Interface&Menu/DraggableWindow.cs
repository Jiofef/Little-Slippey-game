using Godot;
using System;
using System.Linq;

[Tool]
public partial class DraggableWindow : FlexibleWindow
{
    [Signal] public delegate void WindowClosedByButtonEventHandler();
    private string _windowTitle = "Title";
    [Export] public string WindowTitle
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
    private bool _canClose = true;
    [Export]
    public bool CanClose
    {
        get => _canClose;
        set
        {
            _canClose = value;
            CallDeferred("UpdateCloseButton");
        }
    }
    private void UpdateCloseButton()
    {
        GetNode<TextureButton>("MarginContainer/VBoxContainer/BlueBox/CloseButton").Visible = _canClose;
        
    }
    private void OnCloseButtonPressed()
    {
        EmitSignal("WindowClosedByButton");
        QueueFree();
    }

    private bool _isBlueBoxHovered = false, _isDragging = false;
    public Vector2 Velocity;
    private Vector2 _savedMousePos;

    public void OnBlueBoxInput(InputEvent @event)
    {
        if (@event.IsActionPressed("MouseLeftClick"))
        {
            _isDragging = true;
            _savedMousePos = GetLocalMousePosition();
        }
        if (@event is InputEventMouseMotion)
            _mouseVelocity = ((InputEventMouseMotion)@event).ScreenRelative;
    }

    private Vector2 _mouseVelocity;

    public override void _Process(double delta)
    {
        base._Process(delta);

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
                Velocity = Vector2.Zero;
                Rotation = 0;
            }

        }

        if (Velocity == Vector2.Zero && Rotation == 0)
            return;

        var blueBox = GetNode<NinePatchRect>("MarginContainer/VBoxContainer/BlueBox");

        Vector2 blueBoxPos = blueBox.GlobalPosition - GlobalPosition;
        //GD.Print(blueBoxPos);
        //GD.Print(1280 - blueBox.Size.X - blueBoxPos.X);
        GlobalPosition = new Vector2(Math.Clamp(GlobalPosition.X, -Size.X + 80, 1280 - 80 - blueBoxPos.X), Math.Clamp(GlobalPosition.Y + blueBox.Position.Y, -blueBoxPos.Y, 720 - blueBox.Size.Y - blueBoxPos.Y));
        Velocity /= 1.25f;
        Rotation /= 1.1f;

        _mouseVelocity = Vector2.Zero;
    }
}
