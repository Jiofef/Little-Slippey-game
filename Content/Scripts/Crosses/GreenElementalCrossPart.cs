using Godot;
using System;

public partial class GreenElementalCrossPart : Node2D
{
    [Signal] public delegate void ElementExplodedEventHandler();
    private const int _flowerSpritesCount = 4;
    private Sprite2D _sprite;
    private PathFollow2D _pathFollow2D;
    private AnimatedSprite2D _vineAnimation;
    private const float _elementsMultipleSLowdown = 1.04f;
    private float _elementSpeed = 3f;
    private bool _doExplosed = false;
    public override void _Ready()
    {
        Random random = new Random();

        _sprite = GetNode<Sprite2D>("Path2D/PathFollow2D/Sprite2D");

        _sprite.Texture = ResourceLoader.Load("res://Content/Sprites/Crosses/GreenElementalCrossPartVar" + (random.Next(_flowerSpritesCount) + 1) + ".png") as Texture2D;

        _pathFollow2D = GetNode<PathFollow2D>("Path2D/PathFollow2D");

        _vineAnimation = GetNode<AnimatedSprite2D>("Path2D/VineAnimation");

        GetNode<Path2D>("Path2D").Scale = new Vector2(random.Next(25, 100) / 100f * (random.Next(100) > 50 ? 1 : -1), random.Next(40, 100) / 100f);
        _pathFollow2D.GlobalScale = new Vector2(3, 3);
        _pathFollow2D.Modulate = new Color(_pathFollow2D.Modulate.R, _pathFollow2D.Modulate.G, _pathFollow2D.Modulate.B, 0);

        GetNode<AnimatedSprite2D>("Path2D/VineAnimation").Play();

        GetNode<CpuParticles2D>("ScrapsParticles").Emitting = true;
    }
    public override void _PhysicsProcess(double delta)
    {
        if (_pathFollow2D.ProgressRatio < 0.98f)
        {
            _pathFollow2D.Progress += _elementSpeed;
            _elementSpeed /= _elementsMultipleSLowdown;
            _pathFollow2D.Modulate = new Color(_pathFollow2D.Modulate.R, _pathFollow2D.Modulate.G, _pathFollow2D.Modulate.B, _pathFollow2D.Modulate.A + 0.1f);
        }
        else if (!_doExplosed)
        {

            const string link = "Path2D/PathFollow2D/";
            var explosionAnimation = GetNode<AnimatedSprite2D>(link + "ExplosionAnimation");
            var explosiveArea = GetNode<CollisionShape2D>(link + "ExplosiveArea/CollisionShape2D");
            if (explosionAnimation.IsPlaying())
            {
                explosiveArea.Disabled = true;
                _doExplosed = true;
                return;
            }
            GetNode<Sprite2D>(link + "Sprite2D").QueueFree();
            GetNode<AudioStreamPlayer>("ExplosionSound").Play();
            explosionAnimation.Visible = true;
            explosionAnimation.Play();
            explosiveArea.Disabled = false;
            EmitSignal("ElementExploded");
        }
        else
        {
            _vineAnimation.Modulate = new Color(_vineAnimation.Modulate.R - 0.03f, _vineAnimation.Modulate.G - 0.03f, _vineAnimation.Modulate.B - 0.03f, _vineAnimation.Modulate.A - 0.03f);
        }
    }
    public void Deleting()
    {
        QueueFree();
    }
}
