using Godot;
using System;

public class GreenElementalCrossPart : Node2D
{
    [Signal] public delegate void ElementExploded();
    private const int _flowerSpritesCount = 4;
    private Sprite _sprite;
    private PathFollow2D _pathFollow2D;
    private AnimatedSprite _vineAnimation;
    private const float _elementsMultipleSLowdown = 1.04f, _limitOffsetForElement = 75;
    private float _elementSpeed = 3f;
    private bool _doExplosed = false;
    public override void _Ready()
    {
        Random random = new Random();

        _sprite = GetNode<Sprite>("Path2D/PathFollow2D/Sprite");

        _sprite.Texture = ResourceLoader.Load("res://Content/Sprites/Crosses/GreenElementalCrossPartVar" + (random.Next(_flowerSpritesCount) + 1) + ".png") as Texture;

        _pathFollow2D = GetNode<PathFollow2D>("Path2D/PathFollow2D");

        _vineAnimation = GetNode<AnimatedSprite>("Path2D/VineAnimation");

        Vector2 RandomPathScale = new Vector2(random.Next(25, 100) / 100f, random.Next(40, 100) / 100f);
        RandomPathScale.y *= random.Next(100) > 50 ? -1 : 1;
        GetNode<Path2D>("Path2D").Scale = random.Next(100) > 50 ? RandomPathScale : new Vector2(-RandomPathScale.x, RandomPathScale.y);
        _pathFollow2D.Scale = new Vector2(1 / RandomPathScale.x, 1 / RandomPathScale.y);
        _pathFollow2D.Modulate = new Color(_pathFollow2D.Modulate.r, _pathFollow2D.Modulate.g, _pathFollow2D.Modulate.b, 0);

        GetNode<AnimatedSprite>("Path2D/VineAnimation").Play();

        GetNode<CPUParticles2D>("ScrapsParticles").Emitting = true;
    }
    public override void _PhysicsProcess(float delta)
    {
        if (_pathFollow2D.Offset < _limitOffsetForElement - 5)
        {
            _pathFollow2D.Offset += _elementSpeed;
            _elementSpeed /= _elementsMultipleSLowdown;
            _pathFollow2D.Modulate = new Color(_pathFollow2D.Modulate.r, _pathFollow2D.Modulate.g, _pathFollow2D.Modulate.b, _pathFollow2D.Modulate.a + 0.1f);
        }
        else if (!_doExplosed)
        {

            const string link = "Path2D/PathFollow2D/";
            var explosionAnimation = GetNode<AnimatedSprite>(link + "ExplosionAnimation");
            var explosiveArea = GetNode<CollisionShape2D>(link + "ExplosiveArea/CollisionShape2D");
            if (explosionAnimation.Playing)
            {
                explosiveArea.Disabled = true;
                SetPhysicsProcess(false);
                _doExplosed = true;
                return;
            }
            GetNode<Sprite>(link + "Sprite").QueueFree();
            GetNode<AudioStreamPlayer>("ExplosionSound").Play();
            explosionAnimation.Visible = true;
            explosionAnimation.Play();
            explosiveArea.Disabled = false;
            EmitSignal("ElementExploded");
        }
        else
        {
            _vineAnimation.Modulate = new Color(_vineAnimation.Modulate.r - 0.03f, _vineAnimation.Modulate.g - 0.03f, _vineAnimation.Modulate.b - 0.03f, _vineAnimation.Modulate.a - 0.03f);
        }
    }
    public void Deleting()
    {
        QueueFree();
    }
}
