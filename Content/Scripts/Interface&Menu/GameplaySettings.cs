using Godot;
using System;

public class GameplaySettings : Control
{
    public override void _Ready()
    {
        var dificultyChange = GetNode<OptionButton>("VBoxContainer/DificultyChange");
        dificultyChange.AddItem("Easy");
        dificultyChange.AddItem("Default");
        dificultyChange.AddItem("Hard");
        dificultyChange.Selected = Meta.Instance._dificulty;
        if (G._scores != 0)
            dificultyChange.Disabled = true;
    }
    public void DificultyChanged(int index)
    {
        Meta.Instance._dificulty = (byte)index;
    }
}
