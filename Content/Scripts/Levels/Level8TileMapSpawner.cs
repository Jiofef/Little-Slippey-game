using Godot;
using System;

public partial class Level8TileMapSpawner : Node2D
{
	private CharacterBody2D _player;
	private PackedScene[] _tileMaps = new PackedScene[1];

    private float _spawnedTileMapsNumber = 0;

	public override void _Ready()
	{
		_player = GetNode<CharacterBody2D>("../Player");
		for (int i = 0; i < _tileMaps.Length; i++)
			_tileMaps[i] = ResourceLoader.Load<PackedScene>("res://Content/Scenes/Other/Level8Tilemap" + i + ".tscn");
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_player.Position.X > 640 + _spawnedTileMapsNumber * 1280)
		{
			_spawnedTileMapsNumber++;
			Random random = new Random();
			TileMap tileMap = (TileMap)_tileMaps[random.Next(_tileMaps.Length)].Instantiate();
			tileMap.Position = new Vector2(_spawnedTileMapsNumber * 1280, 0);
		}
	}
}
