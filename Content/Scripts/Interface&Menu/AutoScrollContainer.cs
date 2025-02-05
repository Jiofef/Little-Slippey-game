using Godot;
using System;
using System.Threading.Tasks;

public partial class AutoScrollContainer : ScrollContainer
{
    [Export] public float ScrollSpeed = 60f;
    [Export] public float PauseDuration = 1f;
    [Export] public bool EnableCycleScrolling = true;
    [Export] public bool AutoStart = false;

    private Vector2 _maxSize;
    private Vector2 _minSize;
    private Vector2 _scrollMaxValues;

    private HScrollBar _hScrollBar;
    private VScrollBar _vScrollBar;

    private int _timer => (int)(PauseDuration * 1000);

    public override async void _Ready()
    {
        await ToSignal(GetTree(), "process_frame");

        _hScrollBar = GetHScrollBar();
        _vScrollBar = GetVScrollBar();

        _maxSize.X = (float)_hScrollBar.MaxValue;
        _maxSize.Y = (float)_vScrollBar.MaxValue;

        _minSize = Size;

        _scrollMaxValues = _maxSize - _minSize;

        if (AutoStart)
        {
            ScrollRight();
            ScrollDown();
        }
    }

    public void ScrollRight()
    {
        if (_maxSize.X <= _minSize.X)
            return;

        float where = _maxSize.X - _minSize.X;
        float duration = (where - ScrollHorizontal) / ScrollSpeed;

        Scroll(where, duration, _directionKey.horizontal, RightScrollFinished);
    }

    public void ScrollLeft()
    {
        if (_maxSize.X <= _minSize.X)
            return;

        float where = 0;
        float duration = (ScrollHorizontal) / ScrollSpeed;

        Scroll(where, duration, _directionKey.horizontal, LeftScrollFinished);
    }

    public void ScrollDown()
    {
        if (_maxSize.Y <= _minSize.Y)
            return;

        float where = _maxSize.Y - _minSize.Y;
        float duration = (where - ScrollVertical) / ScrollSpeed;

        Scroll(where, duration, _directionKey.vertical, DownScrollFinished);
    }
    public void ScrollUp()
    {
        if (_maxSize.Y <= _minSize.Y)
            return;

        float where = 0;
        float duration = (ScrollVertical) / ScrollSpeed;

        Scroll(where, duration, _directionKey.vertical, UpScrollFinished);
    }

    private enum _directionKey {vertical, horizontal};

    private void Scroll(float where, float duration, _directionKey directionKey, Action callback)
    {
        Tween tweenDown = GetTree().CreateTween();
        tweenDown.TweenProperty(this, "scroll_" + directionKey.ToString(), where, duration);

        if (EnableCycleScrolling)
            tweenDown.TweenCallback(Callable.From(callback));
    }

    private async void RightScrollFinished()
    {
        await Task.Delay(_timer);
        ScrollLeft();
    }

    private async void LeftScrollFinished()
    {
        await Task.Delay(_timer);
        ScrollRight();
    }

    private async void DownScrollFinished()
    {
        await Task.Delay(_timer);
        ScrollUp();
    }

    private async void UpScrollFinished()
    {
        await Task.Delay(_timer);
        ScrollDown();
    }
}
