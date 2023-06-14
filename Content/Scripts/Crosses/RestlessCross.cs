using Godot;
using System;

public partial class RestlessCross : Node2D
{
    [Signal] public delegate void AnimationStartedEventHandler();
    [Signal] public delegate void WarningStartedEventHandler();
    private int _ticksToNextPhase = 40, _ticksToExplosion = 60;
    private const string _link = "Path2D/PathFollow2D/FollowNode/";
    private bool _doSignaled;
    public override void _Ready()
    {
        Modulate = new Color(Modulate.R, Modulate.G, Modulate.B, 0);
        Random random = new Random();
        Rotation = random.Next(-180, 180);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_ticksToNextPhase > 0)
        {
            _ticksToNextPhase --;
            Scale = new Vector2(Scale.X - 0.05f, Scale.Y - 0.05f);
            Modulate = new Color(Modulate.R, Modulate.G, Modulate.B, Modulate.A + 0.025f);
        }
        else if (_ticksToExplosion > 0)
        {
            if (!_doSignaled)
            {
                _doSignaled = true;
                EmitSignal("WarningStarted");
                GetNode<AudioStreamPlayer>(_link + "StartSignal").Play();
            }
            _ticksToExplosion--;
        }
        else
        {
            var explosionAnimation = GetNode<AnimatedSprite2D>(_link + "ExplosionAnimation");
            var explosiveArea = GetNode<CollisionShape2D>(_link + "ExplosiveArea/CollisionShape2D");
            if (explosionAnimation.IsPlaying())
            {
                explosiveArea.Disabled = true;
                SetPhysicsProcess(false);
                return;
            }
            GetNode<Sprite2D>(_link + "CrossSprite").QueueFree();
            GetNode<Sprite2D>(_link + "WarningSprite").QueueFree();
            GetNode<AudioStreamPlayer>(_link + "ExplosionSound").Play();
            explosionAnimation.Visible = true;
            explosionAnimation.Play();
            explosiveArea.Disabled = false;
        }
    }
}