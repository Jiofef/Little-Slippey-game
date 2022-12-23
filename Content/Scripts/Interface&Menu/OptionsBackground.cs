using Godot;
using System;

public class OptionsBackground : Sprite
{
    PackedScene[] defaultCross = new PackedScene[2];
    Random random = new Random();
    public override void _Ready()
    {
        for (int i = 0; i < defaultCross.Length; i++)
            defaultCross[i] = ResourceLoader.Load<PackedScene>("res://Content/Scenes/Crosses/Cross" + (i + 1) + ".tscn");
    }
    public override void _PhysicsProcess(float delta)
    {
        int RandomRange = 20 - Meta.Instance._dificulty * 5;
        if (random.Next(RandomRange) == 0)
        {
            Node2D Cross = (Node2D)defaultCross[random.Next(defaultCross.Length - 1)].Instance();
            Cross.Position = new Vector2(-750 + random.Next(1500), -450 + random.Next(900));
            AddChild(Cross);
        }
    }
}
