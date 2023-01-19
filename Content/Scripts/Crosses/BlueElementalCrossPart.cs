using Godot;
using System;

public class BlueElementalCrossPart : Node2D
{
    [Signal] public delegate void ElementExploded();
    private Sprite _sprite;
    private PathFollow2D _pathFollow2D;
    private const float _elementAcceleration = 0.04f, _limitOffsetForElement = 125;
    private float _elementSpeed = 0.5f;
    public override void _Ready()
    {
        _sprite = GetNode<Sprite>("Path2D/PathFollow2D/Sprite");
        _pathFollow2D = GetNode<PathFollow2D>("Path2D/PathFollow2D");

        Random random = new Random();
        Vector2 RandomPathScale = new Vector2(random.Next(10, 100) / 100f, random.Next(20, 100) / 100f);
        GetNode<Path2D>("Path2D").Scale = random.Next(100) > 50 ? RandomPathScale : new Vector2(-RandomPathScale.x, RandomPathScale.y);
        _pathFollow2D.Scale = new Vector2(1 / RandomPathScale.x, 1 / RandomPathScale.y);

        _sprite.Modulate = new Color(_sprite.Modulate.r, _sprite.Modulate.g, _sprite.Modulate.b, 0);

        var spawnSound = GetNode<AudioStreamPlayer>("SpawnSound");
        spawnSound.Stream = ResourceLoader.Load<AudioStream>("res://Content/Sounds/Crosses/BlueElementalCrossPartSoundVar" + (random.Next(3) + 1) + ".wav");
        spawnSound.Play();
    }
    public override void _PhysicsProcess(float delta)
    {
        if (_pathFollow2D.Offset < _limitOffsetForElement - 5)
        {
            _pathFollow2D.Offset += _elementSpeed;
            _elementSpeed += _elementAcceleration;
            _sprite.Modulate = new Color(_sprite.Modulate.r, _sprite.Modulate.g, _sprite.Modulate.b, _sprite.Modulate.a + 0.1f);
        }
        else
        {
            const string link = "Path2D/PathFollow2D/";
            var explosionAnimation = GetNode<AnimatedSprite>(link +"ExplosionAnimation");
            var explosiveArea = GetNode<CollisionShape2D>(link + "ExplosiveArea/CollisionShape2D");
            if (explosionAnimation.Playing)
            {
                explosiveArea.Disabled = true;
                SetPhysicsProcess(false);
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
