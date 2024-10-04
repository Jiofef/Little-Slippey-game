using Godot;
using System;

public partial class LevelEditorDefaultButton : Area2D
{
    [Signal] public delegate void ButtonPressedEventHandler();
    [Signal] public delegate void ButtonUnpressedEventHandler();
    [Export] public bool _releaseTimerEnabled = false;
    [Export] public float _releaseTimer = 5;
    CollisionShape2D _collision;

    public override void _Ready()
    {
        _collision = GetNode<CollisionShape2D>("CollisionShape2D");
        AreaEntered += (junk) => Press();
    }
    public void Press()
    {
        if (Monitoring)
        {
            EmitSignal("ButtonPressed");
            SetDeferred("monitoring", false);
            GetNode<Sprite2D>("Sprite2D").RegionRect = new Rect2(16, 0, 16, 8);
            if (_releaseTimerEnabled)
                GetNode<Timer>("ReleaseTimer").Start(_releaseTimer);
        }
    }
    public void UnPress()
    {
        if (!Monitoring)
        {
            EmitSignal("ButtonUnpressed");
            SetDeferred("monitoring", true);
            GetNode<Sprite2D>("Sprite2D").RegionRect = new Rect2(0, 0, 16, 8);
        }
    }
}
