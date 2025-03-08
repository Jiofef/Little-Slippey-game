using Godot;
using System;
using OtherExtension;

public partial class GreenElementalCrossPart : ElementalCrossPart
{
    [Signal] public delegate void ElementExplodedEventHandler();
    private const int _flowerSpritesCount = 4;
    private Sprite2D _sprite, _vineSprite;
    private PathFollow2D _pathFollow2D;
    private float _elementSpeed = 9f;
    private bool _doExplosed = false;
    public override void _Ready()
    {
        // Initializing nodes
        NodesInit();
        _vineSprite = GetNode<Sprite2D>("Path2D/Vine");
        Random random = new Random();
        CrossSprite.Texture = ResourceLoader.Load("res://Content/Sprites/Crosses/GreenElementalCrossPartVar" + (random.Next(_flowerSpritesCount) + 1) + ".png") as Texture2D;

        LifeTime = 1.5f;
        RandomizePathVec(new Rect2(-225, -60, 170, 120));
        PathVec.X *= RandomTools.FiftyFifty() ? 1 : -1; // Mirroring the vec randomly

        GetNode<Path2D>("Path2D").Scale = new Vector2(random.Next(25, 100) / 100f * (random.Next(100) > 50 ? 1 : -1), random.Next(40, 100) / 100f);
        _pathFollow2D.GlobalScale = new Vector2(1, 1);
        _vineSprite.GlobalScale = new Vector2(4, 4);
        _pathFollow2D.Modulate = new Color(_pathFollow2D.Modulate.R, _pathFollow2D.Modulate.G, _pathFollow2D.Modulate.B, 0);
        GetNode<CpuParticles2D>("ScrapsParticles").Emitting = true;
    }
    public override void _PhysicsProcess(double delta)
    {
        if (_pathFollow2D.ProgressRatio < 0.98f)
        {
            _vineSprite.RegionRect = new Rect2(0, 0, new Vector2(_pathFollow2D.Progress / _vineSprite.Scale.X, 7));
            _pathFollow2D.Progress += _elementSpeed;
            _elementSpeed /= 1.04f;
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

            GetNode<CpuParticles2D>("Path2D/Fireparticles").Emitting = true;
        }
        else
        {
            _vineSprite.Modulate = new Color(_vineSprite.Modulate.R - 0.03f, _vineSprite.Modulate.G - 0.03f, _vineSprite.Modulate.B - 0.03f, _vineSprite.Modulate.A - 0.03f);
        }
    }
}
