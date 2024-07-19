using Godot;
using System;
using System.Reflection;

public partial class LevelEditor : Control
{
	Node2D _level;
	TileMap _selectedTileMap, _previousChangedGUITileMap;
	Camera2D _camera;
	private Vector2I _selectedTile = new Vector2I(0, 0);
	private int _selectedAtlas = 0, _selectedAlternativeTile = 0;
	public override void _Ready()
	{
		G.IsLevelVanilla = false;
		_level = (Node2D)ResourceLoader.Load<PackedScene>("res://Content/Scenes/Other/UserLevelLayout.tscn").Instantiate();
		_level.ProcessMode = ProcessModeEnum.Disabled;
		_level.GetNode("Level").ProcessMode = ProcessModeEnum.Disabled;
		AddChild(_level);
		_selectedTileMap = _level.GetNode<TileMap>("Level/TileMap");
		_camera = GetNode<Camera2D>("Camera2D");
		_level.GetNode<CanvasLayer>("EpicIntro").Visible = false;
		UpdateVisibleTileSet();
		UpdateVisibleAtlases();	
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 MousePos = GetGlobalMousePosition();
		Vector2I SelectedTilePos = (Vector2I)MousePos / 64 + new Vector2I(MousePos.X < 0 ? -1 : 0, MousePos.Y < 0 ? -1 : 0);
		var placeTileButton = GetNode<TextureButton>("CanvasLayer/Control/PlaceTileButton");

		if (placeTileButton.ButtonPressed)
		{
            if (Input.IsMouseButtonPressed(MouseButton.Left))
                _selectedTileMap.SetCell(0, SelectedTilePos, _selectedAtlas, _selectedTile, _selectedAlternativeTile);
            else if (Input.IsMouseButtonPressed(MouseButton.Right))
                _selectedTileMap.SetCell(0, SelectedTilePos, _selectedAtlas, new Vector2I(-1, -1));
        }

		if (Input.IsActionJustPressed("MouseWheelScrollUp"))
			SetSelectedTile(_selectedTile.X + _selectedTile.Y * GetCurrentAtlas().GetAtlasGridSize().X + 1);

        if (Input.IsActionJustPressed("MouseWheelScrollDown"))
            SetSelectedTile(_selectedTile.X + _selectedTile.Y * GetCurrentAtlas().GetAtlasGridSize().X - 1);
        _camera.Position += new Vector2(
			Input.GetActionStrength("ui_right") - Input.GetActionStrength("ui_left"), 
			Input.GetActionStrength("ui_down") - Input.GetActionStrength("ui_up"))
			* (Input.IsKeyPressed(Key.Shift) ? 30 : 15);

    }
	public void SetSelectedTile(int index)
	{
		Vector2I PreviousSelectedTile = _selectedTile;

		Vector2I AtlasSize = ((TileSetAtlasSource) _selectedTileMap.TileSet.GetSource(_selectedAtlas)).GetAtlasGridSize();
		if (index >= 0 && index < AtlasSize.X * AtlasSize.Y)
        _selectedTile.Y = Math.DivRem(index, AtlasSize.X, out _selectedTile.X);

		if (PreviousSelectedTile == _selectedTile)
			_selectedAlternativeTile = _selectedAlternativeTile + 1 < GetCurrentAtlas().GetAlternativeTilesCount(_selectedTile) ? _selectedAlternativeTile + 1 : 0;
		else _selectedAlternativeTile = 0;

    }
	public void SetSelectedAtlas(int index)
	{
		if (_selectedAtlas != index)
		{
            _selectedAtlas = index;
            UpdateVisibleTileSet();
        }
	}
	public void UpdateVisibleTileSet()
	{
		var tileButtonsContainer = GetNode("CanvasLayer/Control/TileButtonsContainer/HBoxContainer");
		var tileButtonsContainerChildren = tileButtonsContainer.GetChildren();
        for (int i = 0; i < tileButtonsContainerChildren.Count; i++)
			tileButtonsContainerChildren[i].QueueFree();


        Vector2I AtlasSize = GetCurrentAtlas().GetAtlasGridSize();

		ButtonGroup buttonGroup = new ButtonGroup();
		for (int i = 0; i < AtlasSize.X * AtlasSize.Y; i++)
        {
			Vector2I TileCoords = new Vector2I(i % AtlasSize.X, (i - i % AtlasSize.X) / AtlasSize.Y);

            if (GetCurrentAtlas().HasTile(TileCoords))
			{
                var button = new Godot.Button();
                button.CustomMinimumSize = new Vector2(32, 32);
                int crutch = i;
                button.Pressed += () => SetSelectedTile(crutch);
				button.ToggleMode = true;
                button.ButtonGroup = buttonGroup;
				button.FocusMode = FocusModeEnum.None;

				var tileMap = new TileMap();
				tileMap.TileSet = _selectedTileMap.TileSet;
				tileMap.SetCell(0, new Vector2I(0, 0), _selectedAtlas, TileCoords);
				button.Pressed += () => UpdateAlternativeTileInGUI(tileMap);
				tileMap.Position = new Vector2(8, 8);
				button.AddChild(tileMap);

                tileButtonsContainer.AddChild(button);

                if (i == 0)
                    button.ButtonPressed = true;
            }
		}


    }
	public void UpdateVisibleAtlases()
	{
		var atlasButtonsContainer = GetNode("CanvasLayer/Control/AtlasButtonsContainer/VBoxContainer");
		var AtlasButtonsContainerChildren = atlasButtonsContainer.GetChildren();
		for (int i = 0; i < AtlasButtonsContainerChildren.Count; i++)
			AtlasButtonsContainerChildren[i].QueueFree();


		int AtlasesCount = _selectedTileMap.TileSet.GetSourceCount();

		ButtonGroup buttonGroup = new ButtonGroup();
		for (int i = 0; i < AtlasesCount; i++)
		{
			var button = new Godot.Button();
			button.CustomMinimumSize = new Vector2(32, 32);
			int crutch = i;
			button.Pressed += () => SetSelectedAtlas(crutch);
			button.ButtonGroup = buttonGroup;
			button.ToggleMode = true;
			button.FocusMode = FocusModeEnum.None;
			var sprite = new Sprite2D();
			sprite.Texture = ((TileSetAtlasSource)_selectedTileMap.TileSet.GetSource(i)).Texture;
			sprite.RegionEnabled = true;
			sprite.RegionRect = new Rect2(0, 0, 16, 16);
			sprite.Position = button.CustomMinimumSize / 2;
			button.AddChild(sprite);
			atlasButtonsContainer.AddChild(button);

			if (i == 0)
				button.ButtonPressed = true;
		}
	}

	private TileSetAtlasSource GetCurrentAtlas()
	{
		return (TileSetAtlasSource)_selectedTileMap.TileSet.GetSource(_selectedAtlas);
    }
	private void UpdateAlternativeTileInGUI(TileMap tileMap)
	{
		_previousChangedGUITileMap = tileMap;
        Vector2I AtlasSize = GetCurrentAtlas().GetAtlasGridSize();
        tileMap.SetCell(0, new Vector2I(0, 0), _selectedAtlas, _selectedTile, _selectedAlternativeTile);
    }

}
