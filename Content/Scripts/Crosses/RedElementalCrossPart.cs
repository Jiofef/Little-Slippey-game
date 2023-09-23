using Godot;
using System;

public partial class RedElementalCrossPart : Node2D
{
    [Signal] public delegate void ElementExplodedEventHandler();
    private Sprite2D _sprite;
    private PathFollow2D _pathFollow2D;
    private const float _elementSlowdown = 0.03f;
    private float _elementSpeed = 4.5f;
    public override void _Ready()
    {
        _sprite = GetNode<Sprite2D>("Path2D/PathFollow2D/Sprite2D");
        _pathFollow2D = GetNode<PathFollow2D>("Path2D/PathFollow2D");

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
            _elementSpeed -= _elementSlowdown;
            _sprite.Modulate = new Color(_sprite.Modulate.R, _sprite.Modulate.G, _sprite.Modulate.B, _sprite.Modulate.A + 0.1f);
        }
        else
        {
            const string link = "Path2D/PathFollow2D/";
            var explosionAnimation = GetNode<AnimatedSprite2D>(link + "ExplosionAnimation");
            var explosiveArea = GetNode<CollisionShape2D>(link + "ExplosiveArea/CollisionShape2D");
            if (explosionAnimation.IsPlaying())
            {
                explosiveArea.Disabled = true;
                SetPhysicsProcess(false);
                return;
            }
            GetNode<Sprite2D>(link + "Sprite2D").QueueFree();
            GetNode<AudioStreamPlayer>("ExplosionSound").Play();
            explosionAnimation.Visible = true;
            explosionAnimation.Play();
            explosiveArea.Disabled = false;
            EmitSignal("ElementExploded");
        }
    }
}
