 using Godot;
using System;

public class DefaultCross : Node2D
{
    private int _operationsTotal = 40, _deleteTimer = 60;
    public override void _Ready()
    {
        Modulate = new Color(Modulate.r, Modulate.g, Modulate.b, 0);
    }
    public override void _PhysicsProcess(float delta)
    {
        if (_operationsTotal > 0)
        {
            _operationsTotal--;
            Scale = new Vector2(Scale.x - 0.05f, Scale.y - 0.05f);
            Modulate = new Color(Modulate.r, Modulate.g, Modulate.b, Modulate.a + 0.025f);
        }
        else if (_deleteTimer > 0)
        {
            _deleteTimer--;
            if (_deleteTimer == 60 || _deleteTimer == 45 || _deleteTimer == 30 || _deleteTimer == 15)
            {
                GetNode<Sprite>("CrossSprite").Visible = false;
                GetNode<AudioStreamPlayer>("ExplosionSignal").Play();
            }
            else if (_deleteTimer == 55 || _deleteTimer == 40 || _deleteTimer == 25 || _deleteTimer == 10)
            {
                GetNode<Sprite>("CrossSprite").Visible = true;
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
            GetNode<Sprite>("CrossSprite").QueueFree();
            GetNode<Sprite>("WarningSprite").QueueFree();
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