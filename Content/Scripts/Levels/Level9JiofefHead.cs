using Godot;
using System;
using System.Linq;

public partial class Level9JiofefHead : Node2D
{
	AnimationPlayer _animationPlayer;
	AnimatedSprite2D _animatedSprite2D;
	CharacterBody2D _player;
	PackedScene _spitCross, _vomitCross, _jiofefEye;

	Vector2 _dashDirection, _spiderMovingDirection, _bullDashDirection, _moveDirection;
    Random _random = new Random();
    private readonly int[] _attackWeight = { 10, 8, 6, 6, 3, 6, 6};
    private readonly string[] _attackAnimationsList = { "CrossSpit", "CrossVomit", "Spin", "Bite", "Spider", "EyeGouging", "BullUlt"};
    private float _jiofefHeadSpeedMultiplier = 1, _initialDashStackedDirection = 0;
	private string _previousAttack;
	private bool _isPreparingForDash = false, _isBullDashing = false, _isUnpinned = false;

	public override void _Ready()
	{
		SetPhysicsProcess(false);
		G.IsProgressPaused = true;

		_animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		_animatedSprite2D = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        _player = GetNode<CharacterBody2D>("../Player");
        _spitCross = ResourceLoader.Load<PackedScene>("res://Content/Scenes/Other/Level9SpitCross.tscn");
        _vomitCross = ResourceLoader.Load<PackedScene>("res://Content/Scenes/Other/Level9VomitCross.tscn");
		_jiofefEye = ResourceLoader.Load<PackedScene>("res://Content/Scenes/Other/Level9JiofefEye.tscn");
		
        for (int i = 0; i < _attackAnimationsList.Length; i++)
			_animationPlayer.AnimationSetNext(_attackAnimationsList[i] + "Attack", "Idle");
        _animationPlayer.AnimationSetNext("Hahahahahahahaha", "Idle");
        _animationPlayer.AnimationSetNext("Unpinning", "Idle");
    }

	public override void _PhysicsProcess(double delta)
	{
        if (_isUnpinned && _animationPlayer.CurrentAnimation != "BullUltAttack" && _animationPlayer.CurrentAnimation != "BiteAttack" && _animationPlayer.CurrentAnimation != "SpiderAttack" && _animationPlayer.CurrentAnimation != "D E A T H")
		{
            Translate(_moveDirection  * _jiofefHeadSpeedMultiplier);

			float XRadius = 100 * Scale.X * _animatedSprite2D.Scale.X, YRadius = 150 * Scale.Y * _animatedSprite2D.Scale.Y;

            if (GlobalPosition.X >= (G.LevelXYSizes[G.CurrentLevel].X - XRadius) || GlobalPosition.X < XRadius)
			{
                GetNode<CpuParticles2D>("WallSlammingParticles").Emitting = true;
                _moveDirection.X *= -1;
            }
            if (GlobalPosition.Y >= (G.LevelXYSizes[G.CurrentLevel].Y - YRadius) || GlobalPosition.Y < YRadius)
			{
                GetNode<CpuParticles2D>("WallSlammingParticles").Emitting = true;
                _moveDirection.Y *= -1;
            }
        }

		if (G.Scores > 75 && _random.Next(600) == 0 && _animationPlayer.CurrentAnimation != "D E A T H" || _animationPlayer.CurrentAnimation == "")
			_animationPlayer.Play("Hahahahahahahaha");

		if (_animationPlayer.CurrentAnimation == "CrossSpitAttack" || _animationPlayer.CurrentAnimation == "CrossVomitAttack" && _animatedSprite2D.Frame == 2)
		{
            Rotation += GetAngleTo(_player.Position);
			float RotationDegreesBy360 = RotationDegrees % 360;
            _animatedSprite2D.FlipV = RotationDegreesBy360 > 90 && RotationDegreesBy360 < 270 || RotationDegreesBy360 < -90 && RotationDegreesBy360 > -270;
        }

		if (_animationPlayer.CurrentAnimation == "CrossVomitAttack" && _animationPlayer.CurrentAnimationPosition > 1.5f)
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
            float DistanceToPlayerCoeff = Position.DistanceTo(_player.Position) / 100;
            if (DistanceToPlayerCoeff < 1)
                DistanceToPlayerCoeff = 1;

			Vector2 PlayerDirectionToHead = _player.GlobalPosition.DirectionTo(GlobalPosition);
            _player.Velocity += new Vector2(PlayerDirectionToHead.X / (DistanceToPlayerCoeff / 2) * 2f,PlayerDirectionToHead.Y / (DistanceToPlayerCoeff / 1.5f) * 90);

			var AllCrossesOnScreen = GetTree().GetNodesInGroup("Crosses");
			for (int  i = 0; i < AllCrossesOnScreen.Count; i++)
			{
				var Cross = (Node2D)AllCrossesOnScreen[i];
				Cross.Translate(Cross.GlobalPosition.DirectionTo(GlobalPosition + new Vector2(_random.Next(25), _random.Next(25))) * 2.5f * _jiofefHeadSpeedMultiplier);
			}
		}

		if (_animationPlayer.CurrentAnimation == "SpiderAttack" && _animationPlayer.CurrentAnimationPosition > 5f && _animationPlayer.CurrentAnimationPosition < 24f)
		{
			float DashStackedDirection = Math.Abs(_dashDirection.X) + Math.Abs(_dashDirection.Y);
			if (DashStackedDirection > 0.2f && _isPreparingForDash)
			{
				Translate(-_dashDirection / 10);
			}
			else if (DashStackedDirection > 0.2f)
			{
				Translate(_dashDirection);
				_dashDirection -= _dashDirection / 20;
			}
			_spiderMovingDirection += (GlobalPosition.DirectionTo(_player.GlobalPosition) * ((75 - DashStackedDirection) / 75) * 5) - _spiderMovingDirection;
			Translate(_spiderMovingDirection);
		}
		else
			_spiderMovingDirection = new Vector2(0, 0);

		if (_isBullDashing)
		{
			Translate(_bullDashDirection * (60 * _jiofefHeadSpeedMultiplier));
			if (Position.X < -300 || Position.X > 2860)
				_isBullDashing = false;
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
		{
			Attack();
        }
    }

	public void AnimationChanged(string OldAnimation, string NewAnimation)
	{
		SpeedMultiplayerUpdate();

		if (OldAnimation == "Hahahahahahahaha")
			Attack();
		else if (_random.Next(2) == 0 && G.Scores > 25)
			_animationPlayer.Play("Hahahahahahahaha");

		if (G.Scores > 50 && !_isUnpinned)
			_animationPlayer.Play("Unpinning");

		if (OldAnimation == "Unpinning")
		{
			_moveDirection = new Vector2(_random.Next(2) == 0 ? -1.666f : 1.666f, _random.Next(2) == 0 ? -1 : 1);
			_isUnpinned = true;
            _animationPlayer.Play("Idle");
        }

		if (G.Scores > 150)
		{
			GetNode<Timer>("SpeedMultiplierUpdateTimer").Stop();
			_animationPlayer.Play("D E A T H");
			_animationPlayer.SpeedScale = 1;
			G.CrossSpawnMultiplier = 0.0001f;
		}
	}

	public void SpeedMultiplayerUpdate()
	{
        _jiofefHeadSpeedMultiplier = 1.5f + G.Scores / 60;
        _animatedSprite2D.SpeedScale = _jiofefHeadSpeedMultiplier;
        _animationPlayer.SpeedScale = _jiofefHeadSpeedMultiplier;
		G.CrossSpawnMultiplier = 1 / (1 + ((_jiofefHeadSpeedMultiplier - 1) / 3 * (1 - 0.5f * Meta.Instance.Dificulty)));
    }


	public void CrossSpit()
	{
        var spitSpawnPoint = GetNode<Node2D>("AnimatedSprite2D/SpitSpawnPoint");
        spitSpawnPoint.Position = new Vector2(80, 73 * (_animatedSprite2D.FlipV ? -1 : 1));
        for (int i = 0; i < _random.Next(3, 6); i++)
		{
            var SpitCross = (Node2D)_spitCross.Instantiate();
            SpitCross.GlobalPosition = spitSpawnPoint.GlobalPosition;
            GetParent().AddChild(SpitCross);
        }
	}

	public void EyeGouge(Vector2 EyeSpawnPosition)
	{
		var JiofefEye = _jiofefEye.Instantiate<Node2D>();
		JiofefEye.GlobalPosition = GlobalPosition + EyeSpawnPosition;
		GetParent().AddChild(JiofefEye);
	}

	public void PreparingToDash()
	{
		_dashDirection = GlobalPosition.DirectionTo(_player.GlobalPosition) * 50;
        _initialDashStackedDirection = Math.Abs(_dashDirection.X) + Math.Abs(_dashDirection.Y);
        _isPreparingForDash = true;
	}

	public void Dash()
	{
		_isPreparingForDash = false;
	}

	public void PrepareDoBullDash()
	{
		_isBullDashing = false;
        _bullDashDirection = new Vector2(_random.Next(2) == 0 ? -1 : 1, 0);
		Scale = new Vector2(_bullDashDirection.X, Scale.Y);
		GlobalPosition = new Vector2(_bullDashDirection.X == 1 ? -300 : 2860, _player.GlobalPosition.Y + _random.Next(-100, 100));
	}

	public void SetBullDashState(bool value)
	{
		_isBullDashing = value;
	}

    public void TeleportTo(Vector2 value)
    {
        GlobalPosition = value;
    }

    public void TeleportToPlayer()
	{
		GlobalPosition = _player.GlobalPosition;
	}

    public void TeleportToRandomPoint()
	{
		GlobalPosition = new Vector2(_random.Next(2560), _random.Next(1280));
	}

	private void Attack()
	{
        int SelectedAttackNumber;
        do
        {
            int RandomNumber = _random.Next(_attackWeight.Sum());
            for (int i = 0; ; i++)
            {
                if (RandomNumber < _attackWeight[i])
                {
                    SelectedAttackNumber = i;
                    break;
                }
                else RandomNumber -= _attackWeight[i];
            }
        }
        while (_attackAnimationsList[SelectedAttackNumber] == _previousAttack);

        _animationPlayer.Play(_attackAnimationsList[SelectedAttackNumber] + "Attack");
		if (G.Scores > 100 && _random.Next(4) == 0 && _animationPlayer.CurrentAnimation != "BiteAttack")
		_animationPlayer.PlayBackwards();

        _previousAttack = _attackAnimationsList[SelectedAttackNumber];
    }

	private void AFittingEndToAnExistenceDefinedByFutileStruggle()
	{
		var jiofefHeadDeathParticles = (CpuParticles2D)ResourceLoader.Load<PackedScene>("res://Content/Scenes/Other/Level9JiofefHeadDeathParticles.tscn").Instantiate();
		jiofefHeadDeathParticles.GlobalPosition = GlobalPosition;
		jiofefHeadDeathParticles.Emitting = true;
		GetParent().AddChild(jiofefHeadDeathParticles);
		QueueFree();
	}
}
