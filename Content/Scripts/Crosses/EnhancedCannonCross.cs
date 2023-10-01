using Godot;
using System;

public partial class EnhancedCannonCross : Node2D
{
    PackedScene _bomb = new PackedScene();
    CharacterBody2D _player;
	Sprite2D _sprite2D;

	Random _random = new Random();

	private enum State {FlyingToPlayer, Bombing, FlyingAway}
	State _state = State.FlyingToPlayer; 
	int _bombsLeft = 13, _ticksToNextBomb;
	private Vector2 _movementPoint = new Vector2(), _movementGlobalPoint, _velocity;

    public override void _Ready()
	{
		_player = GetNode<CharacterBody2D>("../Player");
		_sprite2D = GetNode<Sprite2D>("Sprite2D");
        _bomb = ResourceLoader.Load<PackedScene>("res://Content/Scenes/Crosses/HelicopterBomb.tscn");

        _movementPoint = new Vector2(_random.Next(-300, 300), -275 + _random.Next(-75, 75));
    }

	public override void _PhysicsProcess(double delta)
	{
		if (_state != State.FlyingAway)
		{
            _movementGlobalPoint = _player.GlobalPosition + _movementPoint;
			if (_movementGlobalPoint.Y < G.CameraLimits.X)
				_movementGlobalPoint.Y = G.CameraLimits.X;
        }

		_velocity += (_movementGlobalPoint - GlobalPosition) / 500;
        GlobalTranslate(_velocity);
		_velocity /= 1.1f;
		_sprite2D.FlipH = _velocity.X < 0;

        switch (_state)
		{
			case State.FlyingToPlayer:
				if (GlobalPosition.DistanceTo(_movementGlobalPoint) < 300)
					_state = State.Bombing;
				break;

            case State.Bombing:
				_ticksToNextBomb--;
				if (_random.Next(60) == 0)
					_movementPoint = new Vector2(_random.Next(-300, 300), -275 + _random.Next(-75, 75));
				if (_ticksToNextBomb <= 0)
				{
					var Bomb = (Node2D)_bomb.Instantiate();
					Bomb.GlobalPosition = GlobalPosition;
					if (_sprite2D.FlipH)
						Bomb.Scale = new Vector2(-1, 1);
					GetParent().AddChild(Bomb);
                    _ticksToNextBomb = 20 + _random.Next(21);
					_bombsLeft--;
					if (_bombsLeft <= 0)
					{
                        _state = State.FlyingAway;
						_movementGlobalPoint = new Vector2(GlobalPosition.X, G.CameraLimits.X - 300);
                    }
                }
                break;

            case State.FlyingAway:
				if (GlobalPosition.Y < G.CameraLimits.X - 300)
				{

					QueueFree();
					GD.Print("s");
				}
                break;
        }
	}
}
