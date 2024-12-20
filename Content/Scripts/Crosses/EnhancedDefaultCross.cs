using Godot;
using System;

public partial class EnhancedDefaultCross : Node2D
{
    Sprite2D _crossSprite, _warningSprite;

    private int _ticksToExplosion = 90;
    private float _warningSpriteFallSpeedMultiplier = -1f;

    private float _defaultTicksToAppear = 60;
    private float _ticksToAppear = 0;

    private float _defaultRotation;
    private float _rotationGoal;

    public override void _Ready()
    {
        _ticksToAppear = _defaultTicksToAppear;

        Random random = new Random();
        _defaultRotation = random.Next(-75, 75);
        _rotationGoal = random.Next(-30, 30);

        RotationDegrees = _defaultRotation;

        _crossSprite = GetNode<Sprite2D>("CrossSprite");
        _warningSprite = GetNode<Sprite2D>("WarningSprite");
    }
    public override void _PhysicsProcess(double delta)
    {
        if (_ticksToAppear > 0)
        {
            _ticksToAppear--;
            float TicksCoeff = 1 - (_ticksToAppear / _defaultTicksToAppear);
            TicksCoeff = Mathf.Lerp(0.0f, 1.0f, 1 - (1 - TicksCoeff) * (1 - TicksCoeff) * (1 - TicksCoeff));

            RotationDegrees = _defaultRotation + _rotationGoal * TicksCoeff;
            Scale = new Vector2(3 - 2 * TicksCoeff, 3 - 2 * TicksCoeff);
            Modulate = new Color(Modulate.R, Modulate.G, Modulate.B, TicksCoeff);
        }
        if (_ticksToAppear < 40)
        {
            if (_ticksToExplosion > 0)
            {
                _ticksToExplosion--;

                if (_ticksToExplosion == 89)
                    GetNode<CollisionShape2D>("TriggerArea/CollisionShape2D").Disabled = false;

                _crossSprite.Modulate = new Color(_crossSprite.Modulate.R, _crossSprite.Modulate.G, _crossSprite.Modulate.B, _crossSprite.Modulate.A - 0.05f);
                if (_ticksToExplosion == 70 || _ticksToExplosion == 50 || _ticksToExplosion == 30 || _ticksToExplosion == 0)
                    _crossSprite.Modulate = new Color(_crossSprite.Modulate.R, _crossSprite.Modulate.G, _crossSprite.Modulate.B, 1);
            }
            else if (_warningSprite.Scale > Vector2.Zero)
            {
                _crossSprite.Modulate = new Color(_crossSprite.Modulate.R, _crossSprite.Modulate.G, _crossSprite.Modulate.B, _crossSprite.Modulate.A - 0.02f);
                _warningSprite.Scale -= new Vector2(1f * _warningSpriteFallSpeedMultiplier, 1f * _warningSpriteFallSpeedMultiplier);
                _warningSpriteFallSpeedMultiplier += 0.08f;
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
                _crossSprite.QueueFree();
                _warningSprite.Visible = false;
                GetNode<AudioStreamPlayer>("ExplosionSound").Play();
                GetNode<CollisionShape2D>("TriggerArea/CollisionShape2D").Disabled = true;
                explosionAnimation.Visible = true;
                explosionAnimation.Play();
                explosiveArea.Disabled = false;

                var Groups = GetGroups();
                for (int i = 0; i < Groups.Count; i++)
                    RemoveFromGroup(Groups[i]);
            }
        }

    }

    public void TRIGGERED()
    {
        _crossSprite.Modulate = new Color(_crossSprite.Modulate.R, _crossSprite.Modulate.G, _crossSprite.Modulate.B, 1);
        _ticksToExplosion = 0;
    }
}
