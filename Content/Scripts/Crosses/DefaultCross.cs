using Godot;

public partial class DefaultCross : Node2D
{
    Sprite2D _crossSprite, _warningSprite;

    private int _ticksToExplosion = 60;

    public override void _Ready()
    {
        _crossSprite = GetNode<Sprite2D>("CrossSprite");
        _warningSprite = GetNode<Sprite2D>("WarningSprite");
    }
    public override void _PhysicsProcess(double delta)
    {
        if (Scale.X > 1 && Scale.Y > 1 && Modulate.A < 1)
        {
            Scale = new Vector2(Scale.X - 0.05f, Scale.Y - 0.05f);
            Modulate = new Color(Modulate.R, Modulate.G, Modulate.B, Modulate.A + 0.025f);
        }
        else if (_ticksToExplosion > 0)
        {
            _ticksToExplosion--;
            _crossSprite.Modulate = new Color(_crossSprite.Modulate.R, _crossSprite.Modulate.G, _crossSprite.Modulate.B, _crossSprite.Modulate.A - 0.1f);
            if (_ticksToExplosion == 45 || _ticksToExplosion == 30 || _ticksToExplosion == 15)
                _crossSprite.Modulate = new Color(_crossSprite.Modulate.R, _crossSprite.Modulate.G, _crossSprite.Modulate.B, 1);
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

            var Groups = GetGroups();
            for (int i = 0; i < Groups.Count; i++)
                RemoveFromGroup(Groups[i]);
        }
    }
}