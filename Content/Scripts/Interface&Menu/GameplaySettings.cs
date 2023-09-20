using Godot;
using System;

public partial class GameplaySettings : Control
{
    public override void _Ready()
    {
        var dificultyChange = GetNode<OptionButton>("VBoxContainer/DificultyChange");
        dificultyChange.Selected = Meta.Instance.Dificulty;
        if (G.CurrentLevel != 0)
            dificultyChange.Disabled = true;
    }
    public void DificultyChanged(int index)
    {
        Meta.Instance.Dificulty = index;
    }
}
