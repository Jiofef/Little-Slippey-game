using Godot;
using System;

public partial class Level8TileMapSpawner : Node2D
{
    PackedScene[] _tileMaps = new PackedScene[5];
    CharacterBody2D _player;
	TileMap[] _previousTileMaps = new TileMap[2];

    private float _spawnedTileMapsNumber = 0;

	public override void _Ready()
	{
		_player = GetNode<CharacterBody2D>("../Player");
		for (int i = 0; i < _tileMaps.Length; i++)
			_tileMaps[i] = ResourceLoader.Load<PackedScene>("res://Content/Scenes/Level8TileMaps/TileMap" + (i + 1) + ".tscn");
		_previousTileMaps[0] = GetNode<TileMap>("../TileMap");
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_player.Position.X > 1440 + _spawnedTileMapsNumber * 2560)
		{
			Random random = new Random();
			TileMap tileMap = (TileMap)_tileMaps[random.Next(_tileMaps.Length)].Instantiate();
            tileMap = (TileMap)_tileMaps[4].Instantiate();
            tileMap.Position = new Vector2(2560 + _spawnedTileMapsNumber * 2560, 0);
			AddChild(tileMap);
            _spawnedTileMapsNumber++;

            if (_previousTileMaps[1] != null)
                _previousTileMaps[1].QueueFree();

            _previousTileMaps[1] = _previousTileMaps[0];
			_previousTileMaps[0] = tileMap;
		}
	}
}
