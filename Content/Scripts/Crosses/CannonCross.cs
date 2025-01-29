using Godot;
using static OtherExtension.RandomTools;

public partial class CannonCross : Path2D
{
    Node2D _sprites, _ball;
    PathFollow2D _cannonPathFollow2D;
    Sprite2D _frontWheel, _backWheel, _barrel, _ballSprite;
    AudioStreamPlayer _chargeSound;
    private float _ballYBound = G.CameraLimits.End.Y + 256, _ballYMotion = 0.5f, _appearedCoeff = 0, _afterShotCoeff = 0;

    private Rect2 _ballBounds = new Rect2(
        G.CameraLimits.Position + new Vector2(-256, -256),
        G.CameraLimits.Size + new Vector2(256, 512));

    private bool _didCannonShot, _isLevelTooWide = G.CameraLimits.Size.X / G.CameraLimits.Size.Y > 10;
    public override void _Ready()
    {
        // Initializing nodes
        #region
        _sprites = GetNode<Node2D>("PathFollow2D/Cannon/Sprites");
        _ball = GetNode<Node2D>("PathFollow2D/Cannon/Ball");
        _ballSprite = GetNode<Sprite2D>("PathFollow2D/Cannon/Ball/Ball");
        _cannonPathFollow2D = GetNode<PathFollow2D>("PathFollow2D");
        _frontWheel = GetNode<Sprite2D>("PathFollow2D/Cannon/Sprites/FrontWheel");
        _backWheel = GetNode<Sprite2D>("PathFollow2D/Cannon/Sprites/BackWheel");
        _barrel = GetNode<Sprite2D>("PathFollow2D/Cannon/Sprites/Barrel");
        _chargeSound = GetNode<AudioStreamPlayer>("PathFollow2D/Cannon/Sounds/Charge");
        #endregion



        // 25 and 125 are off-camera positions that don't show the sudden appearance of the cannon

        // If level is too wide or infinite
        if (_isLevelTooWide)
        {
            RotationDegrees = FiftyFifty() ? 90 : -90;
            GlobalPosition = new Vector2(RandomIn(G.Player.GlobalPosition.X - 940, G.Player.GlobalPosition.X + 940), RotationDegrees == 90 ? G.CameraLimits.Position.Y - 25 : G.CameraLimits.End.Y + 120);
            return;
        }
        else
        {
            Scale = new Vector2(FiftyFifty() ? 1 : -1, 1);
            GlobalPosition = Scale.X == -1 ?
                new Vector2(G.CameraLimits.End.X + 25, RandomIn(G.CameraLimits.Position.Y, G.CameraLimits.End.Y)) :
                new Vector2(G.CameraLimits.Position.X - 25, RandomIn(G.CameraLimits.Position.Y, G.CameraLimits.End.Y));
        }
    }
    public override void _PhysicsProcess(double delta)
    {
        if (_appearedCoeff <= 0.99f)
        {
            // Cannon traveling animation
            _appearedCoeff += 0.012f;
            float LogCoeff = Mathf.Lerp(0.0f, 1.0f, 1 - (1 - _appearedCoeff) * (1 - _appearedCoeff));
            _cannonPathFollow2D.ProgressRatio = LogCoeff;
            _backWheel.Rotation = -LogCoeff * 3;
            _frontWheel.Rotation = -LogCoeff * 3;
            _barrel.RotationDegrees = -120 + LogCoeff * 60;
        }
        if (_appearedCoeff < 0.0f) return;



        if (!_didCannonShot)
        {
            // Animation of cannon charging
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
                _didCannonShot = true;

                var Groups = GetGroups();
                for (int i = 0; i < Groups.Count; i++)
                    RemoveFromGroup(Groups[i]);
            }
        }
        else
        {
            // Ball flight
            if (!_isLevelTooWide)
                _ball.Translate(new Vector2(-1.75f, _ballYMotion));
            else
                _ball.GlobalTranslate(new Vector2(0, 7.5f * (RotationDegrees == 90 ? 1 : -1)));

            _ballSprite.Rotation -= 0.06f;
            _ballYMotion -= 0.006f;

            // Cannon animation
            if (_afterShotCoeff <= 0.98)
            {
                _afterShotCoeff += 0.15f;
                float LogCoeff = Mathf.Lerp(0.0f, 0.6f, 1 - (1 - _afterShotCoeff) * (1 - _afterShotCoeff));
                _barrel.Scale = new Vector2(1, 0.4f + LogCoeff);
            }

            if (_sprites.Visible == true && _sprites.Position.X < 50)
            {
                _sprites.Position = new Vector2(_sprites.Position.X + 2f, _sprites.Position.Y);
                _sprites.Rotation -= 0.1f;
            }
            else if (_sprites.Visible == true)
                _sprites.Visible = false;

            if (IsBallOutOfBounds())
                QueueFree();
        }
    }

    public bool IsBallOutOfBounds()
    {
        return !_ballBounds.HasPoint(_ball.GlobalPosition);
    }
}
