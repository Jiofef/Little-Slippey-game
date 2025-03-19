using Godot;
using System;
using System.Collections.Generic;

public partial class EnhancedCannonCross : UnusualCrossNode
{
    public CrossSpawner ParentSpawner;

    public PackedScene Bomb;
	public Sprite2D HelicopterSprite;
    public ColorRect Blades;

	Random _random = new Random();

	private enum State {FlyingToPlayer, Bombing, FlyingAway}
	State _state = State.FlyingToPlayer; 
	private int _bombsLeft = 13, _ticksToNextBomb;
	private Vector2 _movementPoint = new Vector2(), _movementGlobalPoint, _velocity;

    public override void _Ready()
	{
        // Initializing nodes
        HelicopterSprite = GetNode<Sprite2D>("HelicopterSprite");
        Blades = GetNode<ColorRect>("Blades");

        Bomb = ResourceLoader.Load<PackedScene>("res://Content/Scenes/Crosses/HelicopterBomb.tscn");

        if (GetParent() is CrossSpawner spawner)
        {
            ParentSpawner = spawner;
            if (!spawner.EverythingImportant.ContainsKey("HelicopterBombsSavedPool"))
            {
                spawner.EverythingImportant.Add("HelicopterBombsSavedPool", new List<HelicopterBomb>());
            }

            _bombsPool = ParentSpawner.EverythingImportant["HelicopterBombsSavedPool"] as List<HelicopterBomb>;
        }

        _movementPoint = new Vector2(_random.Next(-300, 300), -275 + _random.Next(-75, 75));
    }

    // Blades animation
    private const int BLADES_FRAMES = 6;
    private readonly float[] _bladesScaleFrames = [1, 0.66f, 0.33f, 0.0f, 0.33f, 0.66f];
    private byte _bladesAnimationFrame;
    private Vector2 _bladesScale = new Vector2(1, 1);

	public override void _PhysicsProcess(double delta)
	{
        // Blades animation
        if (_bladesAnimationFrame < BLADES_FRAMES - 1)
            _bladesAnimationFrame++;
        else
            _bladesAnimationFrame = 0;
        _bladesScale.X = _bladesScaleFrames[_bladesAnimationFrame];
        Blades.Scale = _bladesScale;

        // Player following
		if (_state != State.FlyingAway)
		{
            _movementGlobalPoint = G.Player.GlobalPosition + _movementPoint;
			if (_movementGlobalPoint.Y < G.CameraLimits.Position.Y)
				_movementGlobalPoint.Y = G.CameraLimits.Position.Y;
        }

        // Moving
		_velocity += (_movementGlobalPoint - GlobalPosition) / 600;
        GlobalTranslate(_velocity);
		_velocity /= 1.1f;
        
		Scale = new Vector2(_velocity.X > 0 ? 1 : -1, 1);
        RotationDegrees += _velocity.X / 5;
		if (RotationDegrees > 20)
			RotationDegrees = 20;
		else if (RotationDegrees < -20)
			RotationDegrees = -20;
		RotationDegrees /= 1.1f;

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
                    DropBomb();

                break;

            case State.FlyingAway:
				if (GlobalPosition.Y < G.CameraLimits.Position.Y - 250)
					OnFinished();
                break;
        }
	}

    private List<HelicopterBomb> _bombsPool;
    public void DropBomb()
	{
        HelicopterBomb bomb;
        _ticksToNextBomb = 20 + _random.Next(21);
        _bombsLeft--;


        if (_bombsPool.Count > 0)
        {
            int id = _bombsPool.Count - 1;
            bomb = _bombsPool[id];
            _bombsPool.RemoveAt(id);

            bomb.Position = Position;

            bomb.Respawn();
        }
        else
        {
            bomb = (HelicopterBomb)Bomb.Instantiate();

            bomb.ShouldBeSavedInPool = ShouldBeSavedInPool;
            bomb.Save += () => _bombsPool.Add(bomb);

            bomb.Position = Position;

            GetParent().AddChild(bomb);
        }

        bomb.GlobalRotation = 0;
        if (Scale.X == -1)
            bomb.Scale = new Vector2(-1, 1);


        if (_bombsLeft <= 0)
        {
            _state = State.FlyingAway;
            _movementGlobalPoint = new Vector2(GlobalPosition.X, G.CameraLimits.Position.Y - 400);
        }
    }

    public override void Respawn()
    {
        base.Respawn();

        _movementPoint = new Vector2(_random.Next(-300, 300), -275 + _random.Next(-75, 75));
        _state = State.FlyingToPlayer;

        _bombsLeft = 13;
    }
}
