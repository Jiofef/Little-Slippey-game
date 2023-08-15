using Godot;
using System;

public partial class Level6FlyingCar : AnimatableBody2D
{
	[Export] bool _isPreSpawned = false;

	private int _speed, _moveXVector;
    private bool _doExploded = false;
    private float _carHalfsYMotion;
    private bool _iDKHowNameThisVar; // if false, car will appear and dissapear in the foreground. If true, car will do it in the background.

	CollisionShape2D _collision, _areaCollision;
    Sprite2D _backHalf, _frontHalf;
	Node2D _model;

    public override void _Ready()
    {
        _moveXVector = (int)Scale.X;
        _speed = (3 + Meta.Instance.Dificulty) * _moveXVector;

        Random random = new Random();
        _iDKHowNameThisVar = Convert.ToBoolean(random.Next(2));

		_collision = GetNode<CollisionShape2D>("CollisionShape2D");
		_areaCollision = GetNode<CollisionShape2D>("CarArea/CollisionShape2D");
        _backHalf = GetNode<Sprite2D>("Model/BackHalf");
        _frontHalf = GetNode<Sprite2D>("Model/FrontHalf");
        _model = GetNode<Node2D>("Model");

		if (!_isPreSpawned)
		{
            Modulate = new Color(Modulate.R, Modulate.G, Modulate.B, 0);
            _areaCollision.SetDeferred("disabled", true);
            _collision.Disabled = true;
        }
    }
    public override void _PhysicsProcess(double delta)
	{
        if (_doExploded)
        {
            _backHalf.Position = new Vector2(_backHalf.Position.X - _moveXVector * 5, _backHalf.Position.Y + _carHalfsYMotion);
            _backHalf.Modulate = new Color(_backHalf.Modulate.R - 0.05f, _backHalf.Modulate.G - 0.05f, _backHalf.Modulate.B - 0.05f, _backHalf.Modulate.A - 0.05f);
            _backHalf.Rotate(0.03f * _moveXVector);
            _frontHalf.Position = new Vector2(_frontHalf.Position.X + _moveXVector * 5, _frontHalf.Position.Y + _carHalfsYMotion);
            _frontHalf.Modulate = new Color(_frontHalf.Modulate.R - 0.05f, _frontHalf.Modulate.G - 0.05f, _frontHalf.Modulate.B - 0.05f, _frontHalf.Modulate.A - 0.05f);
            _frontHalf.Rotate(0.03f * _moveXVector);

            _carHalfsYMotion += 0.15f;
            return;
        }

		Translate(new Vector2(_speed, 0));

        float DisAppearingCoeff = Position.X < 1280 ? (Position.X - 395) / 200 : (2100 - Position.X) / 200;
        if (DisAppearingCoeff <= 1)
		{
			Modulate = new Color(Modulate.R, Modulate.G, Modulate.B, DisAppearingCoeff);

            float NewModelScale = _iDKHowNameThisVar ? 0.5f + DisAppearingCoeff / 2 : 1.5f - DisAppearingCoeff / 2;
            _model.Scale = new Vector2(NewModelScale, NewModelScale);

			if (!_collision.Disabled)
			{
                _collision.SetDeferred("disabled", true); ;
                _areaCollision.SetDeferred("disabled", true);
            }
        }
		else
		{
            if (_collision.Disabled)
			{
                Modulate = new Color(Modulate.R, Modulate.G, Modulate.B, 1);
                _model.Scale = new Vector2(1, 1);
                _collision.SetDeferred("disabled", false);
                _areaCollision.SetDeferred("disabled", false);
            }
        }

        if (Position.X > 2105 || Position.X < 395)
            QueueFree();
    }

	public void AreaEntered()
	{
        _doExploded = true;
        var explosionAnimation = GetNode<AnimatedSprite2D>("ExplosionAnimation");
        explosionAnimation.Visible = true;
        explosionAnimation.Play();

        _collision.SetDeferred("disabled", true);
        _areaCollision.SetDeferred("disabled", true);
        GetNode<CpuParticles2D>("ExplosionParticles").Emitting = true;
        if (G.CurrentLevel != 0)
            GetNode<AudioStreamPlayer>("ExplosionSound").Play();

        _carHalfsYMotion = -4;
	}
}