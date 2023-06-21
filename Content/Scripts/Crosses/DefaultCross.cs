using Godot;

public partial class DefaultCross : Node2D
{
    private int _ticksToNextPhase = 40, _ticksToExplosion = 60;
    public override void _Ready()
    {
        Modulate = new Color(Modulate.R, Modulate.G, Modulate.B, 0);
    }
    public override void _PhysicsProcess(double delta)
    {
        if (_ticksToNextPhase > 0)
        {
            _ticksToNextPhase--;
            Scale = new Vector2(Scale.X - 0.05f, Scale.Y - 0.05f);
            Modulate = new Color(Modulate.R, Modulate.G, Modulate.B, Modulate.A + 0.025f);
        }
        else if (_ticksToExplosion > 0)
        {
            if (_ticksToExplosion == 45 || _ticksToExplosion == 30 || _ticksToExplosion == 15)
            {
                GetNode<Sprite2D>("CrossSprite").Visible = false;
                GetNode<AudioStreamPlayer>("ExplosionSignal").Play();
            }
            else if (_ticksToExplosion == 40 || _ticksToExplosion == 25 || _ticksToExplosion == 10)
            {
                GetNode<Sprite2D>("CrossSprite").Visible = true;
            }
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
        }
    }
}