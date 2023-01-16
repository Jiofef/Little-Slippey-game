using Godot;
using System;

public class CannonCross : Path2D
{
    Node2D _sprites;
    PathFollow2D _cannonPathFollow2D, _ballPathFollow;
    Sprite _frontWheel, _backWheel, _barrel, _tornBarrel;
    private float _ballSpeed = 2, _cannonSpeed = 0.7f;
    private bool _doCannonShoted = false;
    public override void _Ready()
    {
        _sprites = GetNode<Node2D>("PathFollow2D/Cannon/Sprites");
        _cannonPathFollow2D = GetNode<PathFollow2D>("PathFollow2D");
        _ballPathFollow = GetNode<PathFollow2D>("PathFollow2D/Cannon/BallPath/BallPathFollow");
        _frontWheel = GetNode<Sprite>("PathFollow2D/Cannon/Sprites/FrontWheel");
        _backWheel = GetNode<Sprite>("PathFollow2D/Cannon/Sprites/BackWheel");
        _barrel = GetNode<Sprite>("PathFollow2D/Cannon/Sprites/Barrel");
        _tornBarrel = GetNode<Sprite>("PathFollow2D/Cannon/Sprites/TornBarrel");
    }
    public override void _PhysicsProcess(float delta)
    {
        if (_cannonPathFollow2D.UnitOffset < 0.95f)
        {
            _cannonPathFollow2D.Offset += _cannonSpeed;
            _backWheel.Rotation -= _cannonSpeed / 20;
            _frontWheel.Rotation -= _cannonSpeed / 20;

            if (_cannonPathFollow2D.UnitOffset >= 0.95f)
            {
                GetNode<AudioStreamPlayer>("PathFollow2D/Cannon/Sounds/RollingCannon").Stop();
                GetNode<AudioStreamPlayer>("PathFollow2D/Cannon/Sounds/Charge").Play();
            }
        }
        else if (!_doCannonShoted)
        {
            _barrel.Scale = new Vector2(_barrel.Scale.x + 0.0025f, _barrel.Scale.y - 0.005f);
            _barrel.Modulate = new Color(_barrel.Modulate.r, _barrel.Modulate.g - 0.0085f, _barrel.Modulate.b - 0.0085f);
            if (_barrel.Scale.y <= 0.6f)
            {
                GetNode<AudioStreamPlayer>("PathFollow2D/Cannon/Sounds/Shot").Play();
                GetNode<CPUParticles2D>("PathFollow2D/Cannon/ExplosionParticles").Emitting = true;
                GetNode<CollisionShape2D>("PathFollow2D/Cannon/BallPath/BallPathFollow/Hitbox/CollisionShape2D").Disabled = false;
                _barrel.Visible = false;
                _ballPathFollow.Visible = true;
                _tornBarrel.Visible = true;
                _doCannonShoted = true;
            }
        }
        else if (_ballPathFollow.UnitOffset < 0.95f)
        {
            _ballPathFollow.Offset += _ballSpeed;
            if (_sprites.Position.x < 50)
            {
                _sprites.Position = new Vector2(_sprites.Position.x + 1.5f, _sprites.Position.y);
                _sprites.Rotation += 0.06f;
            }
        }
        else
        {
            QueueFree();
        }
    }
}
