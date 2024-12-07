using Godot;
using System;

public partial class DefaultCross : Node2D
{
    Sprite2D _crossSprite, _warningSprite;

    private int _ticksToExplosion = 60;
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
            _warningSprite.GlobalPosition = _crossSprite.GlobalPosition - new Vector2(2, 2) * (3 - 2 * TicksCoeff);
            Scale = new Vector2(3 - 2 * TicksCoeff, 3 - 2 * TicksCoeff);
            Modulate = new Color(Modulate.R, Modulate.G, Modulate.B, TicksCoeff);
        }
        if (_ticksToExplosion > 0 && (_ticksToAppear <= 0 || _ticksToAppear <= 30))
        {
            _ticksToExplosion--;
            _crossSprite.Modulate = new Color(_crossSprite.Modulate.R, _crossSprite.Modulate.G, _crossSprite.Modulate.B, _crossSprite.Modulate.A - 0.1f);

            if (_ticksToExplosion % 15 == 0)
                _crossSprite.Modulate = new Color(_crossSprite.Modulate.R, _crossSprite.Modulate.G, _crossSprite.Modulate.B, 1);
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