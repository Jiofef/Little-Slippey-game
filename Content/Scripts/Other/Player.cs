using Godot;

public partial class Player : CharacterBody2D
{
    [Export] float _speed = 400, _gravity = 18.6f, _jumpForce = 600;

    // floats related to wall-jumping
    private float _wallDetectNumber, _inertion, _savedWallNumber, _wallJumpTimer = 0,

          // wall climbs
          _climbBufer, _climbTimer, _savedClimbWallNumber, _climbUncontrollingTimer,

          // other
          _moveCalculationTimer = 0;
          readonly float _floatDelta = 0.016667f;

    // other variable
    private bool _isFliph;

    private string _animationName;

    private byte _moveCalculationStartTimer;

    private enum State {Default, DownDash, Inerted}
    State _state = State.Default;

    AnimatedSprite2D _animatedSprite;

    Vector2 _motion = new Vector2();
    Vector2[] _savedPastPositions = new Vector2[11];

    private void PlaySound(string SoundName)
    {
        GetNode<AudioStreamPlayer>("Sounds/" + SoundName).Play();
    }

    public override void _Ready()
    {
        _animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
    }

    public override void _PhysicsProcess(double delta)
    {
        //Player control and physic consequence
        _motion = Velocity;
        _motion.Y += _gravity;

        if (_state != State.DownDash)
        {
            _motion.X = Input.GetActionStrength("ui_right") - Input.GetActionStrength("ui_left");
            _motion.X *= _speed;

            if (_state == State.Inerted)
            {
                float uncontrolling = _motion.X * _inertion / 2.5f;
                _motion.X += _inertion > 0 ? -uncontrolling : uncontrolling;
            }
        }
    
        if (_wallDetectNumber != 0 && Input.IsActionPressed("wall_catch") && !IsOnFloor() && _motion.Y > 0 && _state != State.DownDash)
            if (!Input.IsActionPressed("jump") || _inertion != 0)
            {
                _motion.Y = 15;
                _animationName = "WallCatch";
            }

        if (_climbBufer < 3 && _climbUncontrollingTimer > 0)
            _motion.X /= _motion.X / _savedClimbWallNumber < 0 ? _climbUncontrollingTimer * 3 + 1 : 1;

        if (Input.IsActionPressed("jump"))
        {
            if (IsOnFloor())
            {
                _motion.Y = -_jumpForce;
                PlaySound("Jump");
            }
            else if (Input.IsActionPressed("wall_catch") && _wallDetectNumber != 0 && _inertion == 0 && _wallJumpTimer < 0 && _climbTimer < 0)
            {
                _savedWallNumber = _wallDetectNumber;
                _motion.Y = -_jumpForce;
                _inertion = 1.25f * -_savedWallNumber;
                _state = State.Inerted;
                PlaySound("Climb");
            }
            else if (!Input.IsActionPressed("wall_catch") && _wallDetectNumber != 0 && _climbTimer < 0 && _climbBufer > 0)
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
        else if (Input.IsActionJustPressed("down_pull") && !IsOnFloor() && _state != State.DownDash)
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
                _motion.Y = 1000;
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
                PlaySound("DownPullHit");
            _state = State.Default;
        }
        //Animations
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

        float DoFlip = _animationName == "Climb" || _animationName == "WallCatch" ? _wallDetectNumber : Input.GetActionStrength("ui_right") - Input.GetActionStrength("ui_left");
        if (DoFlip != 0)
            _isFliph = DoFlip == 1 ? true : false;
        _animatedSprite.FlipH = _isFliph;


        //Physics injection
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

        //BOO
        var ghost = GetNode<Sprite2D>("Ghost");
        if (ghost.Visible)
            ghost.GlobalPosition = new Vector2(_savedPastPositions[0].X, _savedPastPositions[0].Y);
    }

    public void AnimationFinished()
    {
        if (_animationName == "Climb")
            _animationName = "Jump";
        _animatedSprite.Play();
    }

    public void DamageGetting(Area2D BodyDetected)
    {
        if (G.PlayerDead) return;
        G.PlayerDead = true;
        GetNode<AnimatedSprite2D>("AnimatedSprite2D").Visible = false;
        GetNode<Sprite2D>("DeadPlayer/Sprite2D").Visible = true;
        SetPhysicsProcess(false);
        G.SaveRecords();
        UnchangableMeta.SaveToFile();
    }

    public void LeftWallDetect(Node BodyDetected)
    {
        _wallDetectNumber = -1;
    }

    public void RightWallDetect(Node BodyDetected)
    {
        _wallDetectNumber = 1;
    }

    public void WallUndetected(Node BodyDetected)
    {
        _wallDetectNumber = 0;
    }
}