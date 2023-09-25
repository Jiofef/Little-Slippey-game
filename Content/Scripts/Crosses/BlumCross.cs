using Godot;
using System;

public partial class BlumCross : Node2D
{
    private float _cycleSpeedMultiplier = 1f / 60 / 2, _xSpriteMotion, _ySpriteMotion = -3, _gravity = 9.8f;
    private byte _cyclesToExplosion = 10;
    private Sprite2D _crossSprite, _warningSprite, _abortButton;
    private AudioStreamPlayer _explosiveSignal;
    private bool _abortButtonPressed = false;
    private Random _random = new Random();

    public override void _Ready()
    {
        _crossSprite = GetNode<Sprite2D>("CrossSprite");
        _warningSprite = GetNode<Sprite2D>("WarningSprite");
        _abortButton = GetNode<Sprite2D>("AbortButton");
        _explosiveSignal = GetNode<AudioStreamPlayer>("ExplosionSignal");

        _crossSprite.Modulate = new Color(_crossSprite.Modulate.R + _cycleSpeedMultiplier, _crossSprite.Modulate.G, _crossSprite.Modulate.B);

        _explosiveSignal.Play();

        _xSpriteMotion = _random.Next(-2, 3);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_abortButtonPressed)
        {
            if (Modulate.A > 0)
            {
                Modulate = new Color(Modulate.R, Modulate.G, Modulate.B, Modulate.A - 0.03f);
                Translate(new Vector2(_xSpriteMotion, _ySpriteMotion));
                _ySpriteMotion += _gravity / 100;
                Rotation += 0.01f;
                return;
            }
            else QueueFree();
        }

        if (_cyclesToExplosion > 0)
        {
            if (Modulate.A < 1)
                Modulate = new Color(Modulate.R, Modulate.G, Modulate.B, Modulate.A + _cycleSpeedMultiplier);

            _crossSprite.Modulate = new Color(_crossSprite.Modulate.R + _cycleSpeedMultiplier, _crossSprite.Modulate.G, _crossSprite.Modulate.B);
            _warningSprite.Modulate = new Color(_warningSprite.Modulate.R, _warningSprite.Modulate.R, _warningSprite.Modulate.R, _warningSprite.Modulate.A + _cycleSpeedMultiplier);
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
            _warningSprite.QueueFree();
            _abortButton.QueueFree();
            GetNode<AudioStreamPlayer>("ExplosionSound").Play();
            explosionAnimation.Visible = true;
            explosionAnimation.Play();
            explosiveArea.Disabled = false;

            var Groups = GetGroups();
            for (int i = 0; i < Groups.Count; i++)
                RemoveFromGroup(Groups[i]);
        }
    }

    public void SignalPlayed()
    {
        if (_cyclesToExplosion <= 0) return;
        _cycleSpeedMultiplier *= 1.5f;
        _cyclesToExplosion--;

        _crossSprite.Modulate = new Color(0, _crossSprite.Modulate.G, _crossSprite.Modulate.B);
        _warningSprite.Modulate = new Color(_warningSprite.Modulate.R, _warningSprite.Modulate.R, _warningSprite.Modulate.R, 0);


        _explosiveSignal.PitchScale *= 1.5f;
        _explosiveSignal.Play();
    }

    public void AbortButtonPressed()
    {
        GetNode<AudioStreamPlayer>("AbortButtonPressedSound").Play();
        _abortButtonPressed = true;
    }   
}
