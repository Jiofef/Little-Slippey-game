using Godot;
using System;

public partial class OptionsBackground : Sprite2D
{
    PackedScene[] _defaultCross = new PackedScene[5];
    Random _random = new Random();
    public override void _Ready()
    {
        for (int i = 0; i < _defaultCross.Length; i++)
            _defaultCross[i] = ResourceLoader.Load<PackedScene>("res://Content/Scenes/Crosses/Cross" + (i + 1) + ".tscn");
    }
    public override void _PhysicsProcess(double delta)
    {
        int SelectedCrossNumber;
        int RandomNumber = _random.Next(200);
        for (int i = 0; ; i++)
        {
            if (RandomNumber < G.DefaultCrossWeight[i])
            {
                SelectedCrossNumber = i;
                break;
            }
            else RandomNumber -= G.DefaultCrossWeight[i];
        }

        int RandomRange = 20 - Meta.Instance.Dificulty * 5;
        if (_random.Next(RandomRange) == 0)
        {
            Node2D Cross = (Node2D)_defaultCross[SelectedCrossNumber].Instantiate();
            Cross.Position = new Vector2(-750 + _random.Next(1500), -450 + _random.Next(900));
            if (Cross.Name == "CannonCross")
            {
                Cross.Scale = new Vector2(_random.Next(100) <= 50 ? 1 : -1, 1);
                Cross.GlobalPosition = new Vector2(640 * - Cross.Scale.X, Cross.Position.Y);
            }
            AddChild(Cross);
        }
    }
}
