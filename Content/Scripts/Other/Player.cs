using Godot;

public class Player : KinematicBody2D
{
    [Export] float _speed = 200, _gravity = 9.8f, _jumpForce = 300;

    // floats related to wall-jumping
    private float _wallDetectNumber, _inertion, _savedWallNumber, _wallJumpTimer = 0,

          // wall climbs
          _climbBufer, _climbTimer, _savedClimbWallNumber, _climbUncontrollingTimer,

          // other
          _moveCalculationTimer = 0;

    // other variable
    private bool _isFliph;

    private string _animationName;

    private byte _moveCalculationStartTimer;

    private enum State {Default, DownDash, Inerted}
    State _state = State.Default;

    AnimatedSprite _animatedSprite;

    Vector2 _motion = new Vector2();
    Vector2[] _savedPastPositions = new Vector2[11];

    private void PlaySound(string SoundName)
    {
        GetNode<AudioStreamPlayer>("Sounds/" + SoundName).Play();
    }
    public override void _Ready()
    {
        _animatedSprite = GetNode<AnimatedSprite>("AnimatedSprite");
    }
    public override void _PhysicsProcess(float delta)
    {
        if (G.PlayerDead) return;
        //Player control and physic consequence
        float DoFlip = Input.GetActionStrength("ui_right") - Input.GetActionStrength("ui_left");

        if (_state != State.DownDash)
        _motion.x = Input.GetActionStrength("ui_right") - Input.GetActionStrength("ui_left");
        _motion.x *= _speed;

        if (_wallDetectNumber != 0 && Input.IsActionPressed("wall_catch") && !IsOnFloor() && _motion.y > 0 && _state != State.DownDash)
            if (!Input.IsActionPressed("jump") || _inertion != 0)
            {
                _motion.y = 0;
                _animationName = "WallCatch";
            }

        if (_state == State.Inerted)
        {
            float uncontrolling = _motion.x * _inertion / 2.5f;
            _motion.x += _inertion > 0 ? -uncontrolling : uncontrolling;
        }

        if (_climbBufer < 3 && _climbUncontrollingTimer > 0)
        {
            _motion.x /= _motion.x / _savedClimbWallNumber < 0 ? _climbUncontrollingTimer * 3 + 1 : 1;
        }

        _motion = MoveAndSlide(_motion, Vector2.Up);

        if (IsOnFloor() && _motion.x == 0)
            _animationName = "Idle";

        if (Input.IsActionPressed("jump"))
        {
            if (IsOnFloor())
            {
                _motion.y = -_jumpForce;
                PlaySound("Jump");
            }
            else if (Input.IsActionPressed("wall_catch") && _wallDetectNumber != 0 && _inertion == 0 && _wallJumpTimer < 0 && _climbTimer < 0)
            {
                _savedWallNumber = _wallDetectNumber;
                _motion.y = -_jumpForce;
                _inertion = 2.5f * -_savedWallNumber;
                _state = State.Inerted;
                PlaySound("Climb");
            }
            else if (!Input.IsActionPressed("wall_catch") && _wallDetectNumber != 0 && _climbTimer < 0 && _climbBufer > 0)
            {
                _climbTimer = 0.2f;
                _climbBufer--;
                _motion.y = -_jumpForce * 0.7f;
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
            if (_savedWallNumber != 0)
            {
                _motion.x += _inertion * _speed;
                _inertion -= _inertion > 0 ? 0.05f : -0.05f;
                if (_inertion < 0.1f && _inertion > 0 || _inertion > -0.1f && _inertion < 0)
                    _inertion = 0;
            }

            if (_animationName != "WallCatch" && _animationName != "Climb" || _animationName == "WallCatch" && _wallDetectNumber == 0)
                _animationName = _motion.y < 0 ? "Jump" : "Fall";
            _wallJumpTimer -= delta;
            _climbTimer -= delta;
            _climbUncontrollingTimer -= delta;
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
            {
                _state = State.Default;
                PlaySound("DownPullHit");
            }
        }


        if (_motion.x != 0 && IsOnFloor())
        {
            _animationName = "Walk";
        }


        _motion.y += _gravity;

        if (_state == State.DownDash)
        {
            _motion.x = 0;
            _motion.y = 500;
            _animationName = "Fall";
        }

        //Animations
        if (_animationName == "Climb" || _animationName == "WallCatch")
            DoFlip = _wallDetectNumber;

        if (_animatedSprite.Animation != _animationName)
            _animatedSprite.Animation = _animationName;

        if (DoFlip != 0)
            _isFliph = DoFlip == 1 ? true : false;
        _animatedSprite.FlipH = _isFliph;


        //Physics injection
        _moveCalculationTimer += delta * 3;
        if (_moveCalculationTimer > 0.1f)
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

        MoveAndSlide(_motion);

        var ghost = GetNode<Sprite>("Ghost");
        if (ghost.Visible)
        {
            ghost.GlobalPosition = new Vector2(_savedPastPositions[0].x, _savedPastPositions[0].y);
        }

        _motion.x = 0;
    }

    public void AnimationFinished()
    {
        if (_animationName == "Climb")
            _animationName = "Jump";
    }
    public void DamageGetting(Area BodyDetected)
    {
        if (G.PlayerDead) return;
        G.PlayerDead = true;
        var animatedSprite = GetNode<AnimatedSprite>("AnimatedSprite");
        animatedSprite.Visible = false;
        var deadPlayer = GetNode<Sprite>("DeadPlayer/Sprite");
        deadPlayer.Visible = true;
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
