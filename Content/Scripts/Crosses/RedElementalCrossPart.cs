using Godot;
using System;

public partial class RedElementalCrossPart : ElementalCrossPart
{
    [Signal] public delegate void ElementExplodedEventHandler();
    private Sprite2D _sprite;
    private PathFollow2D _pathFollow2D;
    private float _elementSpeed = 4.5f;
    public override void _Ready()
    {
        // Initializing nodes
        NodesInit();

        LifeTime = 2.0f;
        RandomizePathVec(new Rect2(-90, -300, 90 * 2, 240));

        Random random = new Random();
        GetNode<Path2D>("Path2D").Scale = new Vector2(random.Next(10, 100) * (random.Next(100) > 50 ? 1 : -1) / 100f, random.Next(20, 100) / 100f);
        _pathFollow2D.GlobalScale = new Vector2(1, 1);

        _sprite.Modulate = new Color(_sprite.Modulate.R, _sprite.Modulate.G, _sprite.Modulate.B, 0);
    }
    public override void _PhysicsProcess(double delta)
    {
        if (_pathFollow2D.ProgressRatio < 0.98f)
        {
            _pathFollow2D.Progress += _elementSpeed;
            _elementSpeed -= 0.03f;
            _sprite.Modulate = new Color(_sprite.Modulate.R, _sprite.Modulate.G, _sprite.Modulate.B, _sprite.Modulate.A + 0.1f);
        }
        else
        {
            // Explode();
        }
    }
}
