using Godot;
using System;

public partial class RestlessCross : Node2D
{
    [Export] bool _isCrossEnhanced;
    private int _ticksToExplosion = 60;
    private bool _isSignaled;
    private float _timerToExplosion;
    public override void _Ready()
    {
        Random random = new Random();
        RotationDegrees = random.Next(-180, 180);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (Scale.X > 1 && Scale.Y > 1 && Modulate.A < 1)
        {
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
                Vector2 playerPos = GetNode<CharacterBody2D>("../Player").Position;
                float RotationValue = GetAngleTo(playerPos) / 10;
                if (RotationValue > 0.033f)
                    RotationValue = 0.033f;
                else if (RotationValue < -0.033f)
                    RotationValue = -0.033f;
                Rotation += RotationValue;

                var pointingRect = GetNode<Node2D>("PointingRect");
                pointingRect.Rotation += pointingRect.GetAngleTo(playerPos) / 30;
                if (pointingRect.RotationDegrees > 70)
                    pointingRect.RotationDegrees = 70;
                else if (pointingRect.RotationDegrees < -70)
                    pointingRect.RotationDegrees = -70;
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

            var Groups = GetGroups();
            for (int i = 0; i < Groups.Count; i++)
                RemoveFromGroup(Groups[i]);
        }
    }
}