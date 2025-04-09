using Godot;
using Godot.Collections;
using System;
using System.Threading.Tasks;
using static OtherExtension.FastInstanceCreator;

public partial class Player : CharacterBody2D
{

    ///////////////////////////

    //InEditor options
    [ExportGroup("Main settings")]
    public const float DEFAULT_SPEED = 430, DEFAULT_GRAVITY = 9.8f * 2f, DEFAULT_JUMP_FORCE = 620;
    [Export] public float Speed = DEFAULT_SPEED, Gravity = DEFAULT_GRAVITY, JumpForce = DEFAULT_JUMP_FORCE;

    public const int DEFAULT_MAX_CLIMBS = 3;
    private int _maxClimbs = DEFAULT_MAX_CLIMBS;
    [Export] public int MaxClimbs { get { return _maxClimbs; } set { _maxClimbs = value; GetNode<TextureProgressBar>("Camera2D/ClimbsBar").MaxValue = value; } }
    private bool _enableStandingPenalty = true;
    [Export]
    public bool EnableStandingPenalty
    {
        get => _enableStandingPenalty;

        set
        {
            _enableStandingPenalty = value;
            GetGui().Options.DisableStandBar = !value;
            GetGui().CallDeferred("UpdateStandingBarOptions");
        }
    }

    [Export] public bool EnableRigidBodyPhysics = false;
    [Export] public float RigidBodyPushForce = 8;

    [ExportGroup("Secondary settings")]
    [Export] public float CoyoteTime = 0.1f, WallJumpInertion = 1.4f, InertionControl = 3.3f, DownDashSpeed = 1250, MaxFallSpeed = 1250, ResurrectionImmortalityTime = 5f, MinMoveCoeff = 0, MaxMoveCoeff = 1;

    [ExportGroup("GUI")]
    #region hell no
    [ExportSubgroup("Main GUI")]
    [Export]
    public bool DisableScoresLabel
    {
        get => GetGui().Options.DisableScoresLabel;
        set => CallDeferred("SetDisableScoresLabel", value);
    }
    private void SetDisableScoresLabel(bool value)
    {
        GetGui().Options.DisableScoresLabel = value;
    }
    [ExportSubgroup("After death GUI")]
    [Export]
    public bool DisableAfterDeathGui
    {
        get => GetGui().Options.DisableAfterDeathGui;
        set => CallDeferred("SetDisableAfterDeathGui", value);
    }
    private void SetDisableAfterDeathGui(bool value)
    {
        GetGui().Options.DisableAfterDeathGui = value;
    }
    [Export]
    public bool DisableAfterDeathScoresLabel
    {
        get => GetGui().Options.DisableAfterDeathScoresLabel;
        set => CallDeferred("SetDisableAfterDeathScoresLabel", value);
    }
    private void SetDisableAfterDeathScoresLabel(bool value)
    {
        GetGui().Options.DisableAfterDeathScoresLabel = value;
    }
    [Export]
    public bool DisableNewRecordLabel
    {
        get => GetGui().Options.DisableNewRecordLabel;
        set => CallDeferred("SetDisableNewRecordLabel", value);
    }
    private void SetDisableNewRecordLabel(bool value)
    {
        GetGui().Options.DisableNewRecordLabel = value;
    }
    [Export]
    public bool DisableHoldRText
    {
        get => GetGui().Options.DisableHoldRText;
        set => CallDeferred("SetDisableHoldRText", value);
    }
    private void SetDisableHoldRText(bool value)
    {
        GetGui().Options.DisableHoldRText = value;
    }
    [Export]
    public bool DisableReturnToMenuButton
    {
        get => GetGui().Options.DisableReturnToMenuButton;
        set => CallDeferred("SetDisableReturnToMenuButton", value);
    }
    private void SetDisableReturnToMenuButton(bool value)
    {
        GetGui().Options.DisableReturnToMenuButton = value;
    }
    [Export]
    public bool DisableResurrectButton
    {
        get => GetGui().Options.DisableResurrectButton;
        set => CallDeferred("SetDisableResurrectButton", value);
    }
    private void SetDisableResurrectButton(bool value)
    {
        GetGui().Options.DisableResurrectButton = value;
    }
    [Export]
    public bool ShowGoldenCrossesAmountAfterDeath = true;

    public void UpdateGUIOptions()
    {
        GetGui().UpdateAllTheOptions();
    }
    #endregion

    // The values in the camera used to be changed via this signal. Now it's just an auxiliary signal for modders.
    [Signal] public delegate void CameraLimitsChangedEventHandler(bool doResetSmoothing, Rect2 limits);

    [Signal] public delegate void PlayerDiedEventHandler();
    [Signal] public delegate void PlayerResurrectedEventHandler();

    ///////////////////////////

    // For the future if we have to access some Player nodes, and their path will be changed.
    public const string CAMERA_PATH = "Camera2D", SCORES_PATH = "Camera2D/GUICanvas/GUI/Scores", GUI_CONTROL_PATH = "Camera2D/GUICanvas/GUI", GUI_CANVAS_PATH = "Camera2D/GUICanvas", AFTER_DEATH_GUI_PATH = "Camera2D/GUICanvas/GUI/EmergingElements";


    // Numeric variables
    private float _inertion, _wallJumpTimer = 0, //WallJumping
          _climbTimer, _climbUncontrollingTimer, //Climbing
          _coyoteTimer, //Jumping
          _moveCoeff = 1, // Hardcore =)
          _moveCalculationFramesTimer = 0; //Other
    private int _wallDetectNumber, _nearWallsCount, _savedWallNumber, //WallJumping
        _climbBufer = 3, _savedClimbWallNumber; //Climbing

    readonly float _floatDelta = 0.016667f; // 1/60 fps

    private sbyte _lastXMoveVector; //The direction of movement in the previous frame

    ///////////////////////////

    // Other variables
    private bool _isFliph, _isDownDashing, _skinAnimationPlayerEnabled, _readyAlready;

    private string _animationName;

    private enum State { OnFloor, InAir, Climb, Inerted }
    State _state = State.OnFloor;

    AnimatedSprite2D _animatedSprite;
    AnimationPlayer _animationPlayer = null;

    public Vector2 Motion = new Vector2(); // Comfortable layer for velocity

    private Vector2[] _savedPastPositions = new Vector2[30];
    private Vector2 _averagePosition; // The arithmetic mean of the upper array

    private Vector2 _corpseMotion, _corpseMotionMultiplier = new Vector2(1, 1);

    // Nodes
    public Camera Camera;
    public InGameGui GUI;

    // Player Actions
    public enum Act
    {
        Stand, Jump, Walk, Fall, WallCatch, Climb, WallJump, DownDash, // Basic actions
        HardJump, Tossed
    } // Secondary or secret actions
    [ExportGroup("Actions")]
    [Export]
    public Dictionary<Act, bool> DefaultActBlockedState = new Dictionary<Act, bool>()
    {
        { Act.Stand, false },
        { Act.Jump, false },
        { Act.Walk, false },
        { Act.Fall, false },
        { Act.WallCatch, false },
        { Act.Climb, false},
        { Act.WallJump, false },
        { Act.DownDash, false },
        { Act.HardJump, false },
    };
    public Dictionary<Act, bool> IsActBlocked;


    public Act[] LastActions { get; private set; } = new Act[10];
    /// <summary>
    /// Use to write an action
    /// </summary>
    private void Action(Act action)
    {
        for (int i = LastActions.Length - 1; i > 0; i--)
            LastActions[i] = LastActions[i - 1];
        LastActions[0] = action;
    }

    public async void BlockActFor(Act act, Task task)
    {
        if (IsActBlocked[act]) return;

        IsActBlocked[act] = true;

        await task;

        IsActBlocked[act] = DefaultActBlockedState[act];
    }
    public async void BlockActFor(Act act, float timeSec)
    {
        if (IsActBlocked[act]) return;

        IsActBlocked[act] = true;

        var timer = GetTree().CreateTimer(timeSec, false);
        await ToSignal(timer, "timeout");

        IsActBlocked[act] = DefaultActBlockedState[act];
    }

    public async void BlockActsFor(Act[] acts, float timeSec)
    {
        foreach (Act act in acts)
            IsActBlocked[act] = true;

        var timer = GetTree().CreateTimer(timeSec, false);
        await ToSignal(timer, "timeout");

        foreach (Act act in acts)
            IsActBlocked[act] = DefaultActBlockedState[act];
    }

    ///////////////////////////


    private void PlaySound(string SoundName)
    {
        GetNode<AudioStreamPlayer>("Sounds/" + SoundName).Play();
    }

    public override void _EnterTree()
    {
        G.Player = this;
    }
    public bool IsDisposed { get; private set; }
    public override void _Ready()
    {
        TreeExited += () =>
        {
            if (G.Player == this)
                G.Player = null;
            IsDisposed = true;
        };

        if (_readyAlready) return;

        IsActBlocked = DefaultActBlockedState.Duplicate();


        Camera = GetNode<Camera>("Camera2D");
        GUI = GetGui();

        UpdateSkin();

        ToggleStandingPenalty(EnableStandingPenalty);

        preDeathParams = new PreDeathParams(this);

        _readyAlready = true;

        Action(Act.Fall);

        SetDeferred("DisableScoresLabel", false);
    }
    public InGameGui GetGui()
    {
        return GetNode<InGameGui>("Camera2D/GUICanvas/GUI");
    }





    public override void _PhysicsProcess(double delta)
    {
        if (G.IsPlayerDead) //smertb
        {
            if (G.PlayerCorpseFlightTimer != 4.5f)
            {
                G.PlayerCorpseFlightTimer = G.PlayerCorpseFlightTimer < 4.5f ? G.PlayerCorpseFlightTimer + 0.016667f : 4.5f;
                if (Meta.Instance.Gameplay.ChosenSkinKey == "Chad") return;
                Position += (_corpseMotion * G.GetReversedPlayerCorpseFlightTimerCoeff() * _corpseMotionMultiplier);
                Rotation += _corpseMotion.X / 50 * G.GetReversedPlayerCorpseFlightTimerCoeff();
                _corpseMotion.Y += Gravity / 200;
            }
            else
                G.AfterPlayerCorpseFlightTimer += 0.016667f;


            return;
        }

        bool isOnFloor = IsOnFloor(); // To avoid calling the function every time

        #region Control and physics processing
        {
            Motion = Velocity;
            #region Gravitation
            if (Motion.Y < MaxFallSpeed) //Falling speed limitation
                Motion.Y += Gravity;
            #endregion

            #region Walking
            if (!IsActBlocked[Act.Walk])
                Motion.X += Input.GetActionStrength("ui_right") - Input.GetActionStrength("ui_left"); // Walking
            Motion.X *= Speed;
            if (LastActions[0] != Act.Walk && Motion.X != 0 && isOnFloor && !IsActBlocked[Act.Walk])
                Action(Act.Walk);
            else if (LastActions[0] != Act.Stand && Motion.X == 0 && isOnFloor && !IsActBlocked[Act.Stand])
                Action(Act.Stand);
            #endregion

            #region Jumping off one way platforms
            if (isOnFloor && Input.IsActionJustPressed("DownDash"))
                Position += new Vector2(0, 1);
            #endregion


            #region Effect of inertia on control
            if (_state == State.Inerted)
            {
                float uncontrolling = Motion.X * _inertion / InertionControl;
                Motion.X += _inertion > 0 ? -uncontrolling : uncontrolling;
            }
            #endregion

            #region WallCatching
            if (Input.IsActionPressed("WallCatch") && _wallDetectNumber != 0 && !isOnFloor && Motion.Y > 0 && (!Input.IsActionPressed("Jump") || _inertion != 0) && !IsActBlocked[Act.WallCatch])
            {
                if (LastActions[0] != Act.WallCatch)
                    Action(Act.WallCatch);
                Motion.Y = 15;
                _animationName = "WallCatch";
                if (_inertion == 0 && _state != State.InAir)
                {
                    _state = State.InAir;
                    _climbUncontrollingTimer = 0;
                    _climbBufer = MaxClimbs;
                    _climbTimer = 0;
                    _inertion = 0;
                }
                _isDownDashing = false;
            }
            #endregion

            #region Climb inertion
            if (_climbBufer < 3 && _climbUncontrollingTimer > 0)
                Motion.X /= Motion.X / _savedClimbWallNumber < 0 ? _climbUncontrollingTimer * 3 + 1 : 1;
            #endregion


            #region All the mechanics with a jump
            if (Input.IsActionPressed("Jump"))
            {
                #region Jumping
                if ((isOnFloor && _state == State.OnFloor || _coyoteTimer > 0) && !IsActBlocked[Act.Jump])
                {
                    Action(Act.Jump);

                    Motion.Y = -JumpForce;
                    _coyoteTimer = 0;

                    PlaySound("Jump");
                }
                #endregion
                #region WallJumping
                else if (Input.IsActionPressed("WallCatch") && _wallDetectNumber != 0 && _inertion == 0 && _wallJumpTimer <= 0 && _climbTimer <= 0 && !IsActBlocked[Act.WallJump])
                {
                    _savedWallNumber = _wallDetectNumber;
                    if (Motion.Y <= 0 && LastActions[0] == Act.WallJump && LastActions[1] == Act.Climb && !IsActBlocked[Act.HardJump]) // Secret mechanic
                    {
                        Action(Act.HardJump);
                        GetNode<CpuParticles2D>("HardJumpParticles").Emitting = true;
                        Motion.Y = -JumpForce * 1.25f;
                        _inertion = WallJumpInertion * -_savedWallNumber * 1.25f;
                    }
                    else
                    {
                        Action(Act.WallJump);
                        Motion.Y = -JumpForce;
                        _inertion = WallJumpInertion * -_savedWallNumber;
                    }


                    _state = State.Inerted;
                    PlaySound("Climb");
                    _isDownDashing = false;
                }
                #endregion
                #region  Climbing
                else if (!Input.IsActionPressed("WallCatch") && _wallDetectNumber != 0 && _lastXMoveVector == _wallDetectNumber && _climbTimer < 0 && _climbBufer > 0 && !IsActBlocked[Act.Climb])
                {
                    Action(Act.Climb);
                    if (_skinAnimationPlayerEnabled)
                        _animationPlayer.Play("Climb");

                    _climbTimer = 0.2f;
                    _climbBufer--;
                    Motion.Y = -JumpForce * 0.7f;
                    _climbUncontrollingTimer = 1;
                    _savedClimbWallNumber = _wallDetectNumber;


                    _animatedSprite.Frame = 0;
                    _animationName = "Climb";

                    _state = State.Climb;
                    PlaySound("Climb");

                    _isDownDashing = false;
                    _inertion = 0;

                    var climbBar = GetNode<TextureProgressBar>("Camera2D/ClimbsBar");
                    climbBar.Value = _climbBufer;

                    var climbBarAnimation = climbBar.GetNode<AnimationPlayer>("AnimationPlayer");
                    climbBarAnimation.Stop();
                    climbBarAnimation.Play("Disappearing");
                }
                #endregion
            }
            #region DownDashing
            const float DOWN_DASH_CEILING_THRESHOLD = 50f;
            if (Input.IsActionJustPressed("DownDash") && !isOnFloor && Motion.Y < (DownDashSpeed - DOWN_DASH_CEILING_THRESHOLD) && !IsActBlocked[Act.DownDash]) // This is DownDash
            {
                Action(Act.DownDash);
                _isDownDashing = true;
                Motion.Y = 1250;
                PlaySound("DownDash");
            }
            #endregion

            #region Jump interruption
            else if (Input.IsActionJustReleased("Jump") && _state != State.Climb) // This is jump interruption
            {
                if (Motion.Y < 0)
                    Motion.Y /= 1.5f;
            }
            #endregion

            #endregion


            #region Flight physics
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

                _coyoteTimer -= _floatDelta;
                _wallJumpTimer -= _floatDelta;
                _climbTimer -= _floatDelta;
                _climbUncontrollingTimer -= _floatDelta;
            }
            #endregion
            #region Fall processing
            else if (_state != State.OnFloor)
            {
                Action(Act.Fall);
                _savedWallNumber = 0;
                _inertion = 0;
                _wallJumpTimer = 0.3f;
                _climbTimer = 0.2f;
                _climbBufer = MaxClimbs;
                _coyoteTimer = CoyoteTime;

                if (_isDownDashing)
                {
                    _isDownDashing = false;
                    PlaySound("DownDashHit");
                }
                _state = State.OnFloor;
            }
            #endregion


            #region Saving the direction of walking
            if (Motion.X > 0)
                _lastXMoveVector = 1;
            else if (Motion.X < 0)
                _lastXMoveVector = -1;
            #endregion
        }
        #endregion

        #region Animation processing
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
        #endregion

        #region Physics injection
        {
            _moveCalculationFramesTimer++;
            if (_moveCalculationFramesTimer > 2)
            {
                _moveCalculationFramesTimer = 0;

                //Saving this frame position
                for (int i = 0; i < _savedPastPositions.Length - 1; i++)
                    _savedPastPositions[i] = _savedPastPositions[i + 1];
                _savedPastPositions[_savedPastPositions.Length - 1] = Position;


                // Calculating average position
                Vector2 PastPosSum = new Vector2();
                foreach (var position in _savedPastPositions)
                    PastPosSum += position;
                _averagePosition = PastPosSum / _savedPastPositions.Length;
            }


            if (_enableStandingPenalty)
            {
                Vector2 CorrectedAveragePosition = GlobalPosition - _averagePosition;
                CorrectedAveragePosition.Y *= 1.5f;
                float MoveDist = Mathf.Sqrt(CorrectedAveragePosition.X * CorrectedAveragePosition.X + CorrectedAveragePosition.Y * CorrectedAveragePosition.Y);
                _moveCoeff += (MoveDist < 300 ? -0.6f + MoveDist / 300 : 0.4f) / 60;
                _moveCoeff = Mathf.Clamp(_moveCoeff, MinMoveCoeff, MaxMoveCoeff);
            }

            G.PlayerMoveCoeff = _moveCoeff;

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
        #endregion
    }

    private void AnimationFinished()
    {
        #region When climbing animation ends
        if (_animationName == "Climb")
        {
            _animationName = "Jump";
            _state = State.InAir;

            if (_inertion != 0)
            {
                _inertion = 0;
                _climbUncontrollingTimer = 0;
            }
            _animatedSprite.Play();
        }
        #endregion

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

    public void UpdateSkin()
    {
        var skinContainer = GetNode<Node2D>("SkinContainer");
        foreach (var Child in skinContainer.GetChildren())
        {
            Child.QueueFree();
        }

        if (!Meta.Instance.Gameplay.IsSkinModded)
            _animatedSprite = (AnimatedSprite2D)ResourceLoader.Load<PackedScene>("res://Content/Scenes/PlayerSkins/" + Meta.Instance.Gameplay.ChosenSkinKey + ".tscn").Instantiate();
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

            if (skin.HasDeathAnimation)
            {
                _animationPlayer = _animatedSprite.GetNode<AnimationPlayer>("AnimationPlayer");

                if (!IsConnected("PlayerResurrected", new Callable(_animationPlayer, "stop")))
                {
                    Connect("PlayerResurrected", new Callable(_animationPlayer, "stop"));
                    PlayerResurrected += () => _animationPlayer.Play("RESET");
                }

            }
        }

        skinContainer.AddChild(_animatedSprite);
    }

    //Above - use with caution


    //Below - use freely 
    private class PreDeathParams
    {
        Player player;
        public bool HasSavedParams = false;
        public PreDeathParams(Player thisPlayer)
        {
            player = thisPlayer;
        }

        public int ZIndex;
        public float RotationDegrees;


        public bool IsCrossesEnabled;
        public bool IsProgressPaused;
        public bool IsMusicPaused;
        public bool IsCollisionDisabled;
        public bool IsDamageCollisionDisabled;

        public Vector2 Position;

        public void SaveParams()
        {
            ZIndex = player.ZIndex;
            RotationDegrees = player.RotationDegrees;


            IsCrossesEnabled = G.IsCrossesEnabled;
            IsProgressPaused = G.IsProgressPaused;
            if (G.MusicPlayer != null)
                IsMusicPaused = G.MusicPlayer.StreamPaused;

            var collision = player.GetNodeOrNull<CollisionShape2D>("FullBodyCollider");
            if (collision != null)
                IsCollisionDisabled = collision.Disabled;

            var damageCollision = player.GetNodeOrNull<CollisionShape2D>("Areas/PlayerDamageDetector/CollisionShape2D");
            if (damageCollision != null)
                IsDamageCollisionDisabled = damageCollision.Disabled;


            Position = player.Position;

            HasSavedParams = true;
        }

        public void LoadParams()
        {
            if (!HasSavedParams) return;

            player.ZIndex = ZIndex;
            player.Rotation = RotationDegrees;

            G.IsCrossesEnabled = IsCrossesEnabled;
            G.IsProgressPaused = IsProgressPaused;
            if (G.MusicPlayer != null)
                G.MusicPlayer.StreamPaused = IsMusicPaused;

            var collision = player.GetNode<CollisionShape2D>("FullBodyCollider");
            if (collision != null)
                collision.Disabled = IsCollisionDisabled;

            var damageCollision = player.GetNodeOrNull<CollisionShape2D>("Areas/PlayerDamageDetector/CollisionShape2D");
            if (damageCollision != null)
                damageCollision.Disabled = IsDamageCollisionDisabled;

            player.Position = Position;
        }
    }
    PreDeathParams preDeathParams;

    public void Death()
    {
        if (G.IsPlayerDead) return;

        Input.StartJoyVibration(0, 1f, 1f, 0.33f);

        preDeathParams.SaveParams();

        UnchangableMeta.DeathsNumber++;
        ZIndex++;

        const float CORPSE_MAX_X_SPEED = 5;
        float XPosCoeff = Mathf.Clamp(GlobalPosition.X / Camera.LimitRight, -1f, 1f);

        Random random = new Random();
        _corpseMotion.X = random.Next(100) > 50 ? -CORPSE_MAX_X_SPEED * XPosCoeff : CORPSE_MAX_X_SPEED * (1 - XPosCoeff);

        _corpseMotion.Y = -8;

        bool IsLevelTooWide = (Camera.LimitRight - Camera.LimitLeft) > 12800;

        if (IsLevelTooWide)
            _corpseMotion.X = random.Next(100) > 50 ? -CORPSE_MAX_X_SPEED : +CORPSE_MAX_X_SPEED;

        G.IsCrossesEnabled = false;
        G.IsProgressPaused = true;

        PlaySound("Death");
        G.MusicPlayer?.Set("stream_paused", true);
        GetNode<CollisionShape2D>("FullBodyCollider").SetDeferred("disabled", true);
        GetNode<CollisionShape2D>("Areas/PlayerDamageDetector/CollisionShape2D").SetDeferred("disabled", true);
        _animatedSprite.Animation = "Death";
        if (Convert.ToBoolean((string)_animatedSprite.GetMeta("HasDeathPlayerAnimation")))
            _animatedSprite.GetNode<AnimationPlayer>("AnimationPlayer").Play("Death");
        G.IsPlayerDead = true;

        switch (UnchangableMeta.DeathsNumber)
        {
            case 2: Achievements.GetAchievement("It's worth a shot"); break;
            case 35: Achievements.GetAchievement("I'm no stranger to"); break;
            case 273: Achievements.GetAchievement("Over and over and over and over and over and"); break;
        }
        UnchangableMeta.SaveRecords();
        UnchangableMeta.SaveToFile();

        Camera.OnPlayerDead();
        GUI.OnPlayerDead();

        EmitSignal("PlayerDied");
    }

    public async void Resurrect()
    {
        if (!G.IsPlayerDead || !preDeathParams.HasSavedParams) return;
        // >>Before resurrection

        // Resurrection effects
        var resurrectionEffect = LoadResScene("Interface&Menu/ResurrectionEffect.tscn");
        var resurrectionAnimation = resurrectionEffect.GetNode<AnimationPlayer>("AnimationPlayer");
        G.AdditionalGuiLayer.AddChild(resurrectionEffect);
        // The only time the animation changes is when the player must be resurrected
        await ToSignal(resurrectionAnimation, "animation_changed");

        // >>After resurrection

        preDeathParams.LoadParams();

        // Resurrection immortability
        bool dmgCollisionDisabled = GetNode<CollisionShape2D>("Areas/PlayerDamageDetector/CollisionShape2D").Disabled;
        if (!dmgCollisionDisabled)
            AddImmortality(ResurrectionImmortalityTime);


        G.IsPlayerDead = false;
        G.AfterPlayerCorpseFlightTimer = 0;
        G.PlayerCorpseFlightTimer = 0;

        GetNode<AudioStreamPlayer>("Sounds/Death").Stop();

        Camera.OnPlayerResurrected();
        GUI.OnPlayerResurrected();

        EmitSignal("PlayerResurrected");
    }

    private void ImmortalityIsOver()
    {
        var immortabilityShield = GetNode<TextureProgressBar>("Camera2D/ImmortalityShield");
        immortabilityShield.Visible = false;

        GetNode<CollisionShape2D>("Areas/PlayerDamageDetector/CollisionShape2D").SetDeferred("disabled", false);
    }

    public void AddImmortality(float time)
    {
        var immortabilityShield = GetNode<TextureProgressBar>("Camera2D/ImmortalityShield");
        var immortabilityTimer = immortabilityShield.GetNode<Timer>("Timer");

        double timeLeft = immortabilityTimer.IsStopped() ? 0 : immortabilityTimer.TimeLeft;

        immortabilityTimer.Stop();

        double newTime = timeLeft + time;
        immortabilityTimer.Start(newTime);
        immortabilityTimer.SetProcess(true);

        immortabilityShield.MaxValue = newTime;
        immortabilityShield.Visible = true;

        GetNode<CollisionShape2D>("Areas/PlayerDamageDetector/CollisionShape2D").SetDeferred("disabled", true);
    }



    public void SetCameraLimits(Vector4 value, bool doResetSmoothing = false)
    {
        SetCameraLimits(new Rect2(value[3], value[0], value[1] - value[3], value[2] - value[0]), doResetSmoothing);
    }

    public void SetCameraLimits(Rect2 value, bool doResetSmoothing = false)
    {
        G.CameraLimits = value;
        // The camera itself updates its limits when it's changed in G. However, to reset its smoothing you have to call the method yourself, as here.
        if (doResetSmoothing)
            Camera.CallDeferred("reset_smoothing");

        // The values in the camera used to be changed via this signal. Now it's just an auxiliary signal for modders.
        EmitSignal("CameraLimitsChanged", doResetSmoothing, value);
    }

    public void SetCameraPositionSmoothingSpeed(float value)
    {
        Camera.PositionSmoothingSpeed = value;
    }

    public void ResetCameraSmoothing()
    {
        Camera.ResetSmoothing();
    }

    public void SetGUIVisible(bool value)
    {
        GUI.Visible = value;
    }

    public void ToggleStandingPenalty(bool value)
    {
        EnableStandingPenalty = value;
        GUI.GetNode<TextureProgressBar>("StandBar").Visible = value;
        if (value == false)
            _moveCoeff = 1;
    }

    public void SetMinMoveCoeff(float value)
    {
        MinMoveCoeff = value;
    }
    public void SetMaxMoveCoeff(float value)
    {
        MaxMoveCoeff = value;
    }

    public void Toss(float velocity)
    {
        // To avoid unnecessary _lastActions entries
        BlockActsFor([Act.Walk, Act.Stand], 0.02f);
        BlockActsFor([Act.Jump, Act.Climb], 0.33f);

        Velocity = new Vector2(0, -velocity);

        Action(Act.Tossed);
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