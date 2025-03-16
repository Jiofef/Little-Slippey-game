using Godot;
using System;
using OtherExtension;

public partial class EnhancedDefaultCross : CrossNode
{
    // Time constants (in ticks)
    private const int TICKS_TO_APPEAR = 60;
    private const int TICKS_TO_EXPLOSION_START = 40;
    private const int TICKS_TO_EXPLOSION_END = 90;

    // Rotation
    private CrossRotator R;
    private const int INITIAL_ROTATION_RANGE = 75;
    private const int FINAL_ROTATION_RANGE = 30;
    private bool _shouldRotate = Meta.Instance.Video.CrossRotationWhenSpawning;

    // Effects
    private float _warningSpriteFallSpeedMultiplier = -1f;

    public override void _Ready()
    {
        NodesInit();

        AddToGroup("Crosses");

        // Initialize spawn properties
        Scale = new Vector2(3, 3);
        Modulate = new Color(1, 1, 1, 0);

        // Initialize rotator
        if (_shouldRotate)
        {
            R = new CrossRotator(this, INITIAL_ROTATION_RANGE, FINAL_ROTATION_RANGE);
            R.Randomize();
        }
    }

    Color _crossMod = new Color(1, 1, 1, 1);
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        // Spawn phase
        if (TicksLived <= TICKS_TO_APPEAR)
        {
            float TicksCoeff = TicksLived / TICKS_TO_APPEAR;
            TicksCoeff = MathTools.EaseOut(TicksCoeff, 3);

            // Rotation animation
            if (_shouldRotate)
                R.Rotate(TicksCoeff);

            // Scale and transparency
            Scale = new Vector2(3 - 2 * TicksCoeff, 3 - 2 * TicksCoeff);
            Modulate = new Color(1, 1, 1, TicksCoeff);
        }

        // Rest of the logic remains unchanged
        if (TicksLived >= TICKS_TO_EXPLOSION_START)
        {
            if (TicksLived < TICKS_TO_EXPLOSION_END)
            {
                if (TicksLived == TICKS_TO_EXPLOSION_START)
                    GetNode<CollisionShape2D>("TriggerArea/CollisionShape2D").Disabled = false;

                _crossMod.A -= 0.05f;
                if ((TicksLived % 20) == 0)
                    _crossMod.A = 1f;

                CrossSprite.Modulate = _crossMod;
            }
            else
            {
                if (WarningSprite.Scale > Vector2.Zero)
                {
                    CrossSprite.Modulate = new Color(1, 1, 1, CrossSprite.Modulate.A - 0.02f);
                    WarningSprite.Scale -= new Vector2(1f * _warningSpriteFallSpeedMultiplier, 1f * _warningSpriteFallSpeedMultiplier);
                    _warningSpriteFallSpeedMultiplier += 0.08f;
                }
                else
                {
                    Explode();
                }
            }
        }
    }


    public override void Explode()
    {
        if (IsExploded) return;
        base.Explode();

        // Enhanced-specific actions
        GetNode<CollisionShape2D>("TriggerArea/CollisionShape2D").Disabled = true;
    }

    public override void Respawn()
    {
        base.Respawn();

        AddToGroup("Crosses");

        // Base settings
        _shouldRotate = Meta.Instance.Video.CrossRotationWhenSpawning;
        _crossMod = new Color(1, 1, 1, 1);

        Scale = new Vector2(3, 3);
        Modulate = new Color(Modulate.R, Modulate.G, Modulate.B, 0);
        R.Randomize();

        // Returning the old settings
        CrossSprite.Visible = true;
        WarningSprite.Visible = true;

        ExplosionAnimation.Visible = false;

        // Base enhanced options
        _warningSpriteFallSpeedMultiplier = -1f;
        WarningSprite.Scale = new Vector2(10, 10);
        CrossSprite.Modulate = new Color(1, 1, 1, 1);
        GetNode<CollisionShape2D>("TriggerArea/CollisionShape2D").Disabled = true;
    }

    // When player enters the cross area
    public void TRIGGERED()
    {
        if (TicksLived < TICKS_TO_EXPLOSION_END)
        {
            TicksLived = TICKS_TO_EXPLOSION_END;
            CrossSprite.Modulate = new Color(1, 1, 1, 1);
        }
    }
}