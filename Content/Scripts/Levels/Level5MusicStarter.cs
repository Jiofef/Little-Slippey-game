using Godot;
using System;

public partial class Level5MusicStarter : Area2D
{
    [Signal] public delegate void ButtonPressedEventHandler();

    new public void AreaEntered()
    {
        if (ContentManager.IsAdditionActive(AdditionEnum.OldFilm))
            EmitSignal("ButtonPressed");
    }
}
