using Godot;

public partial class Achievement : Control
{
    private ColorRect _focusRect;
    public override void _Ready()
    {
        _focusRect = GetNode<ColorRect>("FocusRect");
    }

    public override void _PhysicsProcess(double delta)
    {
        _focusRect.Modulate = new Color(
        _focusRect.Modulate.R, _focusRect.Modulate.G, _focusRect.Modulate.B,
        Mathf.Clamp(HasFocus() ? _focusRect.Modulate.A + 0.2f : _focusRect.Modulate.A - 0.2f, 0, 1));
    }

    public void TimerDeleted()
    {
        G.AchievementPopupTimerMultiplier--;
    }
}
