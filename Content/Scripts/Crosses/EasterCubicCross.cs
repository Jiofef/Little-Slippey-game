using Godot;
using System;

public class EasterCubicCross : Node2D
{
    private int _operationstotal = 40, _deletetimer = 60;
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
        if (_operationstotal > 0)
        {
            _operationstotal--;
            Scale = new Vector2(Scale.x - 0.05f, Scale.y - 0.05f);
            Modulate = new Color(Modulate.r, Modulate.g, Modulate.b, Modulate.a + 0.025f);
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
            var explosiveArea = GetNode<CollisionShape2D>("ExplosiveArea/CollisionShape2D");
            if (explosionAnimation.Playing)
            {
                if (!explosiveArea.Disabled)
                    explosiveArea.Disabled = true;
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
