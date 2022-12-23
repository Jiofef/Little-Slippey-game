using Godot;
using Godot.Collections;
using System;
using System.Runtime.CompilerServices;

public class Player : KinematicBody2D
{
    [Export] float _speed = 200, _gravity = 9.8f, _jumpforce = 300;

    // floats related to wall-jumping
    float _walldetectnumber, _inertion, _savedwallnumber, _walljumptimer = 0,

          // wall climbs
          _climbbufer, _climbtimer, _savedclimbwallnumber, _climbuncontrollingtimer,

          // other
          _movecalctimer = 0;

    // other variable
    bool _isfliph;

    string _animationname;

    byte _movecalcstarttimer;
    //
    enum State {Default, DownDash, Inerted}
    State _state = State.Default;

    AnimatedSprite animatedSprite;

    Vector2 motion = new Vector2();
    Vector2[] pastposlog = new Vector2[11];

    private void PlaySound(string SoundName)
    {
        GetNode<AudioStreamPlayer>("Sounds/" + SoundName).Play();
    }
    public override void _Ready()
    {
        animatedSprite = GetNode<AnimatedSprite>("AnimatedSprite");
    }
    public override void _PhysicsProcess(float delta)
    {
        if (G._playerdead) return;
        //Player control and physic consequence
        float DoFlip = Input.GetActionStrength("ui_right") - Input.GetActionStrength("ui_left");

        if (_state != State.DownDash)
        motion.x = Input.GetActionStrength("ui_right") - Input.GetActionStrength("ui_left");
        motion.x *= _speed;

        if (_walldetectnumber != 0 && Input.IsActionPressed("wall_catch") && !IsOnFloor() && motion.y > 0 && _state != State.DownDash)
            if (!Input.IsActionPressed("jump") || _inertion != 0)
            {
                motion.y = 0;
                _animationname = "WallCatch";
            }

        if (_state == State.Inerted)
        {
            float uncontrolling = motion.x * _inertion / 2.5f;
            motion.x += _inertion > 0 ? -uncontrolling : uncontrolling;
        }

        if (_climbbufer < 3 && _climbuncontrollingtimer > 0)
        {
            motion.x /= motion.x / _savedclimbwallnumber < 0 ? _climbuncontrollingtimer * 3 + 1 : 1;
        }

        motion = MoveAndSlide(motion, Vector2.Up);

        if (IsOnFloor() && motion.x == 0)
            _animationname = "Idle";

        if (Input.IsActionPressed("jump"))
        {
            if (IsOnFloor())
            {
                motion.y = -_jumpforce;
                PlaySound("Jump");
            }
            else if (Input.IsActionPressed("wall_catch") && _walldetectnumber != 0 && _inertion == 0 && _walljumptimer < 0 && _climbtimer < 0)
            {
                _savedwallnumber = _walldetectnumber;
                motion.y = -_jumpforce;
                _inertion = 2.5f * -_savedwallnumber;
                _state = State.Inerted;
                PlaySound("Climb");
            }
            else if (!Input.IsActionPressed("wall_catch") && _walldetectnumber != 0 && _climbtimer < 0 && _climbbufer > 0)
            {
                _climbtimer = 0.2f;
                _climbbufer--;
                motion.y = -_jumpforce * 0.7f;
                _savedclimbwallnumber = _walldetectnumber;
                _climbuncontrollingtimer = 1;
                animatedSprite.Frame = 0;
                _animationname = "Climb";
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
            if (_savedwallnumber != 0)
            {
                motion.x += _inertion * _speed;
                _inertion -= _inertion > 0 ? 0.05f : -0.05f;
                if (_inertion < 0.1f && _inertion > 0 || _inertion > -0.1f && _inertion < 0)
                    _inertion = 0;
            }

            if (_animationname != "WallCatch" && _animationname != "Climb" || _animationname == "WallCatch" && _walldetectnumber == 0)
                _animationname = motion.y < 0 ? "Jump" : "Fall";
            _walljumptimer -= delta;
            _climbtimer -= delta;
            _climbuncontrollingtimer -= delta;
        }
        else
        {
            _savedwallnumber = 0;
            _inertion = 0;
            _walljumptimer = 0.3f;
            _climbtimer = 0.2f;
            _climbbufer = 3;
            _climbuncontrollingtimer = 0;

            if (_state == State.DownDash)
            {
                _state = State.Default;
                PlaySound("DownPullHit");
            }
        }


        if (motion.x != 0 && IsOnFloor())
        {
            _animationname = "Walk";
        }


        motion.y += _gravity;

        if (_state == State.DownDash)
        {
            motion.x = 0;
            motion.y = 500;
            _animationname = "Fall";
        }

        //Animations
        if (_animationname == "Climb" || _animationname == "WallCatch")
            DoFlip = _walldetectnumber;

        if (animatedSprite.Animation != _animationname)
            animatedSprite.Animation = _animationname;

        if (DoFlip != 0)
            _isfliph = DoFlip == 1 ? true : false;
        animatedSprite.FlipH = _isfliph;


        //Physics injection
        _movecalctimer += delta * 3;
        if (_movecalctimer > 0.1f)
        {
            _movecalctimer = 0;
            pastposlog[pastposlog.Length - 1] = Position;
            for (int i = 0; i < pastposlog.Length - 1; i++)
                pastposlog[i] = pastposlog[i + 1];
            float MoveDist = Position.DistanceTo(pastposlog[0]);
            if (_movecalcstarttimer > 10)
                G._movecoeffplayer = MoveDist < 200 ? MoveDist / 200 : 1;
            else _movecalcstarttimer++;
        }


        MoveAndSlide(motion);

        var ghost = GetNode<Sprite>("Ghost");
        if (ghost.Visible)
        {
            ghost.GlobalPosition = new Vector2(pastposlog[0].x, pastposlog[0].y);
        }

        motion.x = 0;
    }

    public void AnimationFinished()
    {
        if (_animationname == "Climb")
            _animationname = "Jump";
    }
    public void DamageGetting(Area BodyDetected)
    {
        if (G._playerdead) return;
        G._playerdead = true;
        var animatedSprite = GetNode<AnimatedSprite>("AnimatedSprite");
        animatedSprite.Visible = false;
        var deadPlayer = GetNode<Sprite>("DeadPlayer/Sprite");
        deadPlayer.Visible = true;
    }

    public void LeftWallDetect(Node BodyDetected)
    {
        _walldetectnumber = -1;
    }
    public void RightWallDetect(Node BodyDetected)
    {
        _walldetectnumber = 1;
    }
    public void WallUndetected(Node BodyDetected)
    {
        _walldetectnumber = 0;
    }
}
