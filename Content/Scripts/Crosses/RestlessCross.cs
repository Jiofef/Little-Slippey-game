using Godot;
using System;

public partial class RestlessCross : Node2D
{
    [Export] bool _isCrossEnhanced;
    private int _ticksToExplosion = 60;
    private float _defaultTicksToAppear = 60;
    private float _ticksToAppear = 0;

    private float defaultRotation;
    private float rotationGoal;

    private bool _isSignaled;
    private float _timerToExplosion;

    public override void _Ready()
    {
        _ticksToAppear = _defaultTicksToAppear;

        Random random = new Random();
        defaultRotation = random.Next(-75, 75);
        rotationGoal = random.Next(-30, 30);

        RotationDegrees = defaultRotation;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_ticksToAppear > 0)
        {
            _ticksToAppear--;
            float TicksCoeff = 1 - (_ticksToAppear / _defaultTicksToAppear);
            TicksCoeff = Mathf.Lerp(0.0f, 1.0f, 1 - (1 - TicksCoeff) * (1 - TicksCoeff) * (1 - TicksCoeff));

            RotationDegrees = defaultRotation + rotationGoal * TicksCoeff;
            Scale = new Vector2(3 - 2 * TicksCoeff, 3 - 2 * TicksCoeff);
            Modulate = new Color(Modulate.R, Modulate.G, Modulate.B, TicksCoeff);
        }
        if (_ticksToExplosion > 0 && (_ticksToAppear <= 0 || _ticksToAppear <= 30))
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
        else if (_ticksToExplosion <= 0)
        {
            var explosionAnimation = GetNode<AnimatedSprite2D>("ExplosionAnimation");
            var explosiveArea = GetNode<CollisionShape2D>("ExplosiveArea/CollisionShape2D");

            if (!explosionAnimation.IsPlaying())
            {
                GetNode<Sprite2D>("CrossSprite").QueueFree();
                GetNode<Sprite2D>("WarningSprite").QueueFree();
                GetNode<AudioStreamPlayer>("ExplosionSound").Play();
                explosionAnimation.Visible = true;
                explosionAnimation.Play();
                explosiveArea.Disabled = false;
                return;
            }

            explosiveArea.Disabled = true;
            SetPhysicsProcess(false);

            foreach (var group in GetGroups())
                RemoveFromGroup(group);
        }
    }
}