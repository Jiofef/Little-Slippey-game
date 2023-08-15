using Godot;
using System;

public partial class Level9SpitCross : Node2D
{
    private Vector2 _crossVelocity;

    private int _ticksToExplosion = 60;
    Random _random = new Random();

    public override void _Ready()
    {
        _ticksToExplosion = _random.Next(45, 76);

        _crossVelocity = (GetNode<CharacterBody2D>("../Player").GlobalPosition - GlobalPosition) / 20;
        _crossVelocity += new Vector2(_crossVelocity.X / 15 * _random.Next(-3, 4), _crossVelocity.Y / 15 * _random.Next(-3, 4));
    }

    public override void _PhysicsProcess(double delta)
	{
        Translate(new Vector2(_random.Next(-4, 5), _random.Next(-4, 5)) + _crossVelocity);
        _crossVelocity *= 0.955f;
        if (_ticksToExplosion > 0)
            _ticksToExplosion--;
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
