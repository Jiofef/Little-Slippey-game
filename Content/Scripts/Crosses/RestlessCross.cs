using Godot;
using System;

public class RestlessCross : Node2D
{
    [Signal] public delegate void AnimationStarted();
    [Signal] public delegate void WarningStarted();
    private int _operationsTotal = 40, _deleteTimer = 60;
    private const string _link = "Path2D/PathFollow2D/FollowNode/";
    private bool _doSignaled;
    public override void _Ready()
    {
        Modulate = new Color(Modulate.r, Modulate.g, Modulate.b, 0);
        Random random = new Random();
        Rotation = random.Next(-180, 180);
    }

    public override void _PhysicsProcess(float delta)
    {
        if (_operationsTotal > 0)
        {
            _operationsTotal --;
            Scale = new Vector2(Scale.x - 0.05f, Scale.y - 0.05f);
            Modulate = new Color(Modulate.r, Modulate.g, Modulate.b, Modulate.a + 0.025f);
        }
        else if (_deleteTimer > 0)
        {
            if (!_doSignaled)
            {
                _doSignaled = true;
                EmitSignal("WarningStarted");
            }
            _deleteTimer--;
            if (_deleteTimer == 60 || _deleteTimer == 45 || _deleteTimer == 30 || _deleteTimer == 15)
            {
                var crossSprite = GetNode<Sprite>(_link + "CrossSprite");
                crossSprite.Visible = false;
                var explosionSignal = GetNode<AudioStreamPlayer>(_link + "ExplosionSignal");
                explosionSignal.Play();
            }
            else if (_deleteTimer == 55 || _deleteTimer == 40 || _deleteTimer == 25 || _deleteTimer == 10)
            {
                var crossSprite = GetNode<Sprite>(_link + "CrossSprite");
                crossSprite.Visible = true;
            }
        }
        else
        {
            var explosionAnimation = GetNode<AnimatedSprite>(_link + "ExplosionAnimation");
            var explosiveArea = GetNode<CollisionShape2D>(_link + "ExplosiveArea/CollisionShape2D");
            if (explosionAnimation.Playing)
            {
                if (!explosiveArea.Disabled)
                    explosiveArea.Disabled = true;
                return;
            }
            GetNode<Sprite>(_link + "CrossSprite").QueueFree();
            GetNode<Sprite>(_link + "WarningSprite").QueueFree();
            GetNode<AudioStreamPlayer>(_link + "ExplosionSound").Play();
            explosionAnimation.Visible = true;
            explosionAnimation.Play();
            explosiveArea.Disabled = false;
        }
    }
    public void Deleting()
    {
        QueueFree();
    }
}