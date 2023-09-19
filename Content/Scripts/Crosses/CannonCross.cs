using Godot;

public partial class CannonCross : Path2D
{
    Node2D _sprites, _ball;
    PathFollow2D _cannonPathFollow2D;
    Sprite2D _frontWheel, _backWheel, _barrel, _tornBarrel, _ballSprite;
    private float _cannonSpeed = 0.7f, _ballYBound = G.LevelXYSizes[G.CurrentLevel].Y + 100, _ballYMotion = 0.5f;
    private bool _doCannonShoted;
    public override void _Ready()
    {
        _sprites = GetNode<Node2D>("PathFollow2D/Cannon/Sprites");
        _ball = GetNode<Node2D>("PathFollow2D/Cannon/Ball");
        _ballSprite = GetNode<Sprite2D>("PathFollow2D/Cannon/Ball/Ball");
        _cannonPathFollow2D = GetNode<PathFollow2D>("PathFollow2D");
        _frontWheel = GetNode<Sprite2D>("PathFollow2D/Cannon/Sprites/FrontWheel");
        _backWheel = GetNode<Sprite2D>("PathFollow2D/Cannon/Sprites/BackWheel");
        _barrel = GetNode<Sprite2D>("PathFollow2D/Cannon/Sprites/Barrel");
        _tornBarrel = GetNode<Sprite2D>("PathFollow2D/Cannon/Sprites/TornBarrel");
    }
    public override void _PhysicsProcess(double delta)
    {
        if (_cannonPathFollow2D.ProgressRatio < 0.95f)
        {
            _cannonPathFollow2D.Progress += _cannonSpeed;
            _backWheel.Rotation -= _cannonSpeed / 20;
            _frontWheel.Rotation -= _cannonSpeed / 20;

            if (_cannonPathFollow2D.ProgressRatio >= 0.95f)
            {
                GetNode<AudioStreamPlayer>("PathFollow2D/Cannon/Sounds/RollingCannon").Stop();
                GetNode<AudioStreamPlayer>("PathFollow2D/Cannon/Sounds/Charge").Play();
            }
        }
        else if (!_doCannonShoted)
        {
            _barrel.Scale = new Vector2(_barrel.Scale.X + 0.0025f, _barrel.Scale.Y - 0.005f);
            _barrel.Modulate = new Color(_barrel.Modulate.R, _barrel.Modulate.G - 0.0085f, _barrel.Modulate.B - 0.0085f);
            if (_barrel.Scale.Y <= 0.6f)
            {
                GetNode<AudioStreamPlayer>("PathFollow2D/Cannon/Sounds/Shot").Play();
                GetNode<CpuParticles2D>("PathFollow2D/Cannon/ExplosionParticles").Emitting = true;
                GetNode<CollisionShape2D>("PathFollow2D/Cannon/Ball/Hitbox/CollisionShape2D").Disabled = false;
                _barrel.Visible = false;
                _ball.Visible = true;
                _tornBarrel.Visible = true;
                _doCannonShoted = true;
            }
        }
        else if (_ball.GlobalPosition.Y < _ballYBound && G.CurrentLevel != 8 || _ball.GlobalPosition.Y < 800 && _ball.GlobalPosition.Y > -100 && G.CurrentLevel == 8)
        {
            if (G.CurrentLevel != 8)
                _ball.Translate(new Vector2(-1.75f, _ballYMotion));
            else
                _ball.GlobalTranslate(new Vector2(0, 7.5f * (RotationDegrees == 90 ? 1 : -1)));
            _ballSprite.Rotation -= 0.06f;
            _ballYMotion -= 0.006f;
            if (_sprites.Position.X < 50)
            {
                _sprites.Position = new Vector2(_sprites.Position.X + 1.5f, _sprites.Position.Y);
                _sprites.Rotation -= 0.06f;
            }
        }
        else
            QueueFree();
    }
}
