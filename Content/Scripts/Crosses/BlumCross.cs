using Godot;
using System;

public class BlumCross : Node2D
{
    private float _cycleSpeedMultiplier = 1f / 60 / 2, _xSpriteMotion, _ySpriteMotion = -3, _gravity = 9.8f;
    private byte _cyclesToExplosion = 10;
    private Sprite _crossSprite, _warningSprite, _abortButton;
    private AudioStreamPlayer _explosiveSignal;
    private bool _abortButtonPressed = false;
    private Random _random = new Random();
    public override void _Ready()
    {
        Modulate = new Color(Modulate.r, Modulate.g, Modulate.b, 0);
        _crossSprite = GetNode<Sprite>("CrossSprite");
        _warningSprite = GetNode<Sprite>("WarningSprite");
        _abortButton = GetNode<Sprite>("AbortButton");
        _explosiveSignal = GetNode<AudioStreamPlayer>("ExplosionSignal");

        _crossSprite.Modulate = new Color(_crossSprite.Modulate.r + _cycleSpeedMultiplier, _crossSprite.Modulate.g, _crossSprite.Modulate.b);

        _explosiveSignal.Play();

        _xSpriteMotion = _random.Next(-2, 3);
    }
    public override void _PhysicsProcess(float delta)
    {
        if (_abortButtonPressed)
        {
            if (Modulate.a != 0)
            {
                Modulate = new Color(Modulate.r, Modulate.g, Modulate.b, Modulate.a - 0.03f);
                Position = new Vector2(Position.x + _xSpriteMotion, Position.y + _ySpriteMotion);
                _ySpriteMotion += _gravity / 100;
                Rotation += 0.01f;
                return;
            }
            else QueueFree();
        }
        if (_cyclesToExplosion > 0)
        {
            if (Modulate.a < 1)
                Modulate = new Color(Modulate.r, Modulate.g, Modulate.b, Modulate.a + _cycleSpeedMultiplier);

            _crossSprite.Modulate = new Color(_crossSprite.Modulate.r + _cycleSpeedMultiplier, _crossSprite.Modulate.g, _crossSprite.Modulate.b);
            _warningSprite.Modulate = new Color(_warningSprite.Modulate.r, _warningSprite.Modulate.r, _warningSprite.Modulate.r, _warningSprite.Modulate.a + _cycleSpeedMultiplier);
        }
        else
        {
            var explosionAnimation = GetNode<AnimatedSprite>("ExplosionAnimation");
            var explosiveArea = GetNode<CollisionShape2D>("ExplosiveArea/CollisionShape2D");
            if (explosionAnimation.Playing)
            {
                if (!explosiveArea.Disabled)
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
        }
    }
    public void SignalPlayed()
    {
        if (_cyclesToExplosion <= 0) return;
        _cycleSpeedMultiplier *= 1.5f;
        _cyclesToExplosion--;

        _crossSprite.Modulate = new Color(0, _crossSprite.Modulate.g, _crossSprite.Modulate.b);
        _warningSprite.Modulate = new Color(_warningSprite.Modulate.r, _warningSprite.Modulate.r, _warningSprite.Modulate.r, 0);

        _explosiveSignal.PitchScale *= 1.5f;
        _explosiveSignal.Play();
    }
    public void AbortButtonPressed(Node junk)
    {
        GetNode<AudioStreamPlayer>("AbortButtonPressedSound").Play();
        _abortButtonPressed = true;
        GD.Print(Position);
    }
    public void HideExplosionAnimation()
    {
        var explosionAnimation = GetNode<AnimatedSprite>("ExplosionAnimation");
        explosionAnimation.Stop();
        explosionAnimation.Visible = false;
    }
    public void Deleting()
    {
        QueueFree();
    }
}
