using Godot;

public partial class EnhancedDefaultCross : Node2D
{
    Sprite2D _crossSprite, _warningSprite;

    private int _ticksToExplosion = 90;
    private float _warningSpriteFallSpeedMultiplier = -1f;

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

            if (_ticksToExplosion == 89)
                GetNode<CollisionShape2D>("TriggerArea/CollisionShape2D").Disabled = false;
            else if (_ticksToExplosion == 0)
                GetNode<AudioStreamPlayer>("Warning").Play();

            _crossSprite.Modulate = new Color(_crossSprite.Modulate.R, _crossSprite.Modulate.G, _crossSprite.Modulate.B, _crossSprite.Modulate.A - 0.05f);
            if (_ticksToExplosion == 70 || _ticksToExplosion == 50 || _ticksToExplosion == 30 || _ticksToExplosion == 0)
                _crossSprite.Modulate = new Color(_crossSprite.Modulate.R, _crossSprite.Modulate.G, _crossSprite.Modulate.B, 1);
        }
        else if (_warningSprite.Scale > Vector2.Zero && _warningSprite.Modulate.A > 0)
        {
            _crossSprite.Modulate = new Color(_crossSprite.Modulate.R, _crossSprite.Modulate.G, _crossSprite.Modulate.B, _crossSprite.Modulate.A - 0.05f);
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
            if (Material == null || Material.ResourceName != "StaticNoise")
                explosiveArea.Disabled = false;

            var Groups = GetGroups();
            for (int i = 0; i < Groups.Count; i++)
                RemoveFromGroup(Groups[i]);
        }
    }

    public void TRIGGERED()
    {
        _crossSprite.Visible = true;
        _ticksToExplosion = 0;
    }
}
