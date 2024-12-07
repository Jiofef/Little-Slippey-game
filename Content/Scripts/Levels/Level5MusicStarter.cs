using Godot;
using System;

public partial class Level5MusicStarter : Area2D
{
    [Signal] public delegate void ButtonPressedEventHandler();

    new public void AreaEntered()
    {
        if (Meta.Instance.Gameplay.AdditionStatuses[1])
            EmitSignal("ButtonPressed");
    }
}
