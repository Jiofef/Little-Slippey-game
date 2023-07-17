using Godot;
using System;

public partial class Level8TileMapSpawner : Node2D
{
	[Signal] public delegate void TileMapSpawnedEventHandler(int TileMapID);

    PackedScene[] _tileMaps = new PackedScene[10];
    CharacterBody2D _player;
	TileMap[] _previousTileMaps = new TileMap[2];

    private float _spawnedTileMapsNumber = 0, _tileMapsPassed = 1;
	private int _lastTileMapID;

	public override void _Ready()
	{
		_player = GetNode<CharacterBody2D>("../Player");
		for (int i = 0; i < _tileMaps.Length; i++)
			_tileMaps[i] = ResourceLoader.Load<PackedScene>("res://Content/Scenes/Level8TileMaps/TileMap" + i + ".tscn");
		_previousTileMaps[0] = GetNode<TileMap>("../TileMap");
	}

	public override void _PhysicsProcess(double delta)
	{
        if (_player.Position.X > 1280 + _spawnedTileMapsNumber * 2560)
		{
			while (true)
			{
                Random random = new Random();
                int NewTileMapID = random.Next(_tileMaps.Length);

                if (NewTileMapID != _lastTileMapID)
				{
					_lastTileMapID = NewTileMapID;
					break;
				}
            }
            TileMap tileMap = (TileMap)_tileMaps[_lastTileMapID].Instantiate();
            tileMap.Position = new Vector2(2560 + _spawnedTileMapsNumber * 2560, 0);
			AddChild(tileMap);
            _spawnedTileMapsNumber++;

            if (_previousTileMaps[1] != null)
                _previousTileMaps[1].QueueFree();

            _previousTileMaps[1] = _previousTileMaps[0];
			_previousTileMaps[0] = tileMap;
		}

		if (_player.Position.X > _tileMapsPassed * 2560)
		{
			_tileMapsPassed++;
            EmitSignal("TileMapSpawned", _lastTileMapID);
        }
	}
}
