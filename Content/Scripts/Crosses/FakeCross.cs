using Godot;
using OtherExtension;
using System;

public partial class FakeCross : UnusualCrossNode
{
    const int TICKS_TO_APPEAR = 60, TICKS_TO_EXPLOSION = 90;

    public AnimatedSprite2D CrossSprite;
    public Sprite2D WarningSprite;

    public override void _Ready()
    {
        // Initializing nodes
        CrossSprite = GetNode<AnimatedSprite2D>("CrossSprite");
        WarningSprite = GetNode<Sprite2D>("WarningSprite");

        Scale = new Vector2(0, 0);
    }

    private Color _spriteMod = new Color(1, 1, 1);
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        if (TicksLived <= TICKS_TO_APPEAR)
        {
            float TicksCoeff = TicksLived / TICKS_TO_APPEAR;
            TicksCoeff = MathTools.EaseOut(TicksCoeff, 3);

            WarningSprite.GlobalPosition = CrossSprite.GlobalPosition - new Vector2(2, 2) * (3 - 2 * TicksCoeff);
            Scale = new Vector2(TicksCoeff, TicksCoeff);
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

    public void Explode()
    {
        var explosionAnimation = GetNode<ExplosionAnimation>("ExplosionAnimation");

        explosionAnimation.Show();
        explosionAnimation.Play();

        CrossSprite.Hide();
        WarningSprite.Hide();

        SetPhysicsProcess(false);
    }

    public override void Respawn()
    {
        base.Respawn();

        Scale = new Vector2(0, 0);

        CrossSprite.Show();
        WarningSprite.Show();

        SetPhysicsProcess(true);
    }
}
