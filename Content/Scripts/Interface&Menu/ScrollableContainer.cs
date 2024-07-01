using Godot;
using System;

public partial class ScrollableContainer : ScrollContainer
{
	[Export] Vector2 _scrollingSpeed = new Vector2 (5, 5);

	public override void _PhysicsProcess(double delta)
	{
        if (Visible)
        {
            ScrollVertical += (int)(Input.GetActionStrength("ui_scroll_down") * _scrollingSpeed.X) - (int)(Input.GetActionStrength("ui_scroll_up") * _scrollingSpeed.X);
            ScrollHorizontal += (int)(Input.GetActionStrength("ui_scroll_right") * _scrollingSpeed.Y) - (int)(Input.GetActionStrength("ui_scroll_left") * _scrollingSpeed.Y);
        }
    }
}
