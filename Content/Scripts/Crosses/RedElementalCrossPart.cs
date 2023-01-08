using Godot;
using System;

public class RedElementalCrossPart : Node2D
{
    [Signal] public delegate void ElementExploded();
    private Sprite _sprite;
    private PathFollow2D _pathFollow2D;
    private const float _elementSlowdown = 0.01f, _limitOffsetForElement = 100;
    private float _elementSpeed = 1.5f;
    public override void _Ready()
    {
        _sprite = GetNode<Sprite>("Path2D/PathFollow2D/Sprite");
        _pathFollow2D = GetNode<PathFollow2D>("Path2D/PathFollow2D");

        Random random = new Random();
        Vector2 RandomPathScale = new Vector2(random.Next(10, 100) / 100f, random.Next(20, 100) / 100f);
        GetNode<Path2D>("Path2D").Scale = random.Next(100) > 50 ? RandomPathScale : new Vector2(-RandomPathScale.x, RandomPathScale.y);
        _pathFollow2D.Scale = new Vector2(1 / RandomPathScale.x, 1 / RandomPathScale.y);

        _sprite.Modulate = new Color(_sprite.Modulate.r, _sprite.Modulate.g, _sprite.Modulate.b, 0);
    }
    public override void _PhysicsProcess(float delta)
    {
        if (_pathFollow2D.Offset < _limitOffsetForElement - 5)
        {
            _pathFollow2D.Offset += _elementSpeed;
            _elementSpeed -= _elementSlowdown;
            _sprite.Modulate = new Color(_sprite.Modulate.r, _sprite.Modulate.g, _sprite.Modulate.b, _sprite.Modulate.a + 0.1f);
        }
        else
        {
            const string link = "Path2D/PathFollow2D/";
            var explosionAnimation = GetNode<AnimatedSprite>(link + "ExplosionAnimation");
            var explosiveArea = GetNode<CollisionShape2D>(link + "ExplosiveArea/CollisionShape2D");
            if (explosionAnimation.Playing)
            {
                if (!explosiveArea.Disabled)
                    explosiveArea.Disabled = true;
                return;
            }
            GetNode<Sprite>(link + "Sprite").QueueFree();
            GetNode<AudioStreamPlayer>("ExplosionSound").Play();
            explosionAnimation.Visible = true;
            explosionAnimation.Play();
            explosiveArea.Disabled = false;
            EmitSignal("ElementExploded");
        }
    }
    public void Deleting()
    {
        QueueFree();
    }
}
