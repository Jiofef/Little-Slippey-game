using Godot;
using System;

public class RestlessCross : Node2D
{
    [Signal] public delegate void AnimationStarted();
    [Signal] public delegate void WarningStarted();
    private int _ticksToNextPhase = 40, _ticksToExplosion = 60;
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
        if (_ticksToNextPhase > 0)
        {
            _ticksToNextPhase --;
            Scale = new Vector2(Scale.x - 0.05f, Scale.y - 0.05f);
            Modulate = new Color(Modulate.r, Modulate.g, Modulate.b, Modulate.a + 0.025f);
        }
        else if (_ticksToExplosion > 0)
        {
            if (!_doSignaled)
            {
                _doSignaled = true;
                EmitSignal("WarningStarted");
            }
            _ticksToExplosion--;
            if (_ticksToExplosion == 60 || _ticksToExplosion == 45 || _ticksToExplosion == 30 || _ticksToExplosion == 15)
            {
                GetNode<Sprite>(_link + "CrossSprite").Visible = false;
                GetNode<AudioStreamPlayer>(_link + "ExplosionSignal").Play();
            }
            else if (_ticksToExplosion == 55 || _ticksToExplosion == 40 || _ticksToExplosion == 25 || _ticksToExplosion == 10)
            {
                GetNode<Sprite>(_link + "CrossSprite").Visible = true;
            }
        }
        else
        {
            var explosionAnimation = GetNode<AnimatedSprite>(_link + "ExplosionAnimation");
            var explosiveArea = GetNode<CollisionShape2D>(_link + "ExplosiveArea/CollisionShape2D");
            if (explosionAnimation.Playing)
            {
                explosiveArea.Disabled = true;
                SetPhysicsProcess(false);
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