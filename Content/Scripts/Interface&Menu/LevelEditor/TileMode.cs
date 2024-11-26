using Godot;
using System;

public partial class TileMode : Control
{
    public LevelEditor L;

    private enum TileInstrumentEnum { Brush, Rectangle, Filling, SmartFilling }
    private TileInstrumentEnum TileInstrument = TileInstrumentEnum.Brush;

    private Rect2I _tileRect = new Rect2I();
    private Vector2I _selectedTile = new Vector2I(0, 0), _mouseTilePos, _mouseLastFrameTilePos;
    private TileMap _selectedTileMap, _previousChangedGUITileMap, _assistiveTileMap, _erasingAssistiveTileMap;
    private Godot.Collections.Array<TileMap> _allTheTileMaps = new Godot.Collections.Array<TileMap>();
    private int _selectedLayer = 0, _selectedAtlas = 0, _selectedAlternativeTile = 0, _previousTileIndex = 0, _tileChangeProbability = 1;
    private bool _isRectStarted = false, _isRectErasing = false;

    public override void _Ready()
    {
        _selectedTileMap = L._level.GetNode<TileMap>("Level/TileMap");
        _assistiveTileMap = L.GetNode<TileMap>("AssistiveTileMap");
        _erasingAssistiveTileMap = L.GetNode<TileMap>("ErasingAssistiveTileMap");
        _assistiveTileMap.TileSet = _selectedTileMap.TileSet;

        UpdateVisibleTileMapsLayers();
        UpdateVisibleAtlases();
        UpdateVisibleTileSet();
        UpdateVisibleTileMaps();
    }

    public override void _PhysicsProcess(double delta)
    {
        _mouseTilePos = new Vector2I((int)((L._globalMousePos.X - _selectedTileMap.GlobalPosition.X) / (16 * _selectedTileMap.Scale.X)), (int)((L._globalMousePos.Y - _selectedTileMap.GlobalPosition.Y) / (16 * _selectedTileMap.Scale.Y))) + new Vector2I(L._globalMousePos.X < 0 ? -1 : 0, L._globalMousePos.Y < 0 ? -1 : 0);

        //if (Input.IsActionJustPressed("MouseWheelClick"))
        //{
        //   SetSelectedTile(_previousTileIndex);
        //    UpdateAlternativeTileInGUI(_previousChangedGUITileMap, _selectedAlternativeTile);
        //}

        if (_mouseLastFrameTilePos != _mouseTilePos || Input.IsActionJustPressed("MouseClick") || Input.IsActionJustReleased("MouseClick"))
        {
            _mouseLastFrameTilePos = _mouseTilePos;
            _assistiveTileMap.Clear();
            _erasingAssistiveTileMap.Clear();
            if (Input.IsMouseButtonPressed(MouseButton.Right) && !Input.IsMouseButtonPressed(MouseButton.Left))
                _erasingAssistiveTileMap.SetCell(0, _mouseTilePos, 0, Vector2I.Zero);
            else
                _assistiveTileMap.SetCell(0, _mouseTilePos, _selectedAtlas, _selectedTile, _selectedAlternativeTile);
            Random random = new Random();
            switch (TileInstrument)
            {
                case TileInstrumentEnum.Brush:
                    if (L._screenButton.ButtonPressed && random.Next(_tileChangeProbability) == 0)
                    {
                        if (Input.IsMouseButtonPressed(MouseButton.Left))
                            _selectedTileMap.SetCell(_selectedLayer, _mouseTilePos, _selectedAtlas, _selectedTile, _selectedAlternativeTile);
                        else if (Input.IsMouseButtonPressed(MouseButton.Right))
                            _selectedTileMap.SetCell(_selectedLayer, _mouseTilePos, _selectedAtlas, new Vector2I(-1, -1));
                    }
                    break;

                case TileInstrumentEnum.Rectangle:
                    Rect2I GetBlockRect()
                    {
                        Rect2I BlockRect = new Rect2I();
                        BlockRect = _tileRect;
                        BlockRect.Position = new Vector2I(BlockRect.Size.X > 0 ? BlockRect.Position.X : BlockRect.Position.X + BlockRect.Size.X, BlockRect.Size.Y > 0 ? BlockRect.Position.Y : BlockRect.Position.Y + BlockRect.Size.Y);
                        BlockRect.Size = new Vector2I(Math.Abs(BlockRect.Size.X), Math.Abs(BlockRect.Size.Y));
                        return BlockRect;
                    }
                    if (L._screenButton.ButtonPressed)
                    {
                        if (!_isRectStarted)
                        {
                            _isRectStarted = true;
                            _tileRect.Position = _mouseTilePos;
                            _isRectErasing = Input.IsMouseButtonPressed(MouseButton.Right);
                        }
                        _tileRect.Size = _mouseTilePos - _tileRect.Position;
                        Rect2I BlockRect = GetBlockRect();
                        for (int i = 0; i < BlockRect.Size.Y + 1; i++)
                            for (int j = 0; j < BlockRect.Size.X + 1; j++)
                            {
                                if (!_isRectErasing)
                                    _assistiveTileMap.SetCell(0, BlockRect.Position + new Vector2I(j, i), _selectedAtlas, _selectedTile, _selectedAlternativeTile);
                                else
                                    _erasingAssistiveTileMap.SetCell(0, BlockRect.Position + new Vector2I(j, i), 0, Vector2I.Zero);
                            }
                    }
                    else if (_isRectStarted)
                    {
                        _isRectStarted = false;

                        Rect2I BlockRect = GetBlockRect();
                        for (int i = 0; i < BlockRect.Size.Y + 1; i++)
                            for (int j = 0; j < BlockRect.Size.X + 1; j++)
                                if (random.Next(_tileChangeProbability) == 0)
                                    _selectedTileMap.SetCell(_selectedLayer, BlockRect.Position + new Vector2I(j, i), _selectedAtlas, _isRectErasing ? new Vector2I(-1, -1) : _selectedTile, _selectedAlternativeTile);

                        _isRectErasing = false;
                        _tileRect = new Rect2I();
                    }
                    break;

                case TileInstrumentEnum.Filling:
                    {
                        Rect2I UsedRect = _selectedTileMap.GetUsedRect();

                        Vector2I SelectedCellAtlasPos = _selectedTileMap.GetCellAtlasCoords(_selectedLayer, _mouseTilePos);
                        int SelectedCellAltTile = _selectedTileMap.GetCellAlternativeTile(_selectedLayer, _mouseTilePos),
                            SelectedCellAtlas = _selectedTileMap.GetCellSourceId(_selectedLayer, _mouseTilePos);

                        Vector2I TilePos;
                        if (_mouseTilePos.X >= UsedRect.Position.X && _mouseTilePos.X < UsedRect.Position.X + UsedRect.Size.X && _mouseTilePos.Y >= UsedRect.Position.Y && _mouseTilePos.Y < UsedRect.Position.Y + UsedRect.Size.Y)
                        {
                            if (L._screenButton.ButtonPressed && Input.IsActionJustPressed("MouseClick"))
                            {
                                bool IsFillingErasing = Input.IsActionJustPressed("MouseClick") && Input.IsMouseButtonPressed(MouseButton.Right) && !Input.IsMouseButtonPressed(MouseButton.Left);
                                for (int i = 0; i < UsedRect.Size.Y; i++)
                                    for (int j = 0; j < UsedRect.Size.X; j++)
                                    {
                                        TilePos = UsedRect.Position + new Vector2I(j, i);
                                        if (random.Next(_tileChangeProbability) == 0)
                                            if (_selectedTileMap.GetCellSourceId(_selectedLayer, TilePos) == SelectedCellAtlas && _selectedTileMap.GetCellAtlasCoords(_selectedLayer, TilePos) == SelectedCellAtlasPos && _selectedTileMap.GetCellAlternativeTile(_selectedLayer, TilePos) == SelectedCellAltTile)
                                                _selectedTileMap.SetCell(_selectedLayer, TilePos, _selectedAtlas, IsFillingErasing ? new Vector2I(-1, -1) : _selectedTile, _selectedAlternativeTile);
                                    }
                            }
                            else
                            {
                                for (int i = 0; i < UsedRect.Size.Y; i++)
                                    for (int j = 0; j < UsedRect.Size.X; j++)
                                    {
                                        TilePos = UsedRect.Position + new Vector2I(j, i);
                                        if (_selectedTileMap.GetCellSourceId(_selectedLayer, TilePos) == SelectedCellAtlas && _selectedTileMap.GetCellAtlasCoords(_selectedLayer, TilePos) == SelectedCellAtlasPos && _selectedTileMap.GetCellAlternativeTile(_selectedLayer, TilePos) == SelectedCellAltTile)
                                            _assistiveTileMap.SetCell(0, TilePos, _selectedAtlas, _selectedTile, _selectedAlternativeTile);
                                    }
                            }
                        }
                    }
                    break;
                case TileInstrumentEnum.SmartFilling:
                    //YEAH TELL EVERYONE HOW JIOFEF DUPLICATES THE CODE PIECES, I KNOW YOU WANT TO
                    {
                        Rect2I UsedRect = _selectedTileMap.GetUsedRect();

                        Vector2I SelectedCellAtlasPos = _selectedTileMap.GetCellAtlasCoords(_selectedLayer, _mouseTilePos);
                        int SelectedCellAltTile = _selectedTileMap.GetCellAlternativeTile(_selectedLayer, _mouseTilePos),
                            SelectedCellAtlas = _selectedTileMap.GetCellSourceId(_selectedLayer, _mouseTilePos);
                        bool IsVectorInsideUsedRect(Vector2I vector) => vector.X >= UsedRect.Position.X && vector.X < UsedRect.Position.X + UsedRect.Size.X && vector.Y >= UsedRect.Position.Y && vector.Y < UsedRect.Position.Y + UsedRect.Size.Y;
                        if (IsVectorInsideUsedRect(_mouseTilePos))
                        {
                            if (L._screenButton.ButtonPressed && Input.IsActionJustPressed("MouseClick"))
                            {
                                bool IsFillingErasing = Input.IsActionJustPressed("MouseClick") && Input.IsMouseButtonPressed(MouseButton.Right) && !Input.IsMouseButtonPressed(MouseButton.Left);
                                if ((_selectedTileMap.GetCellSourceId(_selectedLayer, _mouseTilePos) != _selectedAtlas || _selectedTileMap.GetCellAtlasCoords(_selectedLayer, _mouseTilePos) != _selectedTile || _selectedTileMap.GetCellAlternativeTile(_selectedLayer, _mouseTilePos) != _selectedAlternativeTile || IsFillingErasing) && (!IsFillingErasing || _selectedTileMap.GetCellAtlasCoords(_selectedLayer, _mouseTilePos) != new Vector2I(-1, -1)))
                                    Recurse(_mouseTilePos);
                                void Recurse(Vector2I position)
                                {
                                    _selectedTileMap.SetCell(_selectedLayer, position, _selectedAtlas, IsFillingErasing ? new Vector2I(-1, -1) : _selectedTile, _selectedAlternativeTile);
                                    Godot.Collections.Array<Vector2I> NeighbourCells = _selectedTileMap.GetSurroundingCells(position);
                                    for (int i = 0; i < NeighbourCells.Count; i++)
                                        if (random.Next(_tileChangeProbability) == 0)
                                            if (_selectedTileMap.GetCellSourceId(_selectedLayer, NeighbourCells[i]) == SelectedCellAtlas && _selectedTileMap.GetCellAtlasCoords(_selectedLayer, NeighbourCells[i]) == SelectedCellAtlasPos && _selectedTileMap.GetCellAlternativeTile(_selectedLayer, NeighbourCells[i]) == SelectedCellAltTile && IsVectorInsideUsedRect(NeighbourCells[i]))
                                                Recurse(NeighbourCells[i]);
                                }
                            }
                            else
                            {

                                if (_selectedTileMap.GetCellSourceId(_selectedLayer, _mouseTilePos) != _selectedAtlas || _selectedTileMap.GetCellAtlasCoords(_selectedLayer, _mouseTilePos) != _selectedTile || _selectedTileMap.GetCellAlternativeTile(_selectedLayer, _mouseTilePos) != _selectedAlternativeTile)
                                    Recurse(_mouseTilePos);
                                void Recurse(Vector2I position)
                                {
                                    _assistiveTileMap.SetCell(_selectedLayer, position, _selectedAtlas, _selectedTile, _selectedAlternativeTile);
                                    Godot.Collections.Array<Vector2I> NeighbourCells = _selectedTileMap.GetSurroundingCells(position);
                                    for (int i = 0; i < NeighbourCells.Count; i++)
                                        if (_selectedTileMap.GetCellSourceId(_selectedLayer, NeighbourCells[i]) == SelectedCellAtlas && _selectedTileMap.GetCellAtlasCoords(_selectedLayer, NeighbourCells[i]) == SelectedCellAtlasPos && _selectedTileMap.GetCellAlternativeTile(_selectedLayer, NeighbourCells[i]) == SelectedCellAltTile && IsVectorInsideUsedRect(NeighbourCells[i])
                                            && _assistiveTileMap.GetCellAtlasCoords(0, NeighbourCells[i]) == new Vector2I(-1, -1))
                                            Recurse(NeighbourCells[i]);
                                }
                            }
                        }
                    }
                    break;
            }
        }
    }

    public void SetSelectedTile(int index)
    {
        if (_previousTileIndex != index && _previousChangedGUITileMap != null)
            _previousChangedGUITileMap.SetCell(0, new Vector2I(0, 0), _selectedAtlas, _selectedTile, 0);
        _previousTileIndex = index;

        Vector2I PreviousSelectedTile = _selectedTile;

        Vector2I AtlasSize = GetCurrentAtlas().GetAtlasGridSize();
        if (index >= 0 && index < AtlasSize.X * AtlasSize.Y)
            _selectedTile.Y = Math.DivRem(index, AtlasSize.X, out _selectedTile.X);

        if (PreviousSelectedTile == _selectedTile)
            _selectedAlternativeTile = _selectedAlternativeTile + 1 < GetCurrentAtlas().GetAlternativeTilesCount(_selectedTile) ? _selectedAlternativeTile + 1 : 0;
        else _selectedAlternativeTile = 0;
        _assistiveTileMap.TileSet = _selectedTileMap.TileSet;
    }
    public void SetSelectedAtlas(int index)
    {
        if (_selectedAtlas != index)
        {
            _selectedAtlas = index;
            UpdateVisibleTileSet();
        }
    }
    public void SetSelectedTileMap(int index)
    {
        _selectedLayer = 0;
        _selectedTileMap = _allTheTileMaps[index];
        UpdateVisibleTileMapsLayers();
        UpdateVisibleAtlases();
        UpdateVisibleTileSet();
        _assistiveTileMap.TileSet = _selectedTileMap.TileSet;
        _assistiveTileMap.GlobalTransform = _erasingAssistiveTileMap.GlobalTransform = _selectedTileMap.GlobalTransform;
    }
    public void SetTileInstrument(int index)
    {
        TileInstrument = (TileInstrumentEnum)index;
    }
    public void SetTileChangeProbability(int value)
    {
        _tileChangeProbability = value;
    }
    public void UpdateVisibleTileSet()
    {
        _selectedTile = new Vector2I(0, 0);
        _previousChangedGUITileMap = null;
        var tileButtonsContainer = GetNode("TileButtonsContainer/HBoxContainer");

        L.KILLCHILDREN(tileButtonsContainer);

        Vector2I AtlasSize = GetCurrentAtlas().GetAtlasGridSize();

        ButtonGroup buttonGroup = new ButtonGroup();
        Texture2D texture = GD.Load<CompressedTexture2D>("res://Content/Sprites/Interface/LevelEditor/BigButton.png");
        for (int i = 0; i < AtlasSize.X * AtlasSize.Y; i++)
        {
            Vector2I TileCoords;
            TileCoords.Y = Math.DivRem(i, AtlasSize.X, out TileCoords.X);
            if (GetCurrentAtlas().HasTile(TileCoords))
            {
                var button = MakeAButton(new Vector2(24, 24), buttonGroup, texture);
                int crutch = i;
                button.Pressed += () => SetSelectedTile(crutch);

                var tileMap = new TileMap();
                tileMap.TileSet = _selectedTileMap.TileSet;
                tileMap.SetCell(0, new Vector2I(0, 0), _selectedAtlas, TileCoords);
                button.Pressed += () => UpdateAlternativeTileInGUI(tileMap, _selectedAlternativeTile);
                tileMap.Position = new Vector2(4, 4);
                button.AddChild(tileMap);

                tileButtonsContainer.AddChild(button);

                if (i == 0)
                {
                    _previousChangedGUITileMap = tileMap;
                    UpdateAlternativeTileInGUI(tileMap, _selectedAlternativeTile);
                    button.ButtonPressed = true;
                }
            }
        }
        if (_previousTileIndex != 0)
            SetSelectedTile(0);
        _selectedAlternativeTile = 0;
    }
    public void UpdateVisibleAtlases()
    {
        var atlasButtonsContainer = GetNode("AtlasButtonsContainer/VBoxContainer");

        L.KILLCHILDREN(atlasButtonsContainer);


        int AtlasesCount = _selectedTileMap.TileSet.GetSourceCount();

        ButtonGroup buttonGroup = new ButtonGroup();
        Texture2D texture = GD.Load<CompressedTexture2D>("res://Content/Sprites/Interface/LevelEditor/BigButton.png");
        for (int i = 0; i < AtlasesCount; i++)
        {
            var button = MakeAButton(new Vector2(24, 24), buttonGroup, texture);
            int crutch = i;
            button.Pressed += () => SetSelectedAtlas(crutch);

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
    public void UpdateVisibleTileMaps()
    {
        var TileMapsButtonsContainer = GetNode("TileMapsButtonsContainer/VBoxContainer");

        L.KILLCHILDREN(TileMapsButtonsContainer);

        UpdateAllTheTileMaps();

        ButtonGroup buttonGroup = new ButtonGroup();
        Texture2D texture = GD.Load<CompressedTexture2D>("res://Content/Sprites/Interface/LevelEditor/BigButton.png");
        for (int i = 0; i < _allTheTileMaps.Count; i++)
        {
            var button = MakeAButton(new Vector2(24, 24), buttonGroup, texture);
            int crutch = i;
            button.Pressed += () => SetSelectedTileMap(crutch);

            TileMapsButtonsContainer.AddChild(button);

            if (i == 0)
                button.ButtonPressed = true;
        }
    }
    public void UpdateVisibleTileMapsLayers()
    {
        var TileMapsLayersContainer = GetNode("TileMapsLayersContainer/VBoxContainer");

        L.KILLCHILDREN(TileMapsLayersContainer);

        int LayersCount = _selectedTileMap.GetLayersCount();

        ButtonGroup buttonGroup = new ButtonGroup();
        Texture2D texture = GD.Load<CompressedTexture2D>("res://Content/Sprites/Interface/LevelEditor/SmallButton.png");
        for (int i = 0; i < LayersCount; i++)
        {
            var button = MakeAButton(new Vector2(16, 16), buttonGroup, texture);
            int crutch = i;
            button.Pressed += () => _selectedLayer = crutch;

            TileMapsLayersContainer.AddChild(button);

            if (i == 0)
                button.ButtonPressed = true;
        }
    }
    private void UpdateAlternativeTileInGUI(TileMap tileMap, int alternativeTileIndex)
    {
        _previousChangedGUITileMap = tileMap;
        Vector2I AtlasSize = GetCurrentAtlas().GetAtlasGridSize();
        tileMap.SetCell(0, new Vector2I(0, 0), _selectedAtlas, _selectedTile, alternativeTileIndex);
    }
    private void UpdateAllTheTileMaps()
    {
        L.UpdateAllNodesArray();
        _allTheTileMaps.Clear();
        foreach (Node node in L._allTheNodes)
        {
            if (node is TileMap)
                _allTheTileMaps.Add((TileMap)node);
        }
    }
    private TileSetAtlasSource GetCurrentAtlas()
    {
        return (TileSetAtlasSource)_selectedTileMap.TileSet.GetSource(_selectedAtlas);
    }
    private EnhancedButton MakeAButton(Vector2 minimumSize = new Vector2(), ButtonGroup buttonGroup = null, Texture2D normalTexture = null)
    {
        EnhancedButton button = (EnhancedButton)GD.Load<PackedScene>("res://Content/Scenes/Interface&Menu/Button.tscn").Instantiate();
        button.TextureNormal = normalTexture;
        button.CustomMinimumSize = minimumSize;
        button.ToggleMode = true;
        if (buttonGroup != null)
            button.ButtonGroup = buttonGroup;

        return button;
    }
}
