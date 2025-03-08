using Godot;
using static OtherExtension.RandomTools;

public partial class CannonCross : UnusualCrossNode
{
    public Node2D Sprites, Ball;
    public PathFollow2D CannonPathFollow2D;
    public Sprite2D FrontWheel, BackWheel, Barrel, BallSprite;
    public AudioStreamPlayer ChargeSound;
    public CollisionShape2D BallCollision;
    private float _ballYMotion = 0.5f, _appearedCoeff = 0, _afterShotCoeff = 0;

    private Rect2 _ballBounds = new Rect2(
        G.CameraLimits.Position + new Vector2(-256, -256),
        G.CameraLimits.Size + new Vector2(256, 512));

    private bool _didCannonShot, _isLevelTooWide = G.CameraLimits.Size.X / G.CameraLimits.Size.Y > 10;
    public override void _Ready()
    {
        // Initializing nodes
        #region
        Sprites = GetNode<Node2D>("PathFollow2D/Cannon/Sprites");
        Ball = GetNode<Node2D>("PathFollow2D/Cannon/Ball");
        BallSprite = GetNode<Sprite2D>("PathFollow2D/Cannon/Ball/Ball");
        BallCollision = GetNode<CollisionShape2D>("PathFollow2D/Cannon/Ball/Hitbox/CollisionShape2D");
        CannonPathFollow2D = GetNode<PathFollow2D>("PathFollow2D");
        FrontWheel = GetNode<Sprite2D>("PathFollow2D/Cannon/Sprites/FrontWheel");
        BackWheel = GetNode<Sprite2D>("PathFollow2D/Cannon/Sprites/BackWheel");
        Barrel = GetNode<Sprite2D>("PathFollow2D/Cannon/Sprites/Barrel");
        ChargeSound = GetNode<AudioStreamPlayer>("PathFollow2D/Cannon/Sounds/Charge");
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
            CannonPathFollow2D.ProgressRatio = LogCoeff;
            BackWheel.Rotation = -LogCoeff * 3;
            FrontWheel.Rotation = -LogCoeff * 3;
            Barrel.RotationDegrees = -120 + LogCoeff * 60;

            if (_appearedCoeff < 0.0f) return;
        }



        if (!_didCannonShot)
        {
            // Animation of cannon charging
            if (!ChargeSound.Playing)
                ChargeSound.Play();
            Barrel.Scale = new Vector2(Barrel.Scale.X + 0.0025f, Barrel.Scale.Y - 0.005f);
            Barrel.Modulate = new Color(Barrel.Modulate.R, Barrel.Modulate.G - 0.0085f, Barrel.Modulate.B - 0.0085f);
            if (Barrel.Scale.Y <= 0.6f)
            {
                GetNode<AudioStreamPlayer>("PathFollow2D/Cannon/Sounds/Shot").Play();
                GetNode<CpuParticles2D>("PathFollow2D/Cannon/ExplosionParticles").Emitting = true;
                GetNode<CpuParticles2D>("PathFollow2D/Cannon/Ball/CPUParticles2D").Emitting = true;
                BallCollision.SetDeferred("disabled", false);
                Barrel.Texture = GD.Load<CompressedTexture2D>("res://Content/Sprites/Crosses/TornCannonBarrel.png");
                Ball.Visible = true;
                _didCannonShot = true;

                foreach (var group in GetGroups())
                    RemoveFromGroup(group);
            }
        }
        else
        {
            // Ball flight
            if (!_isLevelTooWide)
                Ball.Translate(new Vector2(-1.75f, _ballYMotion));
            else
                Ball.GlobalTranslate(new Vector2(0, 7.5f * (RotationDegrees == 90 ? 1 : -1)));

            BallSprite.Rotation -= 0.06f;
            _ballYMotion -= 0.006f;

            // Cannon animation
            if (_afterShotCoeff <= 0.98)
            {
                _afterShotCoeff += 0.15f;
                float LogCoeff = Mathf.Lerp(0.0f, 0.6f, 1 - (1 - _afterShotCoeff) * (1 - _afterShotCoeff));
                Barrel.Scale = new Vector2(1, 0.4f + LogCoeff);
            }

            if (Sprites.Visible == true && Sprites.Position.X < 50)
            {
                Sprites.Position = new Vector2(Sprites.Position.X + 2f, Sprites.Position.Y);
                Sprites.Rotation -= 0.1f;
            }
            else if (Sprites.Visible == true)
                Sprites.Visible = false;

            if (IsBallOutOfBounds())
            {
                OnFinished();
            }
        }
    }

    public bool IsBallOutOfBounds()
    {
        return !_ballBounds.HasPoint(Ball.GlobalPosition);
    }

    public override void Respawn()
    {
        base.Respawn();

        _isLevelTooWide = G.CameraLimits.Size.X / G.CameraLimits.Size.Y > 10;

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
        // All values below are the default values of the cannon when spawned.
        Sprites.Position = Vector2.Zero;
        Sprites.Rotation = 0;
        Sprites.Visible = true;

        CannonPathFollow2D.Progress = 0;

        Barrel.RotationDegrees = -120;
        Barrel.Modulate = new Color(1, 1, 1);

        Ball.GlobalPosition = new Vector2(-5, 3);
        Ball.Visible = false;
        BallCollision.SetDeferred("disabled", true);

        _ballBounds = new Rect2(
        G.CameraLimits.Position + new Vector2(-256, -256),
        G.CameraLimits.Size + new Vector2(256, 512));
    }
}
