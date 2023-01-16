using Godot;
using System;

public class RightMenuBackground : Node2D
{
    public override void _Ready()
    {
        Random random = new Random();
        int RandomLevel = random.Next(G.LevelsInGameTotal);
        var PresentedLevel = (Node2D)ResourceLoader.Load<PackedScene>("res://Content/Scenes/Levels/PresentedParts/PresentedLevel" + (RandomLevel + 1) + ".tscn").Instance();
        PresentedLevel.Scale = new Vector2(840 / G.LevelXYSizes[RandomLevel].x, 600 / G.LevelXYSizes[RandomLevel].y); //840 is the optimal X length, to make the level fit into the menu. 600 is the default Y height of levels
        AddChild(PresentedLevel);
    }
}
