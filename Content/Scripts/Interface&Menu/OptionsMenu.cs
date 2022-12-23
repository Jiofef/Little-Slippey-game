using Godot;
using System;

public class OptionsMenu : Control
{
    public override void _Ready()
    {
        Meta.OptionsReserve = Meta.Instance.Clone();
    }
}
