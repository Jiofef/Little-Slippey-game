using Godot;
using System;

public partial class RestlessCross : Node2D
{
    [Export] bool _isCrossEnhanced;
    private int _ticksToNextPhase = 40, _ticksToExplosion = 60;
    private bool _isSignaled;
    private float _timerToExplosion;
    public override void _Ready()
    {
        Modulate = new Color(Modulate.R, Modulate.G, Modulate.B, 0);
        Random random = new Random();
        RotationDegrees = random.Next(-180, 180);
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
            if (!_isSignaled)
            {
                _isSignaled = true;
                GetNode<AudioStreamPlayer>("StartSignal").Play();
            }
            if (_isCrossEnhanced)
            {
                float AngleToPlayer = GetAngleTo(GetNode<CharacterBody2D>("../Player").Position);
                float RotationValue = AngleToPlayer / 10;
                if (RotationValue > 0.025f)
                    RotationValue = 0.025f;
                else if (RotationValue < -0.025f)
                    RotationValue = -0.025f;
                Rotation += RotationValue;
            }
            Translate(new Vector2(10f * _timerToExplosion, 0).Rotated(Rotation));
            _timerToExplosion += 0.016667f;
            _ticksToExplosion--;
        }
        else
        {
            var explosionAnimation = GetNode<AnimatedSprite2D>("ExplosionAnimation");
            var explosiveArea = GetNode<CollisionShape2D>("ExplosiveArea/CollisionShape2D");
            if (explosionAnimation.IsPlaying())
            {
                explosiveArea.Disabled = true;
                SetPhysicsProcess(false);
                return;
            }
            GetNode<Sprite2D>("CrossSprite").QueueFree();
            GetNode<Sprite2D>("WarningSprite").QueueFree();
            GetNode<AudioStreamPlayer>("ExplosionSound").Play();
            explosionAnimation.Visible = true;
            explosionAnimation.Play();
            explosiveArea.Disabled = false;
        }
    }
}