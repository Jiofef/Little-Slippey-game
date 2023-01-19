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
                GetNode<Sprite>("CrossSprite").Visible = false;
                GetNode<AudioStreamPlayer>("ExplosionSignal").Play();
            }
            else if (_ticksToExplosion == 55 || _ticksToExplosion == 40 || _ticksToExplosion == 25 || _ticksToExplosion == 10)
            {
                GetNode<Sprite>("CrossSprite").Visible = true;
            }
        }
        else
        {
            GetNode<Sprite>("CrossSprite").Visible = false;
            GetNode<Sprite>("WarningSprite").Visible = false;
            GetNode<AudioStreamPlayer>("ExplosionSound").Play();
            var explosionAnimation = GetNode<AnimatedSprite>("ExplosionAnimation");
            explosionAnimation.Visible = true;
            explosionAnimation.Play();
            SetPhysicsProcess(false);
        }
    }
    public void Deleting()
    {
        QueueFree();
    }
}
