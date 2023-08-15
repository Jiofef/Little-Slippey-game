using Godot;
using System;

public partial class Level9JiofefHead : Node2D
{
	AnimationPlayer _animationPlayer;
	AnimatedSprite2D _animatedSprite2D;
	CharacterBody2D _player;
	PackedScene _spitCross, _vomitCross;

    Random _random = new Random();
    private float _jiofefHeadSpeedMultiplier = 1;
	private readonly string[] _attackAnimationsList = {"CrossSpit", "CrossVomit", "Spin"};

	public override void _Ready()
	{
		SetPhysicsProcess(false);
		G.IsProgressPaused = true;

		_animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		_animatedSprite2D = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        _player = GetNode<CharacterBody2D>("../Player");
        _spitCross = ResourceLoader.Load<PackedScene>("res://Content/Scenes/Other/Level9SpitCross.tscn");
        _vomitCross = ResourceLoader.Load<PackedScene>("res://Content/Scenes/Other/Level9VomitCross.tscn");

		for (int i = 0; i < _attackAnimationsList.Length; i++)
			_animationPlayer.AnimationSetNext(_attackAnimationsList[i] + "Attack", "Idle");
    }

	public override void _PhysicsProcess(double delta)
	{
		if (_animationPlayer.CurrentAnimation == "CrossSpitAttack" || _animationPlayer.CurrentAnimation == "CrossVomitAttack" && _animatedSprite2D.Frame == 2)
		{
            Rotation += GetAngleTo(_player.Position);
			float RotationDegreesBy360 = RotationDegrees % 360;
            _animatedSprite2D.FlipV = RotationDegreesBy360 > 90 && RotationDegreesBy360 < 270 || RotationDegreesBy360 < -90 && RotationDegreesBy360 > -270;
        }

		if (_animatedSprite2D.Animation == "CrossVomit" && _animatedSprite2D.Frame == 2)
		{
			if (_random.Next(4) == 0)
			{
                var VomitCross = (Node2D)_vomitCross.Instantiate();

                var SpitSpawnPoint = GetNode<Node2D>("AnimatedSprite2D/SpitSpawnPoint");
                SpitSpawnPoint.Position = new Vector2(80, 73 * (_animatedSprite2D.FlipV ? -1 : 1));

                VomitCross.GlobalPosition = SpitSpawnPoint.GlobalPosition;
                GetParent().AddChild(VomitCross);
            }
        }
		if (_animationPlayer.CurrentAnimation == "SpinAttack" && _animationPlayer.CurrentAnimationPosition > 4.5f)
		{
			float PlayerAddVelocityCoeff = Position.DistanceTo(_player.Position) / 30;
			_player.Velocity += new Vector2((Position.X - _player.Position.X) / 30, (Position.Y - _player.Position.Y) / 30);
		}
	}

	public void GodotIsShit()
	{
		G.IsProgressPaused = false;
		_animationPlayer.CurrentAnimation = "TurningOn";
		_animationPlayer.Play();
	}

	public void IdleCiclePassed()
	{
        if (_random.Next(2) == 0)
            _animationPlayer.Play(_attackAnimationsList[_random.Next(_attackAnimationsList.Length)] + "Attack");
    }

	public void SpeedMultiplayerUpdate()
	{
        _jiofefHeadSpeedMultiplier = 1 + G.Scores / 50;
        _animatedSprite2D.SpeedScale = _jiofefHeadSpeedMultiplier;
        _animationPlayer.SpeedScale = _jiofefHeadSpeedMultiplier;
    }

	public void CrossSpit()
	{
        var SpitSpawnPoint = GetNode<Node2D>("AnimatedSprite2D/SpitSpawnPoint");
        SpitSpawnPoint.Position = new Vector2(80, 73 * (_animatedSprite2D.FlipV ? -1 : 1));
        for (int i = 0; i < _random.Next(3, 6); i++)
		{
            var SpitCross = (Node2D)_spitCross.Instantiate();
            SpitCross.GlobalPosition = SpitSpawnPoint.GlobalPosition;
            GetParent().AddChild(SpitCross);
        }
	}
}
