using Godot;
using System;

public partial class LevelEditor : Control
{
	Node2D _level;
	TileMap _selectedTileMap;
	Camera2D _camera;
	private Vector2I _selectedTile = new Vector2I(0, 0);
	private int _selectedAtlas = 1;
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
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 MousePos = GetGlobalMousePosition();
		Vector2I SelectedTilePos = (Vector2I)MousePos / 64 + new Vector2I(MousePos.X < 0 ? -1 : 0, MousePos.Y < 0 ? -1 : 0);

		if (Input.IsMouseButtonPressed(MouseButton.Left))
			_selectedTileMap.SetCell(0, SelectedTilePos, 1, _selectedTile);
        else if (Input.IsMouseButtonPressed(MouseButton.Right))
            _selectedTileMap.SetCell(0, SelectedTilePos, _selectedAtlas, new Vector2I(-1, -1));


		if (Input.IsActionJustPressed("MouseWheelScrollUp"))
			SetSelectedTile(_selectedTile.X + _selectedTile.Y * ((TileSetAtlasSource)_selectedTileMap.TileSet.GetSource(_selectedAtlas)).GetAtlasGridSize().X + 1);

        if (Input.IsActionJustPressed("MouseWheelScrollDown"))
            SetSelectedTile(_selectedTile.X + _selectedTile.Y * ((TileSetAtlasSource)_selectedTileMap.TileSet.GetSource(_selectedAtlas)).GetAtlasGridSize().X - 1);
        _camera.Position += new Vector2(
			Input.GetActionStrength("ui_right") - Input.GetActionStrength("ui_left"), 
			Input.GetActionStrength("ui_down") - Input.GetActionStrength("ui_up"))
			* (Input.IsKeyPressed(Key.Shift) ? 30 : 15);

    }
	public void SetSelectedTile(int index)
	{
		Vector2I AtlasSize = ((TileSetAtlasSource) _selectedTileMap.TileSet.GetSource(_selectedAtlas)).GetAtlasGridSize();
		if (index >= 0 && index < AtlasSize.X * AtlasSize.Y)
        _selectedTile.Y = Math.DivRem(index, AtlasSize.X, out _selectedTile.X);
    }
	public void UpdateVisibleTileSet()
	{
        Vector2I AtlasSize = ((TileSetAtlasSource)_selectedTileMap.TileSet.GetSource(_selectedAtlas)).GetAtlasGridSize();
		for (int i = 0; i < AtlasSize.X * AtlasSize.Y; i++)
		{
			var button = new Godot.Button();
			button.CustomMinimumSize = new Vector2(32, 32);
			int crutch = i;
			button.Pressed += () => SetSelectedTile(crutch);
			GetNode("CanvasLayer/Control/ScrollContainer/HBoxContainer").AddChild(button);
		}
    }
}
