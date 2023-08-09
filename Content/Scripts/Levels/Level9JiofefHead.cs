using Godot;
using System;

public partial class Level9JiofefHead : Node2D
{
	Random _random = new Random();
	AnimationPlayer _animationPlayer;
	AnimatedSprite2D _animatedSprite2D;
	CharacterBody2D _player;

	private float _jiofefHeadSpeedMultiplier = 1;
	private readonly string[] _attackAnimationsList = {"CrossSpit"};
	public override void _Ready()
	{
		_animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		_animatedSprite2D = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        _player = GetNode<CharacterBody2D>("../Player");
	}

	public override void _PhysicsProcess(double delta)
	{
        if (_animationPlayer.CurrentAnimation == "CrossSpitAttack")
		{
            Rotation += GetAngleTo(_player.Position);
			float RotationDegreesBy360 = RotationDegrees % 360;
            _animatedSprite2D.FlipV = RotationDegreesBy360 > 90 && RotationDegreesBy360 < 270 || RotationDegreesBy360 < -90 && RotationDegreesBy360 > -270;
        }

	}

	public void GodotIsShit()
	{
		_animationPlayer.CurrentAnimation = "TurningOn";
		_animationPlayer.Play();
	}

	public void IdleCiclePassed()
	{
        if (_random.Next(4) == 0)
        {
            _animationPlayer.CurrentAnimation = _attackAnimationsList[_random.Next(_attackAnimationsList.Length)] + "Attack";
        }
    }

	public void CrossSpit()
	{

	}
}
