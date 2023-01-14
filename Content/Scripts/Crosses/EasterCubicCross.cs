using Godot;
using System;

public class EasterCubicCross : Node2D
{
    private int _ticksToNextPhase = 40, _ticksToExplosion = 60;
    public override void _Ready()
    {
        SetPhysicsProcess(false);
    }
    public void Visibled()
    {
        Modulate = new Color(Modulate.r, Modulate.g, Modulate.b, 0);
        SetPhysicsProcess(true);
    }
    public override void _PhysicsProcess(float delta)
    {
        if (_ticksToNextPhase > 0)
        {
            _ticksToNextPhase--;
            Scale = new Vector2(Scale.x - 0.05f, Scale.y - 0.05f);
            Modulate = new Color(Modulate.r, Modulate.g, Modulate.b, Modulate.a + 0.025f);
        }
        else if (_ticksToExplosion > 0)
        {
            _ticksToExplosion--;
            if (_ticksToExplosion == 60 || _ticksToExplosion == 45 || _ticksToExplosion == 30 || _ticksToExplosion == 15)
            {
                var crossSprite = GetNode<Sprite>("CrossSprite");
                crossSprite.Visible = false;
                var explosionSignal = GetNode<AudioStreamPlayer>("ExplosionSignal");
                explosionSignal.Play();
            }
            else if (_ticksToExplosion == 55 || _ticksToExplosion == 40 || _ticksToExplosion == 25 || _ticksToExplosion == 10)
            {
                var crossSprite = GetNode<Sprite>("CrossSprite");
                crossSprite.Visible = true;
            }
        }
        else
        {
            var explosionAnimation = GetNode<AnimatedSprite>("ExplosionAnimation");
            var explosiveArea = GetNode<CollisionShape2D>("ExplosiveArea/CollisionShape2D");
            if (explosionAnimation.Playing)
            {
                if (!explosiveArea.Disabled)
                    explosiveArea.Disabled = true;
                SetPhysicsProcess(false);
                return;
            }
            GetNode<Sprite>("CrossSprite").Visible = false;
            GetNode<Sprite>("WarningSprite").Visible = false;
            GetNode<AudioStreamPlayer>("ExplosionSound").Play();
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
