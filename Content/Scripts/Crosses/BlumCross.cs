using Godot;
using System;

public partial class BlumCross : CrossNode
{
    private float _cycleSpeedMultiplier = 1f / 60 / 2, _xSpriteMotion, _ySpriteMotion = -3;
    private const float GRAVITY = 9.8f;
    private byte _cyclesToExplosion = 10;
    public Sprite2D AbortButton;
    private AudioStreamPlayer _explosiveSignal;
    private bool _abortButtonPressed = false;
    private Random _random = new Random();
    private Vector2 ShakeCenter;

    public override void _Ready()
    {
        // Initializing nodes
        NodesInit();

        AddToGroup("Crosses");

        AbortButton = GetNode<Sprite2D>("AbortButton");
        _explosiveSignal = GetNode<AudioStreamPlayer>("ExplosionSignal");

        // Spawn properties
        Modulate = new Color(Modulate.R, Modulate.G, Modulate.B, 0);


        CrossSprite.Modulate = new Color(CrossSprite.Modulate.R + _cycleSpeedMultiplier, CrossSprite.Modulate.G, CrossSprite.Modulate.B);

        _explosiveSignal.Play();

        _xSpriteMotion = _random.Next(-2, 3);
    }

    public override void OnPositionSetted()
    {
        ShakeCenter = CrossSprite.GlobalPosition;
    }

    public override void _PhysicsProcess(double delta)
    {
        GlobalPosition = ShakeCenter + new Vector2(_random.Next(22 - _cyclesToExplosion * 2), _random.Next(22 - _cyclesToExplosion * 2));
        if (_abortButtonPressed)
        {
            if (Modulate.A > 0)
            {
                Modulate = new Color(Modulate.R, Modulate.G, Modulate.B, Modulate.A - 0.03f);
                Translate(new Vector2(_xSpriteMotion, _ySpriteMotion));
                _ySpriteMotion += GRAVITY / 100;
                Rotation += 0.01f;
                return;
            }
            else OnFinished();
        }

        if (_cyclesToExplosion > 0)
        {
            if (Modulate.A < 1)
                Modulate = new Color(Modulate.R, Modulate.G, Modulate.B, Modulate.A + _cycleSpeedMultiplier);

            CrossSprite.Modulate = new Color(CrossSprite.Modulate.R + _cycleSpeedMultiplier, CrossSprite.Modulate.G, CrossSprite.Modulate.B);
            WarningSprite.Modulate = new Color(WarningSprite.Modulate.R, WarningSprite.Modulate.R, WarningSprite.Modulate.R, WarningSprite.Modulate.A + _cycleSpeedMultiplier);
        }
        else
        {
            AbortButton.Visible = false;
            AbortButton.ProcessMode = ProcessModeEnum.Disabled;
            Explode();
        }
    }

    public void SignalPlayed()
    {
        if (_cyclesToExplosion <= 0) return;
        _cycleSpeedMultiplier *= 1.5f;
        _cyclesToExplosion--;

        CrossSprite.Modulate = new Color(0, CrossSprite.Modulate.G, CrossSprite.Modulate.B);
        WarningSprite.Modulate = new Color(WarningSprite.Modulate.R, WarningSprite.Modulate.R, WarningSprite.Modulate.R, 0);


        _explosiveSignal.PitchScale *= 1.5f;
        _explosiveSignal.Play();
    }

    public void AbortButtonPressed()
    {
        _explosiveSignal.Stop();
        GetNode<AudioStreamPlayer>("AbortButtonPressedSound").Play();
        _abortButtonPressed = true;
    }

    public override void Respawn()
    {
        base.Respawn();

        AddToGroup("Crosses");

        // Base settings
        Modulate = new Color(Modulate.R, Modulate.G, Modulate.B, 0);

        CrossSprite.Modulate = new Color(CrossSprite.Modulate.R + _cycleSpeedMultiplier, CrossSprite.Modulate.G, CrossSprite.Modulate.B);
        _xSpriteMotion = _random.Next(-2, 3);

        // Returning the old settings
        CrossSprite.Visible = true;
        WarningSprite.Visible = true;

        ExplosionAnimation.Visible = false;

        AbortButton.Visible = true;
        AbortButton.ProcessMode = ProcessModeEnum.Inherit;

        Rotation = 0;

        _abortButtonPressed = false;
        _cyclesToExplosion = 10;
        _cycleSpeedMultiplier = 1f / 60 / 2;
        _ySpriteMotion = -3;

        _explosiveSignal.Play();
        _explosiveSignal.PitchScale = 0.25f;
        GetNode<AudioStreamPlayer>("AbortButtonPressedSound").Stop();


        OnPositionSetted();
    }
}
