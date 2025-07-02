using Godot;
using System;

public partial class Level5MusicStarter : Area2D
{
    [Signal] public delegate void ButtonPressedEventHandler();

    new public void AreaEntered()
    {
        if (Additions.IsAdditionActive(Additions.AdditionEnum.OldFilm))
            EmitSignal("ButtonPressed");
    }
}
