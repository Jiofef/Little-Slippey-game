using Godot;
using System;

public partial class Level5MusicStarter : Area2D
{
    [Signal] public delegate void ButtonPressedEventHandler();

    public void AreaEntered()
    {
        if (Meta.Instance.Gameplay.AdditionStatuses[1])
            EmitSignal("ButtonPressed");
    }
}
