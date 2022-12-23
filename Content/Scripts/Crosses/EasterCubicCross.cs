using Godot;
using System;

public class EasterCubicCross : Node2D
{
    private float _replacemodulate = 0, _operationstotal = 40, _deletetimer = 60;
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
        if (!Visible) return;
        if (_operationstotal > 0)
        {
            _operationstotal--;
            _replacemodulate += 0.025f;
            Scale = new Vector2(Scale.x - 0.05f, Scale.y - 0.05f);
            Modulate = new Color(Modulate.r, Modulate.g, Modulate.b, _replacemodulate);
        }
        else if (_deletetimer > 0)
        {
            _deletetimer--;
            if (_deletetimer == 60 || _deletetimer == 45 || _deletetimer == 30 || _deletetimer == 15)
            {
                var crossSprite = GetNode<Sprite>("CrossSprite");
                crossSprite.Visible = false;
                var explosionSignal = GetNode<AudioStreamPlayer>("ExplosionSignal");
                explosionSignal.Play();
            }
            else if (_deletetimer == 55 || _deletetimer == 40 || _deletetimer == 25 || _deletetimer == 10)
            {
                var crossSprite = GetNode<Sprite>("CrossSprite");
                crossSprite.Visible = true;
            }
        }
        else
        {
            var explosionAnimation = GetNode<AnimatedSprite>("ExplosionAnimation");
            if (explosionAnimation.Playing)
                return;
            var crossSprite = GetNode<Sprite>("CrossSprite");
            crossSprite.Visible = false;
            var warningSprite = GetNode<Sprite>("WarningSprite");
            warningSprite.Visible = false;
            explosionAnimation.Visible = true;
            explosionAnimation.Play();
            var explosionSound = GetNode<AudioStreamPlayer>("ExplosionSound");
            explosionSound.Play();
        }
    }
    public void Deleting()
    {
        QueueFree();
    }
}