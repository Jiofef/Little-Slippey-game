using Godot;
using System;
using OtherExtension;

public partial class DefaultCross : CrossNode
{
    CrossRotator R;

    const int TICKS_TO_APPEAR = 60, TICKS_TO_EXPLOSION = 90;

    // Visual rotating effect
    private bool _shouldRotate = Meta.Instance.Video.CrossRotationWhenSpawning;

    public override void _Ready()
    {
        // Initializing nodes
        NodesInit();

        // Spawn properties
        Scale = new Vector2(3, 3);
        Modulate = _mod;

        if (_shouldRotate)
            R = new(this, 75, 30);
    }

    private Color _mod = new Color(1, 1, 1, 0), _spriteMod = new Color(1, 1, 1);
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        if (TicksLived <= TICKS_TO_APPEAR)
        {
            float TicksCoeff = TicksLived / TICKS_TO_APPEAR;
            TicksCoeff = MathTools.EaseOut(TicksCoeff, 3);

            if (_shouldRotate)
                R.Rotate(TicksCoeff);

            WarningSprite.GlobalPosition = CrossSprite.GlobalPosition - new Vector2(2, 2) * (3 - 2 * TicksCoeff);
            Scale = new Vector2(3 - 2 * TicksCoeff, 3 - 2 * TicksCoeff);

            _mod.A = TicksCoeff;
            Modulate = _mod;
        }
        if (TicksLived <= TICKS_TO_EXPLOSION && TicksLived >= TICKS_TO_APPEAR - 30)
        {
            _spriteMod.A = CrossSprite.Modulate.A - 0.1f;

            if (TicksLived % 15 == 0)
                _spriteMod.A = 1f;

            CrossSprite.Modulate = _spriteMod;
        }
        else if (TicksLived >= TICKS_TO_EXPLOSION)
            Explode();
    }

    public override void Respawn()
    {
        base.Respawn();

        // Base settings
        _shouldRotate = Meta.Instance.Video.CrossRotationWhenSpawning;

        Scale = new Vector2(3, 3);
        Modulate = new Color(Modulate.R, Modulate.G, Modulate.B, 0);
        R.Randomize();

        // Returning the old settings
        CrossSprite.Visible = true;
        WarningSprite.Visible = true;

        ExplosionAnimation.Visible = false;
    }
}