using Godot;
using System;

public partial class EnhancedElementalCross : UnusualCrossNode
{
    private ShaderMaterial _noiseShader;
    private Random _random = new Random();
    private PackedScene[] _crosses = new PackedScene[2];

    public override void _Ready()
    {
        QueueFree();
        _noiseShader = (ShaderMaterial)Material;
        for (int i = 0; i < _crosses.Length; i++)
            _crosses[i] = ResourceLoader.Load<PackedScene>("res://Content/Scenes/Crosses/EnhancedCross" + (i + 1) + ".tscn");
    }
    public override void _PhysicsProcess(double delta)
	{
        Modulate = new Color(1, 1, 1, (float)_random.Next(10) / 100);
        if (_random.Next (150) == 0)
        {
            var Cross = (Node2D)_crosses[_random.Next(_crosses.Length)].Instantiate();
            Cross.GlobalPosition = GlobalPosition;
            Cross.Material = _noiseShader;
            Cross.GetNode<Area2D>("ExplosiveArea").Monitorable = false;
            GetParent().AddChild(Cross);
        }

        if (_random.Next(30) == 0)
            GlobalPosition = G.Player.GlobalPosition + new Vector2(_random.Next(-450, 450), _random.Next(-250, 250));

        if (_random.Next(500) == 0)
            OnFinished();
    }
}
