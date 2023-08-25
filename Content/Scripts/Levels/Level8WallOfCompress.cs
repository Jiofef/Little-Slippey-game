using Godot;

public partial class Level8WallOfCompress : Node2D
{
	CharacterBody2D _player;

    private readonly float[] _tileMapWallSpeedMotificators = { 1.4f, 1, 1, 0.85f, 1.1f, 0.6f, 0.8f, 0.9f, 0.5f, 1.25f, };
    private float _wallDefaultSpeed = 3, _wallSpeed, _wallSpeedSmoothedModificator = 1, _wallSpeedHardModificator = 1;
	public override void _Ready()
	{
		_player = GetNode<CharacterBody2D>("../Player");
	}

	public override void _PhysicsProcess(double delta)
	{
		_wallSpeedSmoothedModificator += (_wallSpeedHardModificator - _wallSpeedSmoothedModificator) / 30;
		_wallSpeed = (_wallDefaultSpeed + (_player.GlobalPosition.X - GlobalPosition.X) / 1920 * 4) * _wallSpeedSmoothedModificator;
		Translate(new Vector2(_wallSpeed, 0));
	}

	public void UpdateWallSpeedModificator(int value)
	{
        _wallSpeedHardModificator = _tileMapWallSpeedMotificators[value];
	}
}