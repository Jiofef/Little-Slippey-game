using Godot;
using System;
using System.IO;

public class BlueElementalCrossPart : Node2D
{
    [Signal] public delegate void ElementExploded();
    private Sprite sprite;
    private PathFollow2D pathFollow2D;
    private const float _elementacceleration = 0.04f, _limitoffsetforelement = 125;
    private float _elementspeed = 0.5f;
    public override void _Ready()
    {
        sprite = GetNode<Sprite>("Path2D/PathFollow2D/Sprite");
        pathFollow2D = GetNode<PathFollow2D>("Path2D/PathFollow2D");

        Random random = new Random();
        Vector2 RandomPathScale = new Vector2(random.Next(10, 100) / 100f, random.Next(20, 100) / 100f);
        GetNode<Path2D>("Path2D").Scale = random.Next(100) > 50 ? RandomPathScale : new Vector2(-RandomPathScale.x, RandomPathScale.y);
        pathFollow2D.Scale = new Vector2(1 / RandomPathScale.x, 1 / RandomPathScale.y);

        sprite.Modulate = new Color(sprite.Modulate.r, sprite.Modulate.g, sprite.Modulate.b, 0);

        var spawnSound = GetNode<AudioStreamPlayer>("SpawnSound");
        spawnSound.Stream = ResourceLoader.Load<AudioStream>("res://Content/Sounds/Crosses/BlueElementalCrossPartSoundVar" + (random.Next(3) + 1) + ".wav");
        spawnSound.Play();
    }
    public override void _PhysicsProcess(float delta)
    {
        if (pathFollow2D.Offset < _limitoffsetforelement - 5)
        {
            pathFollow2D.Offset += _elementspeed;
            _elementspeed += _elementacceleration;
            sprite.Modulate = new Color(sprite.Modulate.r, sprite.Modulate.g, sprite.Modulate.b, sprite.Modulate.a + 0.1f);
        }
        else
        {
            const string link = "Path2D/PathFollow2D/";
            var explosionAnimation = GetNode<AnimatedSprite>(link +"ExplosionAnimation");
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
