using Godot;
using System;

public class OptionsBackground : Sprite
{
    PackedScene[] _defaultCross = new PackedScene[2];
    Random _random = new Random();
    public override void _Ready()
    {
        for (int i = 0; i < _defaultCross.Length; i++)
            _defaultCross[i] = ResourceLoader.Load<PackedScene>("res://Content/Scenes/Crosses/Cross" + (i + 1) + ".tscn");
    }
    public override void _PhysicsProcess(float delta)
    {
        int RandomRange = 20 - Meta.Instance.Dificulty * 5;
        if (_random.Next(RandomRange) == 0)
        {
            Node2D Cross = (Node2D)_defaultCross[_random.Next(_defaultCross.Length - 1)].Instance();
            Cross.Position = new Vector2(-750 + _random.Next(1500), -450 + _random.Next(900));
            AddChild(Cross);
        }
    }
}
