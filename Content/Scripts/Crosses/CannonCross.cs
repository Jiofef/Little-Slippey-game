using Godot;

public partial class CannonCross : Path2D
{
    Node2D _sprites, _ball;
    PathFollow2D _cannonPathFollow2D;
    Sprite2D _frontWheel, _backWheel, _barrel, _ballSprite;
    AudioStreamPlayer _chargeSound;
    private float _ballYBound = G.LevelXYSizes[G.CurrentLevel].Y + 100, _ballYMotion = 0.5f, _appearedCoeff = 0, _afterShotCoeff = 0;
    private bool _doCannonShoted;
    public override void _Ready()
    {
        _sprites = GetNode<Node2D>("PathFollow2D/Cannon/Sprites");
        _ball = GetNode<Node2D>("PathFollow2D/Cannon/Ball");
        _ball.ProcessMode = ProcessModeEnum.Disabled;
        _ballSprite = GetNode<Sprite2D>("PathFollow2D/Cannon/Ball/Ball");
        _cannonPathFollow2D = GetNode<PathFollow2D>("PathFollow2D");
        _frontWheel = GetNode<Sprite2D>("PathFollow2D/Cannon/Sprites/FrontWheel");
        _backWheel = GetNode<Sprite2D>("PathFollow2D/Cannon/Sprites/BackWheel");
        _barrel = GetNode<Sprite2D>("PathFollow2D/Cannon/Sprites/Barrel");
        _chargeSound = GetNode<AudioStreamPlayer>("PathFollow2D/Cannon/Sounds/Charge");
    }
    public override void _PhysicsProcess(double delta)
    {
        if (_appearedCoeff <= 0.99f)
        {
            _appearedCoeff += 0.012f;
            float LogCoeff = Mathf.Lerp(0.0f, 1.0f, 1 - (1 - _appearedCoeff) * (1 - _appearedCoeff));
            _cannonPathFollow2D.ProgressRatio = LogCoeff;
            _backWheel.Rotation = -LogCoeff * 3;
            _frontWheel.Rotation = -LogCoeff * 3;
            _barrel.RotationDegrees = -120 + LogCoeff * 60;
        }
        if (_appearedCoeff >= 0.0f)
        {
            if (!_doCannonShoted)
            {
                if (!_chargeSound.Playing)
                    _chargeSound.Play();
                _barrel.Scale = new Vector2(_barrel.Scale.X + 0.0025f, _barrel.Scale.Y - 0.005f);
                _barrel.Modulate = new Color(_barrel.Modulate.R, _barrel.Modulate.G - 0.0085f, _barrel.Modulate.B - 0.0085f);
                if (_barrel.Scale.Y <= 0.6f)
                {
                    GetNode<AudioStreamPlayer>("PathFollow2D/Cannon/Sounds/Shot").Play();
                    GetNode<CpuParticles2D>("PathFollow2D/Cannon/ExplosionParticles").Emitting = true;
                    GetNode<CollisionShape2D>("PathFollow2D/Cannon/Ball/Hitbox/CollisionShape2D").Disabled = false;
                    GetNode<CpuParticles2D>("PathFollow2D/Cannon/Ball/CPUParticles2D").Emitting = true;
                    _barrel.Texture = GD.Load<CompressedTexture2D>("res://Content/Sprites/Crosses/TornCannonBarrel.png");
                    _ball.Visible = true;
                    _ball.ProcessMode = ProcessModeEnum.Pausable;
                    _doCannonShoted = true;

                    var Groups = GetGroups();
                    for (int i = 0; i < Groups.Count; i++)
                        RemoveFromGroup(Groups[i]);
                }
            }
            else if (_ball.GlobalPosition.Y < _ballYBound && G.CurrentLevel != 8 || _ball.GlobalPosition.Y < 800 && _ball.GlobalPosition.Y > -100 && G.CurrentLevel == 8)
            {
                if (G.CurrentLevel != 8)
                    _ball.Translate(new Vector2(-1.75f, _ballYMotion));
                else
                    _ball.GlobalTranslate(new Vector2(0, 7.5f * (RotationDegrees == 90 ? 1 : -1)));
                if (_afterShotCoeff <= 0.98)
                {
                    _afterShotCoeff += 0.15f;
                    float LogCoeff = Mathf.Lerp(0.0f, 0.6f, 1 - (1 - _afterShotCoeff) * (1 - _afterShotCoeff));
                    _barrel.Scale = new Vector2(1, 0.4f + LogCoeff);
                }
                _ballSprite.Rotation -= 0.06f;
                _ballYMotion -= 0.006f;

                if (_sprites.Visible == true && _sprites.Position.X < 50)
                {
                    _sprites.Position = new Vector2(_sprites.Position.X + 2f, _sprites.Position.Y);
                    _sprites.Rotation -= 0.1f;
                }
                else if (_sprites.Visible == true)
                    _sprites.Visible = false;
            }
            else
                QueueFree();
        }
    }
}
