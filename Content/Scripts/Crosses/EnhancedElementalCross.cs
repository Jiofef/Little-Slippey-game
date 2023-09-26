using Godot;
using System;

public partial class EnhancedElementalCross : Node2D
{
    private ShaderMaterial _noiseShader;
    private Random _random = new Random();
    private PackedScene[] _crosses = new PackedScene[2];
    private CharacterBody2D _player;

    public override void _Ready()
    {
        _noiseShader = (ShaderMaterial)Material;
        _player = GetNode<CharacterBody2D>("../Player");
        for (int i = 0; i < _crosses.Length; i++)
            _crosses[i] = ResourceLoader.Load<PackedScene>("res://Content/Scenes/Crosses/EnhancedCross" + (i + 1) + ".tscn");
    }
    public override void _Process(double delta)
	{
        Modulate = new Color(1, 1, 1, (float)_random.Next(10) / 100);
        if (_random.Next (150) == 0)
        {
            var Cross = (Node2D)_crosses[_random.Next(_crosses.Length)].Instantiate();
            Cross.GlobalPosition = GlobalPosition;
            Cross.Material = _noiseShader;
            GetParent().AddChild(Cross);
        }

        if (_random.Next(30) == 0)
            GlobalPosition = _player.GlobalPosition + new Vector2(_random.Next(-450, 450), _random.Next(-250, 250));

        if (_random.Next(500) == 0)
            QueueFree();
    }
}
