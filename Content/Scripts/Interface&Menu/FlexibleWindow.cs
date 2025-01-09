using Godot;
using System;

public partial class FlexibleWindow : Control
{
    [Export] public bool CanChangeSize = true;

    private bool _isChangingSize = false;
    private string _changingBorderName;
    private Rect2 _initialRect;
    protected readonly Rect2I _windowSize = new Rect2I(0, 0, 1280, 720);
    public override void _Process(double delta)
    {
        if (_isChangingSize)
        {
            Vector2 MousePos = GetLocalMousePosition(),
                GlobalMouseMous = GetGlobalMousePosition(),
                NewSize = Size,
                NewPos = Position;

            if (_changingBorderName.Contains("Right"))
            {
                NewSize.X = MousePos.X;
            }

            if (_changingBorderName.Contains("Down"))
            {
                NewSize.Y = MousePos.Y;
            }
            if (_changingBorderName.Contains("Left"))
            {
                if (Size.X - MousePos.X >= CustomMinimumSize.X && GlobalMouseMous.X > 0)
                {
                    NewPos.X += MousePos.X;
                    NewSize.X -= MousePos.X;
                }
            }
            if (_changingBorderName.Contains("Up"))
            {
                if (Size.Y - MousePos.Y >= CustomMinimumSize.Y && GlobalMouseMous.Y > 0)
                {
                    NewPos.Y += MousePos.Y;
                    NewSize.Y -= MousePos.Y;
                }
            }
            NewSize.X = Mathf.Clamp(NewSize.X, 0, 1280 - Position.X);
            NewSize.Y = Mathf.Clamp(NewSize.Y, 0, 720 - Position.Y);
            NewPos.X = Mathf.Clamp(NewPos.X, 0, 1280);
            NewPos.Y = Mathf.Clamp(NewPos.Y, 0, 720);

            Size = NewSize;
            Position = NewPos;
        }
    }

    public void BorderGuiInput(InputEvent @event, string borderName)
    {
        if (@event.IsActionPressed("MouseLeftClick"))
        {
            _isChangingSize = true;
            _changingBorderName = borderName;
            _initialRect = GetGlobalRect();
        }
        if (@event.IsActionReleased("MouseLeftClick"))
        {
            _isChangingSize = false;
            _changingBorderName = null;
            _initialRect = new Rect2();
        }
    }
}
