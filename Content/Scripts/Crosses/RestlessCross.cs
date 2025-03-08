using Godot;
using System;

public partial class RestlessCross : CrossNode
{
    protected CrossRotator R;
    protected const int TICKS_TO_APPEAR = 60;
    protected const int TICKS_TO_START_MOVE = TICKS_TO_APPEAR - 30;
    protected const int TICKS_TO_EXPLOSION = 90;
    protected bool _didSignaled;
    protected bool _shouldRotate = Meta.Instance.Video.CrossRotationWhenSpawning;

    public override void _Ready()
    {
        // Initializing nodes
        NodesInit();

        // Spawn properties
        Scale = new Vector2(3, 3);
        Modulate = new Color(1, 1, 1, 0);

        if (_shouldRotate)
            R = new(this, 180, 45);
    }

    private Color _mod = new Color(1, 1, 1, 0);
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        if (TicksLived <= TICKS_TO_APPEAR)
        {
            float TicksCoeff = TicksLived / TICKS_TO_APPEAR;
            TicksCoeff = Mathf.Lerp(0.0f, 1.0f, 1 - (1 - TicksCoeff) * (1 - TicksCoeff) * (1 - TicksCoeff));

            if (_shouldRotate)
                R.Rotate(TicksCoeff);

            WarningSprite.GlobalPosition = CrossSprite.GlobalPosition - new Vector2(2, 2) * (3 - 2 * TicksCoeff);
            Scale = new Vector2(3 - 2 * TicksCoeff, 3 - 2 * TicksCoeff);

            _mod.A = TicksCoeff;
            Modulate = _mod;
        }

        if (TicksLived >= TICKS_TO_START_MOVE && TicksLived < TICKS_TO_EXPLOSION)
        {
            if (!_didSignaled)
            {
                _didSignaled = true;
                GetNode<AudioStreamPlayer>("StartSignal").Play();   
            }

            Translate(new Vector2(10f * (TicksLived - TICKS_TO_START_MOVE) / 60f, 0).Rotated(Rotation));
        }
        else if (TicksLived >= TICKS_TO_EXPLOSION)
        {
            Explode();
        }
    }

    public override void Respawn()
    {
        base.Respawn();
        _shouldRotate = Meta.Instance.Video.CrossRotationWhenSpawning;

        Scale = new Vector2(3, 3);
        Modulate = new Color(1, 1, 1, 0);
        R?.Randomize();
        CrossSprite.Visible = true;
        WarningSprite.Visible = true;
        ExplosionAnimation.Visible = false;
        _didSignaled = false;
    }
}