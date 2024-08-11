using Godot;
using System;
using System.Linq;

public partial class LevelEditor : Control
{
    [Signal] public delegate void NodeMovedByMouseEventHandler();
    NodeMovedByMouseEventHandler _lastConnectedPositionPropertyUpdate;
    NodeMovedByMouseEventHandler _lastConnectedGlobalPositionPropertyUpdate;
    Node2D _level;
    Camera2D _camera;
    Node[] _allTheNodes = new Node[0];
    TextureButton _screenButton = new TextureButton();

    enum EditorMode { TileMode, NodeMode, CodeMode }
    EditorMode _editorMode = EditorMode.TileMode;

    private bool _isMiddleButtonPressed = false;
    private string _mapPath = "";

    public override void _Ready()
    {
        _mapPath = (string)G.InGameTransitiveValue;
        G.IsLevelVanilla = false;
        _level = (Node2D)ResourceLoader.Load<PackedScene>(_mapPath).Instantiate();
        if (_editorCrutches[0])
            _level.ProcessMode = ProcessModeEnum.Disabled;
        if (_editorCrutches[1])
            _level.GetNode<CanvasLayer>("Pause").Visible = false;
        if (_editorCrutches[2])
            _level.GetNode<CanvasLayer>("EpicIntro").Visible = false;

        GetNode("LevelContainer").AddChild(_level);
        _selectedTileMap = _level.GetNode<TileMap>("Level/TileMap");
        _camera = GetNode<Camera2D>("Camera2D");
        _assistiveTileMap = GetNode<TileMap>("AssistiveTileMap");
        _erasingAssistiveTileMap = GetNode<TileMap>("ErasingAssistiveTileMap");
        _screenButton = GetNode<TextureButton>("CanvasLayer/ScreenButton");
        UpdateVisibleTileMapsLayers();
        UpdateVisibleAtlases();
        UpdateVisibleTileSet();
        UpdateVisibleTileMaps();
        _assistiveTileMap.TileSet = _selectedTileMap.TileSet;
    }

    public override void _PhysicsProcess(double delta)
    {
        _camera.Position += new Vector2(
         Input.GetActionStrength("ui_right") - Input.GetActionStrength("ui_left"),
        Input.GetActionStrength("ui_down") - Input.GetActionStrength("ui_up"))
        * (Input.IsKeyPressed(Key.Shift) ? 30 : 15
        );

        Vector2 MousePos = GetGlobalMousePosition();
        switch (_editorMode)
        {
            case EditorMode.TileMode:
                _mouseTilePos = new Vector2I((int)(MousePos.X / (16 * _selectedTileMap.Scale.X)), (int)(MousePos.Y / (16 * _selectedTileMap.Scale.Y))) + new Vector2I(MousePos.X < 0 ? -1 : 0, MousePos.Y < 0 ? -1 : 0);

                if (Input.IsMouseButtonPressed(MouseButton.Middle))
                {
                    if (!_isMiddleButtonPressed)
                    {
                        SetSelectedTile(_previousTileIndex);
                        UpdateAlternativeTileInGUI(_previousChangedGUITileMap, _selectedAlternativeTile);
                        _isMiddleButtonPressed = true;
                    }
                }
                else
                    _isMiddleButtonPressed = false;

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
                    switch (_tileInstrument)
                    {
                        case TileInstrument.Brush:
                            if (_screenButton.ButtonPressed && random.Next(_tileChangeProbability) == 0)
                            {
                                if (Input.IsMouseButtonPressed(MouseButton.Left))
                                    _selectedTileMap.SetCell(_selectedLayer, _mouseTilePos, _selectedAtlas, _selectedTile, _selectedAlternativeTile);
                                else if (Input.IsMouseButtonPressed(MouseButton.Right))
                                    _selectedTileMap.SetCell(_selectedLayer, _mouseTilePos, _selectedAtlas, new Vector2I(-1, -1));
                            }
                            break;

                        case TileInstrument.Rectangle:
                            Rect2I GetBlockRect()
                            {
                                Rect2I BlockRect = new Rect2I();
                                BlockRect = _tileRect;
                                BlockRect.Position = new Vector2I(BlockRect.Size.X > 0 ? BlockRect.Position.X : BlockRect.Position.X + BlockRect.Size.X, BlockRect.Size.Y > 0 ? BlockRect.Position.Y : BlockRect.Position.Y + BlockRect.Size.Y);
                                BlockRect.Size = new Vector2I(Math.Abs(BlockRect.Size.X), Math.Abs(BlockRect.Size.Y));
                                return BlockRect;
                            }
                            if (_screenButton.ButtonPressed)
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

                        case TileInstrument.Filling:
                            {
                                Rect2I UsedRect = _selectedTileMap.GetUsedRect();

                                Vector2I SelectedCellAtlasPos = _selectedTileMap.GetCellAtlasCoords(_selectedLayer, _mouseTilePos);
                                int SelectedCellAltTile = _selectedTileMap.GetCellAlternativeTile(_selectedLayer, _mouseTilePos),
                                    SelectedCellAtlas = _selectedTileMap.GetCellSourceId(_selectedLayer, _mouseTilePos);

                                Vector2I TilePos;
                                if (_mouseTilePos.X >= UsedRect.Position.X && _mouseTilePos.X < UsedRect.Position.X + UsedRect.Size.X && _mouseTilePos.Y >= UsedRect.Position.Y && _mouseTilePos.Y < UsedRect.Position.Y + UsedRect.Size.Y)
                                {
                                    if (_screenButton.ButtonPressed && Input.IsActionJustPressed("MouseClick"))
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
                        case TileInstrument.SmartFilling:
                            //YEAH TELL EVERYONE HOW JIOFEF DUPLICATES THE CODE PIECES, I KNOW YOU WANT TO
                            {
                                Rect2I UsedRect = _selectedTileMap.GetUsedRect();

                                Vector2I SelectedCellAtlasPos = _selectedTileMap.GetCellAtlasCoords(_selectedLayer, _mouseTilePos);
                                int SelectedCellAltTile = _selectedTileMap.GetCellAlternativeTile(_selectedLayer, _mouseTilePos),
                                    SelectedCellAtlas = _selectedTileMap.GetCellSourceId(_selectedLayer, _mouseTilePos);
                                bool IsVectorInsideUsedRect(Vector2I vector) => vector.X >= UsedRect.Position.X && vector.X < UsedRect.Position.X + UsedRect.Size.X && vector.Y >= UsedRect.Position.Y && vector.Y < UsedRect.Position.Y + UsedRect.Size.Y;
                                if (IsVectorInsideUsedRect(_mouseTilePos))
                                {
                                    if (_screenButton.ButtonPressed && Input.IsActionJustPressed("MouseClick"))
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
                break;
            case EditorMode.NodeMode:
                if (_screenButton.ButtonPressed && Input.IsActionJustPressed("MouseClick"))
                {
                    //foreach (GodotObject node in _allTheNodes)
                    //{
                    //    if (node is Node2D)
                    //    {
                    //        var node2D = (Node2D)node;
                    //        if (node2D.Position.X < MousePos.X && node2D.Position.Y < MousePos.Y && MousePos.X < node2D.GlobalPosition.X  + 50 && MousePos.Y < node2D.GlobalPosition.Y + 50)
                    //            _selectedNode = node2D;
                    //    }
                    //    else if (node is Control)
                    //    {
                    //        var control = (Control)node;
                    //        if (control.Position.X < MousePos.X && control.Position.Y < MousePos.Y && MousePos.X < control.GlobalPosition.X + control.Size.X * control.Scale.X && MousePos.Y < control.GlobalPosition.Y + control.Size.Y * control.Scale.Y)
                    //            _selectedNode = control;
                    //    }
                    //}
                    //for (int i = 0; i < allTheNodes.Length; i++)
                    //{
                    //    GodotObject node = allTheNodes[i];
                    //    if (node is Node2D)
                    //    {
                    //        var node2D = (Node2D)node;
                    //        if (node2D.GlobalPosition.X < MousePos.X && node2D.GlobalPosition.Y < MousePos.Y && MousePos.X < node2D.GlobalPosition.X + 50 && MousePos.Y < node2D.GlobalPosition.Y + 50)
                    //        {
                    //            _selectedNode = node2D;
                    //            GD.Print(_selectedNode);
                    //        }
                    //    }
                    //    else if (node is Control)
                    //    {
                    //        var control = (Control)node;
                    //        if (control.Position.X < MousePos.X && control.Position.Y < MousePos.Y && MousePos.X < control.GlobalPosition.X + control.Size.X * control.Scale.X && MousePos.Y < control.GlobalPosition.Y + control.Size.Y * control.Scale.Y)
                    //            _selectedNode = control;
                    //    }
                    //}
                }
                else if (_screenButton.ButtonPressed && Input.IsMouseButtonPressed(MouseButton.Left) && _selectedNode != null)
                {
                    if (_selectedNode is Node2D)
                        ((Node2D)_selectedNode).Position -= _mouseLastFramePos - MousePos;
                    else if (_selectedNode is Control)
                        ((Control)_selectedNode).Position -= _mouseLastFramePos - MousePos;
                    EmitSignal("NodeMovedByMouse");
                }
                break;
        }

        _mouseLastFramePos = MousePos;
    }

    public void TestLevel()
    {
        GetTree().ChangeSceneToFile(_mapPath);
    }

    public void SetEditorMode(int value)
    {
        _editorMode = (EditorMode)value;
        GetNode<Control>("CanvasLayer/TileModeGUI").Visible = _editorMode == EditorMode.TileMode;
        GetNode<Control>("CanvasLayer/NodeModeGUI").Visible = _editorMode == EditorMode.NodeMode;
        GetNode<Control>("CanvasLayer/CodeModeGUI").Visible = _editorMode == EditorMode.CodeMode;
        GetNode<TileMap>("AssistiveTileMap").Visible = _editorMode == EditorMode.TileMode;
        GetNode<TileMap>("ErasingAssistiveTileMap").Visible = _editorMode == EditorMode.TileMode;
        UpdateAllNodesArray();
        if (_editorMode == EditorMode.NodeMode)
            UpdateVisibleNodesButtons();
    }

    private void KILLCHILDREN(Node node)
    {
        foreach (Node child in node.GetChildren())
            child.QueueFree();
    }

    // TileMode Section
    TileMap _selectedTileMap, _previousChangedGUITileMap, _assistiveTileMap, _erasingAssistiveTileMap;
    Rect2I _tileRect = new Rect2I();
    Godot.Collections.Array<TileMap> _allTheTileMaps = new Godot.Collections.Array<TileMap>();

    private Vector2I _selectedTile = new Vector2I(0, 0), _mouseTilePos, _mouseLastFrameTilePos;
    enum TileInstrument { Brush, Rectangle, Filling, SmartFilling }
    TileInstrument _tileInstrument = TileInstrument.Brush;

    private int _selectedLayer = 0, _selectedAtlas = 0, _selectedAlternativeTile = 0, _previousTileIndex = 0, _tileChangeProbability = 1;


    private bool _isRectStarted = false, _isRectErasing = false;
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
    }
    public void SetTileInstrument(int index)
    {
        _tileInstrument = (TileInstrument)index;
    }
    public void SetTileChangeProbability(int value)
    {
        _tileChangeProbability = value;
    }
    public void UpdateVisibleTileSet()
    {
        _selectedTile = new Vector2I(0, 0);
        _previousChangedGUITileMap = null;
        var tileButtonsContainer = GetNode("CanvasLayer/TileModeGUI/TileButtonsContainer/HBoxContainer");

        KILLCHILDREN(tileButtonsContainer);

        Vector2I AtlasSize = GetCurrentAtlas().GetAtlasGridSize();

        ButtonGroup buttonGroup = new ButtonGroup();
        for (int i = 0; i < AtlasSize.X * AtlasSize.Y; i++)
        {
            Vector2I TileCoords;
            TileCoords.Y = Math.DivRem(i, AtlasSize.X, out TileCoords.X);
            if (GetCurrentAtlas().HasTile(TileCoords))
            {
                var button = MakeAButton(new Vector2(32, 32), buttonGroup);
                int crutch = i;
                button.Pressed += () => SetSelectedTile(crutch);

                var tileMap = new TileMap();
                tileMap.TileSet = _selectedTileMap.TileSet;
                tileMap.SetCell(0, new Vector2I(0, 0), _selectedAtlas, TileCoords);
                button.Pressed += () => UpdateAlternativeTileInGUI(tileMap, _selectedAlternativeTile);
                tileMap.Position = new Vector2(8, 8);
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
        var atlasButtonsContainer = GetNode("CanvasLayer/TileModeGUI/AtlasButtonsContainer/VBoxContainer");

        KILLCHILDREN(atlasButtonsContainer);


        int AtlasesCount = _selectedTileMap.TileSet.GetSourceCount();

        ButtonGroup buttonGroup = new ButtonGroup();
        for (int i = 0; i < AtlasesCount; i++)
        {
            var button = MakeAButton(new Vector2(32, 32), buttonGroup);
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
        var TileMapsButtonsContainer = GetNode("CanvasLayer/TileModeGUI/TileMapsButtonsContainer/VBoxContainer");

        KILLCHILDREN(TileMapsButtonsContainer);

        UpdateAllTheTileMaps();

        ButtonGroup buttonGroup = new ButtonGroup();
        for (int i = 0; i < _allTheTileMaps.Count; i++)
        {
            var button = MakeAButton(new Vector2(32, 32), buttonGroup);
            int crutch = i;
            button.Pressed += () => SetSelectedTileMap(crutch);

            TileMapsButtonsContainer.AddChild(button);

            if (i == 0)
                button.ButtonPressed = true;
        }
    }
    public void UpdateVisibleTileMapsLayers()
    {
        var TileMapsLayersContainer = GetNode("CanvasLayer/TileModeGUI/TileMapsLayersContainer/VBoxContainer");

        KILLCHILDREN(TileMapsLayersContainer);

        int LayersCount = _selectedTileMap.GetLayersCount();

        ButtonGroup buttonGroup = new ButtonGroup();
        for (int i = 0; i < LayersCount; i++)
        {
            var button = MakeAButton(new Vector2(16, 16), buttonGroup);
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
        UpdateAllNodesArray();
        _allTheTileMaps.Clear();
        foreach (Node node in _allTheNodes)
        {
            if (node is TileMap)
                _allTheTileMaps.Add((TileMap)node);
        }
    }
    private TileSetAtlasSource GetCurrentAtlas()
    {
        return (TileSetAtlasSource)_selectedTileMap.TileSet.GetSource(_selectedAtlas);
    }
    private Godot.Button MakeAButton(Vector2 minimumSize = new Vector2(), ButtonGroup buttonGroup = null)
    {
        Godot.Button button = new Godot.Button();
        button.CustomMinimumSize = minimumSize;
        button.ToggleMode = true;
        if (buttonGroup != null)
            button.ButtonGroup = buttonGroup;
        button.FocusMode = FocusModeEnum.None;

        return button;
    }

    // NodeMode Section
    Node _selectedNode;
    Vector2 _mouseLastFramePos = new Vector2();
    private void SetSelectedNode(Node node)
    {
        _selectedNode = node;
    }
    private void SelectItem()
    {
        var NodesButtonsContainer = GetNode<Tree>("CanvasLayer/NodeModeGUI/NodesButtonsTree");
        _selectedNode = (Node)NodesButtonsContainer.GetSelected().GetMeta("CorrespondingNode");
        UpdateVisibleProperties();
    }
    private void UpdateAllNodesArray()
    {
        void Recursion(Node rootNode)
        {
            foreach (Node node in rootNode.GetChildren())
            {
                _allTheNodes = _allTheNodes.Append(node).ToArray();
                Recursion(node);
            }
        }
        _allTheNodes = new Node[0];
        Recursion(_level);
    }
    private void UpdateVisibleNodesButtons()
    {
        var NodesButtonsContainer = GetNode<Tree>("CanvasLayer/NodeModeGUI/NodesButtonsTree");

        KILLCHILDREN(NodesButtonsContainer);

        ButtonGroup buttonGroup = new ButtonGroup();
        var texture = ResourceLoader.Load<Texture2D>("res://Content/Sprites/Interface/AcceptButton.png");

        void Recursion(Node rootNode, TreeItem item)
        {
            foreach (Node node in rootNode.GetChildren())
            {
                var NextItem = item.CreateChild();
                NextItem.SetText(0, node.Name);
                NextItem.SetMeta("CorrespondingNode", node);
                _allTheNodes = _allTheNodes.Append(node).ToArray();
                Recursion(node, NextItem);
            }
        }
        var Item = NodesButtonsContainer.CreateItem();
        Item.SetText(0, _level.Name);
        Item.SetMeta("CorrespondingNode", _level);
        Recursion(_level, Item);
    }
    private void UpdateVisibleProperties()
    {
        var propertiesGrid = GetNode<GridContainer>("CanvasLayer/NodeModeGUI/PropertiesGridContainer/VBoxContainer/PropertiesGrid");

        KILLCHILDREN(propertiesGrid);

        var properties = _selectedNode.GetPropertyList();
        properties.Reverse();

        foreach (Godot.Collections.Dictionary item in properties)
        {
            var text = new Label();
            text.Text = item["name"].ToString().Capitalize();

            propertiesGrid.AddChild(text);

            var propertyValue = _selectedNode.Get(item["name"].ToString());
            var propertyType = (Variant.Type)(int)item["type"];
            switch (propertyType)
            {
                case Variant.Type.Bool:
                    var checkBox = new CheckBox();
                    checkBox.ButtonPressed = (bool)propertyValue;
                    checkBox.Toggled += (value) => SetProperty(_selectedNode, item["name"].ToString(), Variant.Type.Bool, value);
                    propertiesGrid.AddChild(checkBox);
                    break;
                case Variant.Type.Int or Variant.Type.Float:
                    var spinBox = new SpinBox();
                    spinBox.Rounded = false;
                    spinBox.Step = propertyType == Variant.Type.Float ? 0.000001 : 1;
                    spinBox.Value = propertyType == Variant.Type.Float ? (float)propertyValue : (int)propertyValue;
                    spinBox.MinValue = -1000;
                    spinBox.AllowGreater = true;
                    spinBox.AllowLesser = true;
                    spinBox.ValueChanged += (value) => SetProperty(_selectedNode, item["name"].ToString(), Variant.Type.Float, value);
                    propertiesGrid.AddChild(spinBox);
                    break;
                case Variant.Type.String or Variant.Type.StringName:
                    var lineEdit = new LineEdit();
                    lineEdit.Text = (string)propertyValue;
                    lineEdit.TextChanged += (value) => SetProperty(_selectedNode, item["name"].ToString(), Variant.Type.String, value);
                    propertiesGrid.AddChild(lineEdit);
                    break;

                case Variant.Type.Vector2 or Variant.Type.Vector2I:
                    {
                        var vectorSpinBoxX = new SpinBox();
                        vectorSpinBoxX.Rounded = false;
                        vectorSpinBoxX.Step = propertyType == Variant.Type.Float ? 0.000001 : 1;
                        vectorSpinBoxX.MinValue = -1000;
                        vectorSpinBoxX.AllowGreater = true;
                        vectorSpinBoxX.AllowLesser = true;
                        var vectorSpinBoxY = (SpinBox)vectorSpinBoxX.Duplicate();
                        vectorSpinBoxX.Value = ((Vector2)propertyValue).X;
                        vectorSpinBoxY.Value = ((Vector2)propertyValue).Y;
                        vectorSpinBoxX.ValueChanged += (value) => SetProperty(_selectedNode, item["name"].ToString(), Variant.Type.Vector2, value, "X");
                        vectorSpinBoxY.ValueChanged += (value) => SetProperty(_selectedNode, item["name"].ToString(), Variant.Type.Vector2, value, "Y");
                        propertiesGrid.AddChild(new Control());
                        propertiesGrid.AddChild(vectorSpinBoxX);
                        propertiesGrid.AddChild(vectorSpinBoxY);
                        NodeMovedByMouseEventHandler UpdatePositionProperty = () =>
                        {
                            vectorSpinBoxX.Value = ((Vector2)_selectedNode.Get(item["name"].ToString())).X;
                            vectorSpinBoxY.Value = ((Vector2)_selectedNode.Get(item["name"].ToString())).Y;
                        };
                        if (item["name"].ToString() == "position")
                        {
                            if (_lastConnectedPositionPropertyUpdate != null)
                                NodeMovedByMouse -= _lastConnectedPositionPropertyUpdate;
                            _lastConnectedPositionPropertyUpdate = UpdatePositionProperty;
                            NodeMovedByMouse += UpdatePositionProperty;
                        }
                        else if (item["name"].ToString() == "global_position")
                        {
                            if (_lastConnectedGlobalPositionPropertyUpdate != null)
                                NodeMovedByMouse -= _lastConnectedGlobalPositionPropertyUpdate;
                            _lastConnectedGlobalPositionPropertyUpdate = UpdatePositionProperty;
                            NodeMovedByMouse += UpdatePositionProperty;
                        }
                    }

                    break;
                case Variant.Type.Rect2 or Variant.Type.Rect2I:
                    {
                        var RectSpinBoxX = new SpinBox();
                        RectSpinBoxX.Rounded = false;
                        RectSpinBoxX.Step = propertyType == Variant.Type.Float ? 0.000001 : 1;
                        RectSpinBoxX.MinValue = -1000;
                        RectSpinBoxX.AllowGreater = true;
                        RectSpinBoxX.AllowLesser = true;
                        var RectSpinBoxY = (SpinBox)RectSpinBoxX.Duplicate();
                        var RectSpinBoxXPos = (SpinBox)RectSpinBoxX.Duplicate();
                        var RectSpinBoxYPos = (SpinBox)RectSpinBoxX.Duplicate();
                        RectSpinBoxX.Value = ((Rect2)propertyValue).Position.X;
                        RectSpinBoxY.Value = ((Rect2)propertyValue).Position.Y;
                        RectSpinBoxXPos.Value = ((Rect2)propertyValue).Size.X;
                        RectSpinBoxYPos.Value = ((Rect2)propertyValue).Size.Y;
                        RectSpinBoxX.ValueChanged += (value) => SetProperty(_selectedNode, item["name"].ToString(), Variant.Type.Vector2, value, "X");
                        RectSpinBoxY.ValueChanged += (value) => SetProperty(_selectedNode, item["name"].ToString(), Variant.Type.Vector2, value, "Y");
                        RectSpinBoxXPos.ValueChanged += (value) => SetProperty(_selectedNode, item["name"].ToString(), Variant.Type.Vector2, value, "XPos");
                        RectSpinBoxYPos.ValueChanged += (value) => SetProperty(_selectedNode, item["name"].ToString(), Variant.Type.Vector2, value, "YPos");
                        propertiesGrid.AddChild(new Control());
                        propertiesGrid.AddChild(RectSpinBoxX);
                        propertiesGrid.AddChild(RectSpinBoxY);
                        propertiesGrid.AddChild(RectSpinBoxXPos);
                        propertiesGrid.AddChild(RectSpinBoxYPos);
                    }
                    break;
                default:
                    propertiesGrid.AddChild(new Control());
                    break;
            }
        }
    }
    private void SetProperty(Node node, string name, Variant.Type varType, Variant value, string VectorOrRectDesiredValue = "")
    {
        var currentValue = node.Get(name);
        if (varType == Variant.Type.Vector2 || varType == Variant.Type.Vector2)
            switch (VectorOrRectDesiredValue)
            {
                case "X":
                    node.Set(name, new Vector2((float)value, ((Vector2)currentValue).Y));
                    break;
                case "Y":
                    node.Set(name, new Vector2(((Vector2)currentValue).X, (float)value));
                    break;
            }
        else if (varType == Variant.Type.Rect2 || varType == Variant.Type.Rect2I)
            switch (VectorOrRectDesiredValue)
            {
                case "X":
                    node.Set(name, new Rect2(new Vector2((float)value, ((Rect2)currentValue).Position.Y), new Vector2(((Rect2)currentValue).Size.X, ((Rect2)currentValue).Size.Y)));
                    break;
                case "Y":
                    node.Set(name, new Rect2(new Vector2(((Rect2)currentValue).Position.X, (float)value), new Vector2(((Rect2)currentValue).Size.X, ((Rect2)currentValue).Size.Y)));
                    break;
                case "XPos":
                    node.Set(name, new Rect2(new Vector2(((Rect2)currentValue).Position.X, ((Rect2)currentValue).Position.Y), new Vector2((float)value, ((Rect2)currentValue).Size.Y)));
                    break;
                case "YPos":
                    node.Set(name, new Rect2(new Vector2(((Rect2)currentValue).Position.X, ((Rect2)currentValue).Position.Y), new Vector2(((Rect2)currentValue).Size.X, (float)value)));
                    break;
            }
        else
            node.Set(name, value);
    }

    // Save Section
    private bool[] _editorCrutches = { true, true, true };
    public void SaveLevel()
    {
        var ToSave = new PackedScene();
        var LevelClone = _level.Duplicate();
        if (_editorCrutches[0])
            LevelClone.ProcessMode = ProcessModeEnum.Always;
        if (_editorCrutches[1])
            LevelClone.GetNode<CanvasLayer>("Pause").Visible = true;
        if (_editorCrutches[2])
            LevelClone.GetNode<CanvasLayer>("EpicIntro").Visible = true;

        ToSave.Pack(LevelClone);
        ResourceSaver.Save(ToSave, _mapPath);
    }
}
