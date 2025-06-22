using Godot;
using System;

public partial class ScrollableRichTextLabel : RichTextLabel
{
	[Export] float _scrollingSpeed = 10;
    private float _lineScrollTImer = 1f;
    private int _focusedLine = 0;
    public override void _PhysicsProcess(double delta)
    {
        if (Visible)
        {
            int NextLine = _focusedLine + Math.Sign(Input.GetActionStrength("ui_scroll_down") - Input.GetActionStrength("ui_scroll_up"));
            if (NextLine != _focusedLine) 
                _lineScrollTImer -= _scrollingSpeed / 60;
            if (_lineScrollTImer < 0 && NextLine >= 0 && NextLine <= GetLineCount() - GetVisibleLineCount() + 2)
            {
                ScrollToLine(NextLine);
                _lineScrollTImer = 1;
                _focusedLine = NextLine;
            }
        }
    }
}
