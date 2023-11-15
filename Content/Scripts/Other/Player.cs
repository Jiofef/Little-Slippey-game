using Godot;
using System;

public partial class Player : CharacterBody2D
{
    [Export] float _speed = 400, _gravity = 18.6f, _jumpForce = 600;

    [Signal] public delegate void CameraLimitsChangedEventHandler();
    [Signal] public delegate void PlayerDiedEventHandler();

    private float _inertion, _wallJumpTimer = 0, //WallJumping
          _climbTimer, _climbUncontrollingTimer, //Climbing
          _moveCalculationTimer = 0; //Other
    private int _wallDetectNumber, _nearWallsCount, _savedWallNumber, //WallJumping
        _climbBufer = 3, _savedClimbWallNumber; //Climbing

    readonly float _floatDelta = 0.016667f;

    // other variable
    private bool _isFliph;

    private string _animationName;

    private sbyte _moveCalculationStartTimer, _lastXMoveVector;

    private enum State {Default, DownDash, Inerted}
    State _state = State.Default;

    AnimatedSprite2D _animatedSprite;

    Vector2 _motion = new Vector2();
    Vector2[] _savedPastPositions = new Vector2[11];

    Vector2 _corpseMotion;

    private void PlaySound(string SoundName)
    {
        GetNode<AudioStreamPlayer>("Sounds/" + SoundName).Play();
    }

    public override void _Ready()
    {
        GetNode("SkinContainer/Sprite2D").QueueFree();
        string[] SkinNames = {"Slippey", "Sanboy", "Strawman", "Pineplum", "Bondey", "Sleepy", "Hostey"};
        _animatedSprite = (AnimatedSprite2D)ResourceLoader.Load<PackedScene>("res://Content/Scenes/PlayerSkins/" + SkinNames[Meta.Instance.ChosenSkinIndex] + ".tscn").Instantiate();
        _animatedSprite.Connect("animation_finished", new Callable(this, "AnimationFinished"));
        GetNode("SkinContainer").AddChild(_animatedSprite);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (G.IsPlayerDead)
        {
            if (G.PlayerCorpseFlightTimer != 4.5f)
            {
                G.PlayerCorpseFlightTimer = G.PlayerCorpseFlightTimer < 4.5f ? G.PlayerCorpseFlightTimer + 0.016667f : 4.5f;
                Position = new Vector2(Position.X + _corpseMotion.X * G.GetReversedPlayerCorpseFlightTimerCoeff(), Position.Y + _corpseMotion.Y * G.GetReversedPlayerCorpseFlightTimerCoeff());
                Rotation += _corpseMotion.X / 50 * G.GetReversedPlayerCorpseFlightTimerCoeff();
                _corpseMotion.Y += _gravity / 200;
            }
            else
                G.AfterPlayerCorpseFlightTimer += 0.016667f;

            return;
        }

        //Player control and physic consequence
        {
            _motion = Velocity;
            if (_motion.Y < 1250)
                _motion.Y += _gravity;

            if (_state != State.DownDash)
            {
                _motion.X += Input.GetActionStrength("ui_right") - Input.GetActionStrength("ui_left");
                _motion.X *= _speed;

                if (_state == State.Inerted)
                {
                    float uncontrolling = _motion.X * _inertion / 2.5f;
                    _motion.X += _inertion > 0 ? -uncontrolling : uncontrolling;
                }
            }

            if (_wallDetectNumber != 0 && Input.IsActionPressed("WallCatch") && !IsOnFloor() && _motion.Y > 0 && _state != State.DownDash)
                if (!Input.IsActionPressed("Jump") || _inertion != 0)
                {
                    _motion.Y = 15;
                    _animationName = "WallCatch";
                }

            if (_climbBufer < 3 && _climbUncontrollingTimer > 0)
                _motion.X /= _motion.X / _savedClimbWallNumber < 0 ? _climbUncontrollingTimer * 3 + 1 : 1;

            if (Input.IsActionPressed("Jump"))
            {
                if (IsOnFloor())
                {
                    _motion.Y = -_jumpForce;
                    PlaySound("Jump");
                }
                else if (Input.IsActionPressed("WallCatch") && _wallDetectNumber != 0 && _inertion == 0 && _wallJumpTimer < 0 && _climbTimer < 0)
                {
                    _savedWallNumber = _wallDetectNumber;
                    _motion.Y = -_jumpForce;
                    _inertion = 1.25f * -_savedWallNumber;
                    _isFliph = _savedWallNumber == 1;
                    _state = State.Inerted;
                    PlaySound("Climb");
                }
                else if (!Input.IsActionPressed("WallCatch") && _lastXMoveVector == _wallDetectNumber && _wallDetectNumber != 0 && _climbTimer < 0 && _climbBufer > 0)
                {
                    _climbTimer = 0.2f;
                    _climbBufer--;
                    _motion.Y = -_jumpForce * 0.7f;
                    _savedClimbWallNumber = _wallDetectNumber;
                    _climbUncontrollingTimer = 1;
                    _animatedSprite.Frame = 0;
                    _animationName = "Climb";
                    PlaySound("Climb");
                }
            }
            else if (Input.IsActionJustPressed("DownPull") && !IsOnFloor() && _state != State.DownDash)
            {
                _state = State.DownDash;
                PlaySound("PullDown");
            }

            if (!IsOnFloor())
            {
                if (_state == State.Inerted)
                {
                    _motion.X += _inertion * _speed;
                    _inertion -= _inertion > 0 ? 0.025f : -0.025f;

                    if (_inertion < 0.1f && _inertion > 0 || _inertion > -0.1f && _inertion < 0)
                        _inertion = 0;
                }

                if (_state == State.DownDash)
                {
                    _motion.X = 0;
                    _motion.Y = 1250;
                }
                _wallJumpTimer -= _floatDelta;
                _climbTimer -= _floatDelta;
                _climbUncontrollingTimer -= _floatDelta;
            }
            else
            {
                _savedWallNumber = 0;
                _inertion = 0;
                _wallJumpTimer = 0.3f;
                _climbTimer = 0.2f;
                _climbBufer = 3;
                _climbUncontrollingTimer = 0;

                if (_state == State.DownDash)
                    PlaySound("PullDownHit");
                _state = State.Default;
            }

            if (_motion.X > 0)
                _lastXMoveVector = 1;
            else if (_motion.X < 0)
                _lastXMoveVector = -1;
        }

        //Animations
        {
            if (IsOnFloor() && _motion.X == 0)
                _animationName = "Idle";
            else if (_motion.X != 0 && IsOnFloor())
                _animationName = "Walk";
            else if (_state == State.DownDash)
                _animationName = "Fall";
            else if (_animationName != "WallCatch" && _animationName != "Climb" || _animationName == "WallCatch" && _wallDetectNumber == 0)
                _animationName = _motion.Y < 0 ? "Jump" : "Fall";

            if (_animatedSprite.Animation != _animationName)
                _animatedSprite.Animation = _animationName;

            if (_animationName == "Climb")
                _isFliph = _savedClimbWallNumber == 1;
            else if (_animationName == "WallCatch")
                _isFliph = _wallDetectNumber == 1;
            else if (_motion.X != 0)
                _isFliph = _motion.X > 0;

            _animatedSprite.FlipH = _isFliph;
        }

        //Physics injection
        {
            _moveCalculationTimer += _floatDelta;
            if (_moveCalculationTimer > 0.033f)
            {
                _moveCalculationTimer = 0;
                _savedPastPositions[_savedPastPositions.Length - 1] = Position;
                for (int i = 0; i < _savedPastPositions.Length - 1; i++)
                    _savedPastPositions[i] = _savedPastPositions[i + 1];
                float MoveDist = Position.DistanceTo(_savedPastPositions[0]);
                if (_moveCalculationStartTimer > 10)
                    G.PlayerMoveCoeff = MoveDist < 200 ? MoveDist / 200 : 1;
                else _moveCalculationStartTimer++;
            }

            Velocity = _motion;
            MoveAndSlide();
            Velocity = new Vector2(0, Velocity.Y);
        }

        //BOO
        {
            var ghost = GetNode<Sprite2D>("Ghost");
            if (ghost.Visible)
                ghost.GlobalPosition = new Vector2(_savedPastPositions[0].X, _savedPastPositions[0].Y);
        }
    }

    public void AnimationFinished()
    {
        if (_animationName == "Climb")
        {
            _animationName = "Jump";
            _animatedSprite.Play();
        }
    }

    public void Death()
    {
        ZIndex++;

        Random random = new Random();
        _corpseMotion.X = random.Next(100) > 50 ? -5 * (GlobalPosition.X / G.LevelXYSizes[G.CurrentLevel].X) : 5 * (1 - GlobalPosition.X / G.LevelXYSizes[G.CurrentLevel].X);
        if (G.CurrentLevel == 8 || GlobalPosition > G.LevelXYSizes[G.CurrentLevel] || GlobalPosition < Vector2.Zero)
            _corpseMotion.X = random.Next(100) > 50 ? -5 : +5;
        if (G.CurrentLevel == 1 && G.LevelAdditionalLink == "Tutorial")
            _corpseMotion.X *= 0.25f;
        _corpseMotion.Y = -8;

        Connect("PlayerDied", new Callable(GetParent(), "DisablePhysicsProcess"));
        EmitSignal("PlayerDied");

        PlaySound("Death");
        GetNode<AudioStreamPlayer>("../../../LevelMusicPlayer").StreamPaused = true;
        GetNode<CollisionShape2D>("FullBodyCollider").SetDeferred("disabled", true);
        _animatedSprite.Animation = "Death";
        G.IsPlayerDead = true;
        UnchangableMeta.SaveRecords();
        UnchangableMeta.SaveToFile();
    }

    public void LeftWallDetect()
    {
        _wallDetectNumber = -1;
        _nearWallsCount++;
    }

    public void RightWallDetect()
    {
        _wallDetectNumber = 1;
        _nearWallsCount++;
    }

    public void WallUndetected()
    {
        _nearWallsCount--;
        if (_nearWallsCount <= 0)
            _wallDetectNumber = 0;
    }

    public void SetCameraLimits(Vector4 value, bool DoResetSmoothing = false)
    {
        G.CameraLimits = value;
        EmitSignal("CameraLimitsChanged", DoResetSmoothing, 0, 0, 0, 0);
    }

    public void SetCameraPositionSmoothingSpeed (float value)
    {
        GetNode<Camera2D>("Camera2D").PositionSmoothingSpeed = value;
    }
}