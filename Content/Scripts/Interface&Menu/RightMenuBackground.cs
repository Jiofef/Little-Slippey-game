using Godot;
using System;

public partial class RightMenuBackground : Node2D
{
    public override void _Ready()
    {
        Random random = new Random();
        int RandomLevel = random.Next(G.LevelsInGameTotal) + 1;
        var PresentedLevel = (Node2D)ResourceLoader.Load<PackedScene>("res://Content/Scenes/Levels/PresentedParts/PresentedLevel" + (RandomLevel) + ".tscn").Instantiate();
        PresentedLevel.Scale = new Vector2(900 / G.LevelXYSizes[RandomLevel].X, 720 / G.LevelXYSizes[RandomLevel].Y); //900 is the optimal X length, to make the level fit into the menu. 720 is the default Y height of levels
        AddChild(PresentedLevel);
    }
}
