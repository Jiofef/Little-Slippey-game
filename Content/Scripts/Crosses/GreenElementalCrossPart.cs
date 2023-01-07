using Godot;
using System;

public class GreenElementalCrossPart : Node2D
{
    [Signal] public delegate void ElementExploded();
    private const int _flowerspritescount = 4;
    private Sprite sprite;
    private PathFollow2D pathFollow2D;
    private AnimatedSprite vineAnimation;
    private const float _elementsmultiplelowdown = 1.04f, _limitoffsetforelement = 75;
    private float _elementspeed = 3f;
    private bool _doexplosed = false;
    public override void _Ready()
    {
        Random random = new Random();

        sprite = GetNode<Sprite>("Path2D/PathFollow2D/Sprite");

        sprite.Texture = ResourceLoader.Load("res://Content/Sprites/Crosses/GreenElementalCrossPartVar" + (random.Next(_flowerspritescount) + 1) + ".png") as Texture;

        pathFollow2D = GetNode<PathFollow2D>("Path2D/PathFollow2D");

        vineAnimation = GetNode<AnimatedSprite>("Path2D/VineAnimation");

        Vector2 RandomPathScale = new Vector2(random.Next(25, 100) / 100f, random.Next(40, 100) / 100f);
        RandomPathScale.y *= random.Next(100) > 50 ? -1 : 1;
        GetNode<Path2D>("Path2D").Scale = random.Next(100) > 50 ? RandomPathScale : new Vector2(-RandomPathScale.x, RandomPathScale.y);
        pathFollow2D.Scale = new Vector2(1 / RandomPathScale.x, 1 / RandomPathScale.y);
        pathFollow2D.Modulate = new Color(pathFollow2D.Modulate.r, pathFollow2D.Modulate.g, pathFollow2D.Modulate.b, 0);

        GetNode<AnimatedSprite>("Path2D/VineAnimation").Play();

        GetNode<CPUParticles2D>("ScrapsParticles").Emitting = true;
    }
    public override void _PhysicsProcess(float delta)
    {
        if (pathFollow2D.Offset < _limitoffsetforelement - 5)
        {
            pathFollow2D.Offset += _elementspeed;
            _elementspeed /= _elementsmultiplelowdown;
            pathFollow2D.Modulate = new Color(pathFollow2D.Modulate.r, pathFollow2D.Modulate.g, pathFollow2D.Modulate.b, pathFollow2D.Modulate.a + 0.1f);
        }
        else if (!_doexplosed)
        {
            _doexplosed = true;
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
        else
        {
            vineAnimation.Modulate = new Color(vineAnimation.Modulate.r - 0.03f, vineAnimation.Modulate.g - 0.03f, vineAnimation.Modulate.b - 0.03f, vineAnimation.Modulate.a - 0.03f);
        }
    }
    public void Deleting()
    {
        QueueFree();
    }
}
