using Godot;
using System;

public partial class Player : CharacterBody2D
{
    [ExportGroup("Main settings")]
    [Export] public float Speed = 430, Gravity = 18.6f, JumpForce = 620;
    [Export] public bool EnableRigidBodyPhysics = false;
    [Export] public float RigidBodyPushForce = 8;

    [ExportGroup("Secondary settings")]
    [Export] public float CoyoteTime = 0.2f, WallJumpInertion = 1.4f, InertionControl = 3.3f;

    [Signal] public delegate void CameraLimitsChangedEventHandler();
    [Signal] public delegate void PlayerDiedEventHandler();

    private float _inertion, _wallJumpTimer = 0, //WallJumping
          _climbTimer, _climbUncontrollingTimer, //Climbing
          _coyoteTimer, //Jumping
          _moveCalculationFramesTimer = 0; //Other
    private int _wallDetectNumber, _nearWallsCount, _savedWallNumber, //WallJumping
        _climbBufer = 3, _savedClimbWallNumber; //Climbing

    readonly float _floatDelta = 0.016667f;

    // other variable
    private bool _isFliph, _isDownDashing, _skinAnimationPlayerEnabled, _readyAlready;

    private string _animationName;

    private sbyte _moveCalculationStartTimer, _lastXMoveVector;

    private enum State {OnFloor, InAir, Climb, Inerted}
    State _state = State.OnFloor;

    AnimatedSprite2D _animatedSprite;
    AnimationPlayer _animationPlayer = null;

    public Vector2 Motion = new Vector2();
    Vector2[] _savedPastPositions = new Vector2[11];

    Vector2 _corpseMotion, _corpseMotionMultiplier = new Vector2(1, 1);

    private void PlaySound(string SoundName)
    {
        GetNode<AudioStreamPlayer>("Sounds/" + SoundName).Play();
    }

    public override void _Ready()
    {
        G.Player = this;
        TreeExited += () =>
        {
            if (G.Player == this)
                G.Player = null;
        };

        if (_readyAlready) return;

        #region Skin setting
        GetNode("SkinContainer/Default")?.QueueFree();

        if (!Meta.Instance.Gameplay.IsSkinModded)
            _animatedSprite = (AnimatedSprite2D)ResourceLoader.Load<PackedScene>("res://Content/Scenes/PlayerSkins/" + G.VanillaSkinNames[Meta.Instance.Gameplay.ChosenSkinIndex] + ".tscn").Instantiate();
        else;

        _animatedSprite.Connect("animation_finished", new Callable(this, "AnimationFinished"));
        if (_animatedSprite is SkinScript)
        {
            var skin = (SkinScript)_animatedSprite;
            if (skin.HasAnimationAnalogues)
            {
                _skinAnimationPlayerEnabled = true;
                _animationPlayer = _animatedSprite.GetNode<AnimationPlayer>("AnimationPlayer");
            }
        }

        GetNode("SkinContainer").AddChild(_animatedSprite);
        #endregion

        var cameraCallable = new Callable(GetNode("Camera2D"), "LimitsChangingBy");
        if (!IsConnected("CameraLimitsChanged", cameraCallable))
            Connect("CameraLimitsChanged", cameraCallable);

        _readyAlready = true;
    }



    public override void _PhysicsProcess(double delta)
    {
        if (G.IsPlayerDead) //smertb
        {
            if (G.PlayerCorpseFlightTimer != 4.5f)
            {
                G.PlayerCorpseFlightTimer = G.PlayerCorpseFlightTimer < 4.5f ? G.PlayerCorpseFlightTimer + 0.016667f : 4.5f;
                if (Meta.Instance.Gameplay.ChosenSkinIndex == 11) return;
                Position += (_corpseMotion * G.GetReversedPlayerCorpseFlightTimerCoeff() * _corpseMotionMultiplier);
                Rotation += _corpseMotion.X / 50 * G.GetReversedPlayerCorpseFlightTimerCoeff();
                _corpseMotion.Y += Gravity / 200;
            }
            else
                G.AfterPlayerCorpseFlightTimer += 0.016667f;

            return;
        }
        bool isOnFloor = IsOnFloor();
        GD.Print(_state + " " + _inertion);
        //Player control and physic consequence
        {

            Motion = Velocity;
            if (Motion.Y < 1250)
                Motion.Y += Gravity;

            Motion.X += Input.GetActionStrength("ui_right") - Input.GetActionStrength("ui_left");
            Motion.X *= Speed;

            if (_state == State.Inerted)
            {
                float uncontrolling = Motion.X * _inertion / InertionControl;
                Motion.X += _inertion > 0 ? -uncontrolling : uncontrolling;
            }
            if (_wallDetectNumber != 0 && Input.IsActionPressed("WallCatch") && !isOnFloor && Motion.Y > 0 && (!Input.IsActionPressed("Jump") || _inertion != 0))
            {
                Motion.Y = 15;
                _animationName = "WallCatch";
                if (_inertion == 0 && _state != State.InAir)
                    _state = State.InAir;
                _isDownDashing = false;
            }

            if (_climbBufer < 3 && _climbUncontrollingTimer > 0)
                Motion.X /= Motion.X / _savedClimbWallNumber < 0 ? _climbUncontrollingTimer * 3 + 1 : 1;

            if (Input.IsActionPressed("Jump"))
            {
                if (isOnFloor) // This is jump
                {
                    Motion.Y = -JumpForce;
                    PlaySound("Jump");
                }
                else if (Input.IsActionPressed("WallCatch") && _wallDetectNumber != 0 && _inertion == 0 && _wallJumpTimer < 0 && _climbTimer < 0) // This is WallJump
                {
                    _savedWallNumber = _wallDetectNumber;
                    Motion.Y = -JumpForce;
                    _inertion = WallJumpInertion * -_savedWallNumber;
                    _state = State.Inerted;
                    PlaySound("Climb");
                    _isDownDashing = false;
                }
                else if (!Input.IsActionPressed("WallCatch") && _lastXMoveVector == _wallDetectNumber && _wallDetectNumber != 0 && _climbTimer < 0 && _climbBufer > 0) // This is Climb
                {
                    if (_skinAnimationPlayerEnabled)
                        _animationPlayer.Play("Climb");
                    _climbTimer = 0.2f;
                    _climbBufer--;
                    Motion.Y = -JumpForce * 0.7f;
                    _savedClimbWallNumber = _wallDetectNumber;
                    _climbUncontrollingTimer = 1;
                    _animatedSprite.Frame = 0;
                    _animationName = "Climb";
                    _state = State.Climb;
                    PlaySound("Climb");
                    _isDownDashing = false;
                    _inertion = 0;
                }
            }
            else if (Input.IsActionJustPressed("DownDash") && !isOnFloor && !_isDownDashing) // This is DownDash
            {
                _isDownDashing = true;
                Motion.Y = 1250;
                PlaySound("DownDash");
            }
            else if (Input.IsActionJustReleased("Jump") && _state != State.Climb) // This is jump interruption
            {
                if (Motion.Y < 0)
                    Motion.Y /= 1.5f;
            }

            if (!isOnFloor)
            {
                if (_state == State.Inerted)
                {
                    Motion.X += _inertion * Speed;
                    _inertion -= _inertion > 0 ? 0.025f : -0.025f;

                    if (_inertion < 0.1f && _inertion > 0 || _inertion > -0.1f && _inertion < 0)
                        _inertion = 0;
                }

                if (_state != State.Inerted && _state != State.Climb && _state != State.InAir)
                    _state = State.InAir;

                _wallJumpTimer -= _floatDelta;
                _climbTimer -= _floatDelta;
                _climbUncontrollingTimer -= _floatDelta;
            }
            else if (_state != State.OnFloor)
            {
                _savedWallNumber = 0;
                _inertion = 0;
                _wallJumpTimer = 0.3f;
                _climbTimer = 0.2f;
                _climbBufer = 3;
                _climbUncontrollingTimer = 0;
                _coyoteTimer = CoyoteTime;

                if (_isDownDashing)
                {
                    _isDownDashing = false;
                    PlaySound("DownDashHit");
                }
                _state = State.OnFloor;
            }

            if (Motion.X > 0)
                _lastXMoveVector = 1;
            else if (Motion.X < 0)
                _lastXMoveVector = -1;
        }

        //Animations
        {
            if (isOnFloor && Motion.X == 0)
                _animationName = "Idle";
            else if (Motion.X != 0 && isOnFloor)
                _animationName = "Walk";
            else if (_animationName != "WallCatch" && _animationName != "Climb" || _animationName == "WallCatch" && _wallDetectNumber == 0)
                _animationName = Motion.Y < 0 ? "Jump" : "Fall";

            if (_animatedSprite.Animation != _animationName)
            {
                _animatedSprite.Animation = _animationName;
                if (_skinAnimationPlayerEnabled)
                    _animationPlayer.Play(_animationName);
            }

            if (_animationName == "Climb")
                _isFliph = _savedClimbWallNumber == 1;
            else if (_animationName == "WallCatch")
                _isFliph = _wallDetectNumber == 1;
            else if (Motion.X != 0)
                _isFliph = Motion.X > 0;

            GetNode<Node2D>("SkinContainer").Scale = new Vector2(_isFliph ? -1 : 1, 1);
        }

        //Physics injection
        {
            _moveCalculationFramesTimer ++;
            if (_moveCalculationFramesTimer > 3)
            {
                _moveCalculationFramesTimer = 0;
                _savedPastPositions[_savedPastPositions.Length - 1] = Position;
                for (int i = 0; i < _savedPastPositions.Length - 1; i++)
                    _savedPastPositions[i] = _savedPastPositions[i + 1];
                float MoveDist = Position.DistanceTo(_savedPastPositions[0]);
                if (_moveCalculationStartTimer > 10)
                    G.PlayerMoveCoeff = MoveDist < 200 ? MoveDist / 200 : 1;
                else _moveCalculationStartTimer++;

                //BOO
                var ghost = GetNode<Sprite2D>("Ghost");
                if (ghost.Visible)
                    ghost.GlobalPosition = new Vector2(_savedPastPositions[0].X, _savedPastPositions[0].Y);
            }

            Velocity = Motion;
            MoveAndSlide();
            Velocity = new Vector2(0, Velocity.Y);

            if (EnableRigidBodyPhysics)
            {
                for (int i = 0; i < GetSlideCollisionCount(); i++)
                {
                    var collision = GetSlideCollision(i);
                    if (collision.GetCollider() is RigidBody2D)
                        ((RigidBody2D)collision.GetCollider()).ApplyCentralImpulse(-collision.GetNormal() * RigidBodyPushForce);
                }
            }

        }
    }

    private void AnimationFinished()
    {
        if (_animationName == "Climb")
        {
            _animationName = "Jump";
            _state = State.InAir;
            _animatedSprite.Play();
        }
    }

    private void LeftWallDetect()
    {
        _wallDetectNumber = -1;
        _nearWallsCount++;
    }

    private void RightWallDetect()
    {
        _wallDetectNumber = 1;
        _nearWallsCount++;
    }

    private void WallUndetected()
    {
        _nearWallsCount--;
        if (_nearWallsCount <= 0)
            _wallDetectNumber = 0;
    }

    //Above - use with caution


    //Below - use freely 

    public void Death()
    {
        UnchangableMeta.DeathsNumber++;

        ZIndex++;

        Random random = new Random();
        _corpseMotion.X = random.Next(100) > 50 ? -5 * (GlobalPosition.X / G.LevelXYSizes[G.CurrentLevel].X) : 5 * (1 - GlobalPosition.X / G.LevelXYSizes[G.CurrentLevel].X);
        _corpseMotion.Y = -8;
        if (G.LevelXYSizes[G.CurrentLevel].X > 12800 || G.LevelXYSizes[G.CurrentLevel].Y > 12800 || GlobalPosition > G.LevelXYSizes[G.CurrentLevel] || GlobalPosition < Vector2.Zero)
            _corpseMotion.X = random.Next(100) > 50 ? -5 : +5;

        G.IsCrossesEnabled = false;
        G.IsProgressPaused = true;

        EmitSignal("PlayerDied");
        PlaySound("Death");
        GetNode<AudioStreamPlayer>("../../LevelMusicPlayer").StreamPaused = true;
        GetNode<CollisionShape2D>("FullBodyCollider").SetDeferred("disabled", true);
        _animatedSprite.Animation = "Death";
        if (Convert.ToBoolean((string)_animatedSprite.GetMeta("HasDeathPlayerAnimation")))
            _animatedSprite.GetNode<AnimationPlayer>("AnimationPlayer").Play("Death");
        G.IsPlayerDead = true;

        if (G.IsLevelVanilla)
        {
            switch (UnchangableMeta.DeathsNumber)
            {
                case 2: Achievements.GetAchievement(4); break;
                case 35: Achievements.GetAchievement(5); break;
                case 273: Achievements.GetAchievement(6); break;
            }
            UnchangableMeta.SaveRecords();
            UnchangableMeta.SaveToFile();
        }
    }

    public void Resurrect()
    {

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



    // Use that two if you need to limit the character's flight after death (or increase it, god knows what you're doing).
    public void SetCorpseMotionMultiplierX(float value)
    {
        _corpseMotionMultiplier.X = value;
    }
    public void SetCorpseMotionMultiplierY(float value)
    {
        _corpseMotionMultiplier.Y = value;
    }
}