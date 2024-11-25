using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

public partial class LevelEditor : Control
{
    [Signal] public delegate void NodeMovedByMouseEventHandler();
    PackedScene _packedLevel;

    NodeMovedByMouseEventHandler _lastConnectedPositionPropertyUpdate;
    NodeMovedByMouseEventHandler _lastConnectedGlobalPositionPropertyUpdate;
    Node _level, _mainLevelScene;
    Camera2D _camera;
    Node[] _allTheNodes = new Node[0];
    private string[] _allTheFiles = new string[0];
    Timer _guiDelayTimer;
    TextureButton _screenButton;
    Sprite2D _extraCursor;
    Control _nodeModeGui;


    Vector2 _selectedNodeGlobalPosition, _smartPositioningDistance = new Vector2(32, 32); bool _doSmartPos = false; // for "smart" positioning
    bool _seeAllTheNodes = false; string _mainLevelSceneName = "Level";


    enum EditorMode { TileMode, NodeMode, CodeMode, SavePage }
    EditorMode _editorMode = EditorMode.TileMode;

    enum ExtraCursorMode { Null, HoldingNode, HoldingFile}
    ExtraCursorMode _extraCursorMode = ExtraCursorMode.Null;
    private string _pickedTreeItem;
    private Vector2 _itemPickMousePos;

    private string _mapPath = "", _mapFolder = "", _selectedFilePath = "";
    private readonly string _defaultPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + @"\Godot\app_userdata\Little Slippey\mods\";

    public override void _Ready()
    {
        _mapPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + @"\Godot\app_userdata\Little Slippey\mods\" + G.ModMapPath;
        _mapFolder = G.ModMapFolder;
        G.IsLevelVanilla = false;
        _level = (Node2D)ResourceLoader.Load<PackedScene>(_mapPath).Instantiate();
        if (_editorCrutches[0])
            _level.ProcessMode = ProcessModeEnum.Disabled;
        if (_editorCrutches[1])
            _level.GetNode<CanvasLayer>("Pause").Visible = false;
        if (_editorCrutches[2])
            _level.GetNode<CanvasLayer>("EpicIntro").Visible = false;
        if (_editorCrutches[3])
            _level.GetNode<Label>("Level/Player/Camera2D/GUI/Scores").SetDeferred("visible", false);

        GetNode("LevelContainer").AddChild(_level);
        _mainLevelScene = _level.GetNode(_mainLevelSceneName);
        _selectedTileMap = _level.GetNode<TileMap>("Level/TileMap");
        _camera = GetNode<Camera2D>("Camera2D");
        _assistiveTileMap = GetNode<TileMap>("AssistiveTileMap");
        _erasingAssistiveTileMap = GetNode<TileMap>("ErasingAssistiveTileMap");
        _screenButton = GetNode<TextureButton>("CanvasLayer/ScreenButton");
        _extraCursor = GetNode<Sprite2D>("CanvasLayer/ExtraCursor");
        _nodeModeGui = GetNode<Control>("CanvasLayer/TileModeGUI");
        _guiDelayTimer = GetNode<Timer>("CanvasLayer/GUIDelayTimer");
        _codeEdit = GetNode<CodeEdit>("CanvasLayer/CodeModeGUI/CodeEdit");

        _filesButtonsTree = GetNode<Tree>("CanvasLayer/NodeModeGUI/FilesButtonsTree");
        _nodesButtonsTree = GetNode<Tree>("CanvasLayer/NodeModeGUI/NodesButtonsTree");


        UpdateVisibleTileMapsLayers();
        UpdateVisibleAtlases();
        UpdateVisibleTileSet();
        UpdateVisibleTileMaps();
        _assistiveTileMap.TileSet = _selectedTileMap.TileSet;

        GetTree().Root.FilesDropped += AddFile;
        TreeExiting += () => GetTree().Root.FilesDropped -= AddFile;

        GetNode<Tree>("CanvasLayer/NodeModeGUI/NodesButtonsTree").Connect("gui_input", new Callable(this, "OnNodesTreeGuiInput"));
        GetNode<Tree>("CanvasLayer/NodeModeGUI/FilesButtonsTree").Connect("gui_input", new Callable(this, "OnFilesTreeGuiInput"));
    }

    Vector2 _globalMousePos, _localMousePos, _guiMousePos, _globalMouseLastFramePos = new Vector2(), _localMouseLastFramePos;
    public override void _PhysicsProcess(double delta)
    {
        //Mouse processing
        {
            _globalMousePos = GetGlobalMousePosition();
            _localMousePos = _camera.GetLocalMousePosition();
            _guiMousePos = _nodeModeGui.GetLocalMousePosition();

            if (Input.IsActionJustPressed("MouseRightClick"))
            {
                if (_guiDelayTimer.IsStopped())
                {
                    if (GetNode<PopupMenu>("CanvasLayer/NodeModeGUI/NodePopupMenu").Visible)
                        GetNode<PopupMenu>("CanvasLayer/NodeModeGUI/NodePopupMenu").CallDeferred("hide");
                    else if (GetNode<PopupPanel>("CanvasLayer/NodeModeGUI/NodeNameEditPopup").Visible)
                        GetNode<PopupPanel>("CanvasLayer/NodeModeGUI/NodeNameEditPopup").Hide();
                    else if (GetNode<PopupMenu>("CanvasLayer/NodeModeGUI/FilePopupMenu").Visible)
                        GetNode<PopupMenu>("CanvasLayer/NodeModeGUI/FilePopupMenu").CallDeferred("hide");
                }
            }

            if (_screenButton.HasFocus())
                _camera.Position += new Vector2(
                Input.GetActionStrength("ui_right") - Input.GetActionStrength("ui_left"),
                Input.GetActionStrength("ui_down") - Input.GetActionStrength("ui_up")) *
                ((Input.IsKeyPressed(Key.Shift) ? 30 : 15) * 1 / _camera.Zoom.X);

            if (_screenButton.IsHovered())
            {
                if (Input.IsActionJustPressed("MouseWheelScrollDown"))
                    ChangeZoom(0.8f);
                else if (Input.IsActionJustPressed("MouseWheelScrollUp"))
                    ChangeZoom(1.25f);

            }

            if (_screenButton.ButtonPressed)
            {
                if (Input.IsActionPressed("MouseWheelClick"))
                    _camera.GlobalPosition += _localMouseLastFramePos - _localMousePos;
            }

            _extraCursor.Position = _guiMousePos;
            if (_extraCursorMode == ExtraCursorMode.Null)
            {
                if (Input.IsActionJustPressed("MouseLeftClick"))
                {
                    
                }
                else if (Input.IsActionPressed("MouseLeftClick") && _pickedTreeItem != null && _itemPickMousePos.DistanceTo(_localMousePos) > 10 && Enum.TryParse("Holding" + _pickedTreeItem, out ExtraCursorMode newCursorMode))
                {
                    _extraCursorMode = newCursorMode;
                    _extraCursor.Texture = GD.Load<Texture2D>("res://Content/Sprites/Interface/LevelEditor/" + _pickedTreeItem + "Cursor.png");
                }
                else if (Input.IsActionJustReleased("MouseLeftClick"))
                {
                    _pickedTreeItem = "";
                    _extraCursorMode = ExtraCursorMode.Null;
                    _extraCursor.Texture = null;
                }    

            }
            else if (Input.IsActionJustReleased("MouseLeftClick"))
            {
                _pickedTreeItem = "";
                _extraCursorMode = ExtraCursorMode.Null;
                _extraCursor.Texture = null;

                var NodesButtonsContainer = GetNode<Tree>("CanvasLayer/NodeModeGUI/NodesButtonsTree");
                var FilesButtonsContainer = GetNode<Tree>("CanvasLayer/NodeModeGUI/FilesButtonsTree");

                TreeItem TargetItem = null;
                TreeItem SelectedItem = null;
                Tree TargetTree = null;
                Tree SelectedTree = null;

                void TrySetItems(Tree tree)
                {
                    if (tree.GetItemAtPosition(tree.GetLocalMousePosition()) != null)
                    {
                        TargetItem = tree.GetItemAtPosition(tree.GetLocalMousePosition());
                        TargetTree = tree;
                    }

                    if (tree.GetSelected() != null)
                    {
                        SelectedItem = tree.GetSelected();
                        SelectedTree = tree;
                    }
                }

                TrySetItems(NodesButtonsContainer);
                TrySetItems(FilesButtonsContainer);


                //I tried to make it as readable as I could...
                string SelectedFilePath = SelectedTree == FilesButtonsContainer ? SelectedItem.GetMeta("FilePath").ToString() : null;
                string TargetFilePath = TargetTree == FilesButtonsContainer ? TargetItem.GetMeta("FilePath").ToString() : null;

                bool DefaultCondition = TargetItem != null && SelectedItem != TargetItem;
                bool DoesItemsBelongTo(Tree tree)
                {
                    return TargetItem.GetTree() == tree && SelectedItem.GetTree() == tree;
                }
                bool IsSelectedADirectory = Path.GetExtension(SelectedFilePath) == "", IsTargetADirectory = Path.GetExtension(TargetFilePath) == "";

                if (DefaultCondition && DoesItemsBelongTo(NodesButtonsContainer))
                {
                    MoveTreeItem(SelectedItem, TargetItem);
                    _selectedNode.Reparent(TargetItem.GetMeta("CorrespondingNode").As<Node>());
                }
                else if (DefaultCondition && DoesItemsBelongTo(FilesButtonsContainer) && IsTargetADirectory)
                {

                    string OldPath = SelectedFilePath, NewPath = TargetItem.GetMeta("FilePath").ToString() + @"\" + GetFileName(_selectedFilePath);
                    MoveFile(OldPath, TargetItem.GetMeta("FilePath").ToString() + @"\" + GetFileName(_selectedFilePath));
                    SelectedItem.SetMeta("FilePath", NewPath);
                    _selectedFilePath = NewPath;

                    MoveTreeItem(SelectedItem, TargetItem);
                    Recursion(SelectedItem);

                    void Recursion(TreeItem rootItem)
                    {
                        foreach (TreeItem childItem in rootItem.GetChildren())
                        {
                            childItem.SetMeta("FilePath", rootItem.GetMeta("FilePath").ToString() + @"\" + GetFileName(childItem.GetMeta("FilePath").ToString()));
                            Recursion(childItem);
                        }
                    }
                }
                else if (((SelectedTree == FilesButtonsContainer && _screenButton.IsHovered()) || (SelectedTree == FilesButtonsContainer && TargetTree == NodesButtonsContainer)) && !IsSelectedADirectory)
                {
                    
                    if (_screenButton.IsHovered())
                    {
                        HandleScreenButtonHovered();
                    }
                    else
                    {
                        HandleItemHovered();
                    }

                    void HandleScreenButtonHovered()
                    {
                        switch (Path.GetExtension(SelectedFilePath))
                        {
                            case ".tscn":
                                var node = GD.Load<PackedScene>(_selectedFilePath).Instantiate();
                                CreateNode(node, node.Name);

                                if (node is Node2D or Control)
                                    node.Set("global_position", _globalMousePos);
                                break;

                            case ".png" or ".jpg":
                                var sprite = new Sprite2D
                                {
                                    Texture = FileSystemExtension.LoadNonResourceImage(_selectedFilePath),
                                    Scale = new Vector2(4, 4)
                                };

                                CreateNode(sprite, GetFileName(_selectedFilePath));
                                sprite.Set("global_position", _globalMousePos);
                                break;
                        }
                    }

                    void HandleItemHovered()
                    {
                        NodesButtonsContainer.SetSelected(TargetItem, 0);
                        var targetNode = (Node)TargetItem.GetMeta("CorrespondingNode");

                        switch (Path.GetExtension(SelectedFilePath))
                        {
                            case ".tscn":
                                var node = GD.Load<PackedScene>(SelectedFilePath).Instantiate();
                                CreateNode(node, node.Name);
                                break;

                            case ".cs" or ".gd":
                                var script = GD.Load<Script>(SelectedFilePath);
                                targetNode.SetScript(script);
                                script.Reload();
                                break;

                            case ".png" or ".jpg":
                                if (targetNode is not Sprite2D and not TextureRect)
                                {
                                    var sprite = new Sprite2D
                                    {
                                        Texture = FileSystemExtension.LoadNonResourceImage(SelectedFilePath),
                                        Scale = new Vector2(4, 4)
                                    };

                                    CreateNode(sprite, GetFileName(SelectedFilePath));
                                }
                                else
                                {
                                    targetNode.Set("texture", FileSystemExtension.LoadNonResourceImage(SelectedFilePath));
                                }
                                break;
                        }
                    }


                }
                else if (SelectedTree == NodesButtonsContainer && TargetTree == FilesButtonsContainer && IsTargetADirectory)
                {
                    SaveNodeTo(_selectedNode, TargetFilePath);
                }
            }
        }


        switch (_editorMode)
        {
            case EditorMode.TileMode:
                _mouseTilePos = new Vector2I((int)((_globalMousePos.X - _selectedTileMap.GlobalPosition.X) / (16 * _selectedTileMap.Scale.X)), (int)((_globalMousePos.Y - _selectedTileMap.GlobalPosition.Y) / (16 * _selectedTileMap.Scale.Y))) + new Vector2I(_globalMousePos.X < 0 ? -1 : 0, _globalMousePos.Y < 0 ? -1 : 0);

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
                if (_screenButton.ButtonPressed && Input.IsActionJustPressed("MouseLeftClick"))
                {
                    if (_selectedNode is Node2D or Control)
                        _selectedNodeGlobalPosition = (Vector2)_selectedNode.Get("global_position");
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

                    if (_selectedNode is Node2D or Control)
                    {
                        _selectedNodeGlobalPosition -= _globalMouseLastFramePos - _globalMousePos;
                        _selectedNode.Set("global_position", _selectedNodeGlobalPosition - _selectedNodeGlobalPosition % _smartPositioningDistance);
                    }
                    EmitSignal("NodeMovedByMouse");
                }

                break;
        }

        _globalMouseLastFramePos = _globalMousePos;
        _localMouseLastFramePos = _localMousePos;
    }
    public void TakeTreeitem(string name)
    {
        _pickedTreeItem = name;
        _itemPickMousePos = _localMousePos;
    }
    public void SetDoSmartPos(bool value)
    {
        _doSmartPos = value;
        _smartPositioningDistance = _doSmartPos ? new Vector2(32, 32) : new Vector2(1, 1);
    }
    public void SetSeeAllTheNodes(bool value)
    {
        _seeAllTheNodes = value;
        _mainLevelScene = _seeAllTheNodes ? _level : _level.GetNode(_mainLevelSceneName);
        UpdateVisibleNodesButtons();
    }
    public void SetOption(Variant value, string option)
    {
        Set(option, value);
    }
    public void ChangeZoom(float value, bool stickToTheCursor = true)
    {
        float NewZoom = Mathf.Clamp(_camera.Zoom.X * value, 1f / 16, 1 * 16);

        if (stickToTheCursor && _camera.Zoom != new Vector2(NewZoom, NewZoom))
            _camera.Position += _camera.GetLocalMousePosition() * 0.25f * (value > 1 ? 1 : -1);

        _camera.Zoom = new Vector2(NewZoom, NewZoom);
    }

    public void TestLevel()
    {
        SaveLevel();
        GetTree().ChangeSceneToPacked(_packedLevel);
    }

    public void SetEditorMode(int value)
    {
        _editorMode = (EditorMode)value;
        GetNode<Control>("CanvasLayer/TileModeGUI").Visible = _editorMode == EditorMode.TileMode;
        GetNode<Control>("CanvasLayer/NodeModeGUI").Visible = _editorMode == EditorMode.NodeMode;
        GetNode<Control>("CanvasLayer/CodeModeGUI").Visible = _editorMode == EditorMode.CodeMode;
        GetNode<Control>("CanvasLayer/SavePage").Visible = _editorMode == EditorMode.SavePage;
        GetNode<TileMap>("AssistiveTileMap").Visible = _editorMode == EditorMode.TileMode;
        GetNode<TileMap>("ErasingAssistiveTileMap").Visible = _editorMode == EditorMode.TileMode;
        UpdateAllNodesArray();
        switch (_editorMode)
        {
            case EditorMode.TileMode:
                UpdateAllTheTileMaps();
                SetSelectedTileMap(0);
                break;

            case EditorMode.NodeMode:
                UpdateVisibleNodesButtons();
                UpdateVisibleFiles();
                break;
            case EditorMode.CodeMode:

                break;
            case EditorMode.SavePage:

                break;
        }
    }

    private void KILLCHILDREN(Node node)
    {
        foreach (Node child in node.GetChildren())
            child.QueueFree();
    }
    private void MoveTreeItem(TreeItem item, TreeItem target, bool UnCollapseTarget = true)
    {
        item.GetParent().RemoveChild(item);

        target.AddChild(item);

        if (UnCollapseTarget)
            target.Collapsed = false;
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
        _assistiveTileMap.GlobalTransform = _erasingAssistiveTileMap.GlobalTransform = _selectedTileMap.GlobalTransform;
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
        var atlasButtonsContainer = GetNode("CanvasLayer/TileModeGUI/AtlasButtonsContainer/VBoxContainer");

        KILLCHILDREN(atlasButtonsContainer);


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
        var TileMapsButtonsContainer = GetNode("CanvasLayer/TileModeGUI/TileMapsButtonsContainer/VBoxContainer");

        KILLCHILDREN(TileMapsButtonsContainer);

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
        var TileMapsLayersContainer = GetNode("CanvasLayer/TileModeGUI/TileMapsLayersContainer/VBoxContainer");

        KILLCHILDREN(TileMapsLayersContainer);

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

    // NodeMode Section

    Node _selectedNode;
    Tree _nodesButtonsTree;

    private void SetSelectedNode(Node node)
    {
        _selectedNode = node;
        if (_selectedNode is Node2D or Control)
            _selectedNodeGlobalPosition = (Vector2)_selectedNode.Get("global_position");
    }
    private void SelectNode()
    {
        if (Input.IsActionJustPressed("MouseLeftClick"))
            TakeTreeitem("Node");

        Node PreviousSelectedNode = _selectedNode;
        SetSelectedNode((Node)_nodesButtonsTree.GetSelected().GetMeta("CorrespondingNode"));
        UpdateVisibleProperties();

        if (Input.IsMouseButtonPressed(MouseButton.Right))
        {
            var nodePopupMenu = GetNode<PopupMenu>("CanvasLayer/NodeModeGUI/NodePopupMenu");
            
            nodePopupMenu.Position = (Vector2I)GetNode<Control>("CanvasLayer/NodeModeGUI").GetLocalMousePosition();
            nodePopupMenu.Popup();
            _guiDelayTimer.Start(0.02f);
        }
        else if (PreviousSelectedNode == _selectedNode)
            NodePopupAction(4);
    }
    public void NodePopupAction(int actionIndex)
    {
        Node node;

        TreeItem SelectedItem = _nodesButtonsTree.GetSelected();

        switch (actionIndex)
        {
            case 0:
                GetNode<Control>("CanvasLayer/NodeModeGUI/NodeCreateMenu").Show();
                break;
            case 1:
                G.NodeCopyBuffer.Clear();
                G.NodeCopyBuffer.Add(_selectedNode.Duplicate());
                _selectedNode.QueueFree();
                _selectedNode = null;
                _nodesButtonsTree.GetSelected().Free();
                break;
            case 2:
                G.NodeCopyBuffer.Clear();
                G.NodeCopyBuffer.Add(_selectedNode.Duplicate());
                break;
            case 3:
                node = G.NodeCopyBuffer[0].Duplicate();
                _selectedNode.AddChild(node);
                node.Name = G.NodeCopyBuffer[0].Name;

                CreateNodeItem(node);
                break;
            case 4:
                var lineEditPopup = GetNode<PopupPanel>("CanvasLayer/NodeModeGUI/NodeNameEditPopup");

                lineEditPopup.Position = (Vector2I)GetNode<Control>("CanvasLayer/NodeModeGUI").GetLocalMousePosition();
                lineEditPopup.Popup();
                lineEditPopup.GetNode<LineEdit>("LineEdit").Text = _selectedNode.Name;
                _guiDelayTimer.Start(0.02f);
                break;
            case 5:
                var Duplicate = _selectedNode.Duplicate();
                _selectedNode.GetParent().AddChild(Duplicate);
                Duplicate.Name = _selectedNode.Name;

                CreateNodeItem(Duplicate, SelectedItem.GetParent());
                break;
            case 6:
                SaveNodeTo(_selectedNode, _mapFolder);
                break;
            case 7:
                DisplayServer.ClipboardSet(_level.GetPathTo(_selectedNode));
                break;
            case 8:
                DeleteNodeByItem(SelectedItem);
                break;
        }
    }
    private void SetNodeName(string value)
    {
        if (_selectedNode != null)
        {
            _selectedNode.Name = value;
            GetNode<Tree>("CanvasLayer/NodeModeGUI/NodesButtonsTree").GetSelected().SetText(0, _selectedNode.Name);
        }
        GetNode<PopupPanel>("CanvasLayer/NodeModeGUI/NodeNameEditPopup").Hide();
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
        _nodesButtonsTree.Clear();

        ButtonGroup buttonGroup = new ButtonGroup();
        var texture = ResourceLoader.Load<Texture2D>("res://Content/Sprites/Interface/AcceptButton.png");

        void Recursion(Node rootNode, TreeItem item)
        {
            foreach (Node node in rootNode.GetChildren())
            {
                var NextItem = item.CreateChild();
                NextItem.SetText(0, node.Name);
                NextItem.SetMeta("CorrespondingNode", node);
                NextItem.Collapsed = true;
                _allTheNodes = _allTheNodes.Append(node).ToArray();
                Recursion(node, NextItem);
            }
        }
        var Item = _nodesButtonsTree.CreateItem();
        Item.SetText(0, _mainLevelScene.Name);
        Item.SetMeta("CorrespondingNode", _mainLevelScene);
        Recursion(_mainLevelScene, Item);

    }
    private void SaveNodeTo(Node node, string directory)
    {
        PackedScene packedScene = new PackedScene();
        packedScene.Pack(node.Duplicate());

        string path = directory + @"\" + node.Name + ".tscn";

        ResourceSaver.Save(packedScene, path);
        CreateFileItem(path, GetTargetedTreeItem(_filesButtonsTree));
    }
    private void DeleteNodeByItem(TreeItem nodeItem)
    {
        if (nodeItem.GetTree() != _nodesButtonsTree) return;

        var node = (Node)nodeItem.GetMeta("CorrespondingNode");
        _allTheNodes = _allTheNodes.Where(x => x !=  node).ToArray();
        if (_selectedNode == node) 
            _selectedNode = null;
        node.QueueFree();
        nodeItem.Free();
    }

    TreeItem CreateNodeItem(Node itemNode, TreeItem parent = null)
    {
        bool IsParentValid = parent != null && parent.GetTree() == _nodesButtonsTree;

        TreeItem Item;
        Item = _nodesButtonsTree.CreateItem(IsParentValid ? parent : _nodesButtonsTree.GetSelected());
        Item.SetText(0, itemNode.Name);
        Item.SetMeta("CorrespondingNode", itemNode);
        return Item;
    }

    // Files
    Tree _filesButtonsTree;

    private void UpdateVisibleFiles()
    {
        _filesButtonsTree.Clear();

        ButtonGroup buttonGroup = new ButtonGroup();
        var texture = ResourceLoader.Load<Texture2D>("res://Content/Sprites/Interface/AcceptButton.png");


        void Recursion(string rootFile, TreeItem item)
        {
            foreach (string folder in Directory.GetDirectories(rootFile))
            {
                var NextItem = item.CreateChild();
                NextItem.SetText(0, folder.Remove(0, rootFile.Length + 1));
                NextItem.SetMeta("FilePath", folder);
                NextItem.Collapsed = true;
                _allTheFiles = _allTheFiles.Append(folder).ToArray();
                Recursion(folder, NextItem);
            }

            foreach (string file in Directory.GetFiles(rootFile))
            {
                if (HasStringFormats(file, new string[]{"tscn", "png", "jpg", "cs", "gd", "json", "ogv", "gdshader", "mp3", "mp4", "ogg", "wav", "theme", "ttf", "tres"}))
                {
                    var NextItem = item.CreateChild();
                    NextItem.SetText(0, file.Remove(0, rootFile.Length + 1));
                    NextItem.SetMeta("FilePath", file);
                    NextItem.Collapsed = true;
                    _allTheFiles = _allTheFiles.Append(file).ToArray();
                }
            }
        }
        var Item = _filesButtonsTree.CreateItem();
        Item.SetText(0, _mapFolder.Remove(0, _defaultPath.Length));
        Item.SetMeta("FilePath", _mapFolder);
        Recursion(_mapFolder, Item);
    }
    private void SetSelectedFile(string path)
    {
        _selectedFilePath = path;
    }
    private void FilePopupAction(int actionIndex)
    {
        TreeItem SelectedItem = _filesButtonsTree.GetSelected();

        switch (actionIndex)
        {
            case 0:
                for (int i = 0; ; i++)
                {
                    var SuggestedPath = _selectedFilePath + @"\NewFolder" + i;
                    if (!Directory.Exists(SuggestedPath))
                    {
                        Directory.CreateDirectory(SuggestedPath);
                        CreateFileItem(SuggestedPath, SelectedItem);
                        break;
                    }
                }
                break;
            case 1:
                DeleteFileByItem(SelectedItem);
                break;
            case 2:
                DisplayServer.ClipboardSet(ProjectSettings.LocalizePath(ProjectSettings.LocalizePath(_selectedFilePath)));
                break;
            case 3:
                DisplayServer.ClipboardSet(_selectedFilePath);
                break;
            case 4:
                var lineEditPopup = GetNode<PopupPanel>("CanvasLayer/NodeModeGUI/FileNameEditPopup");

                lineEditPopup.Position = (Vector2I)GetNode<Control>("CanvasLayer/NodeModeGUI").GetLocalMousePosition();
                lineEditPopup.Popup();
                lineEditPopup.GetNode<LineEdit>("LineEdit").Text = GetNode<Tree>("CanvasLayer/NodeModeGUI/FilesButtonsTree").GetSelected().GetText(0);
                _guiDelayTimer.Start(0.02f);
                break;
            case 5:

                break;
            case 6:
                Process.Start("explorer.exe", _selectedFilePath.Remove(_selectedFilePath.RFind(@"\")));
                break;
            case 7:
                Process.Start("explorer.exe", _selectedFilePath);
                break;
        }
    }
    private void SetFileName(string value)
    {
        if (_selectedFilePath != null)
        {
            string NewPath = GetFilePath(_selectedFilePath) + value;

            MoveFile(_selectedFilePath, NewPath);

            var treeItem = _filesButtonsTree.GetSelected();
            treeItem.SetText(0, value);
            treeItem.SetMeta("FilePath", NewPath);

            _selectedFilePath = NewPath;
        }
        GetNode<PopupPanel>("CanvasLayer/NodeModeGUI/FileNameEditPopup").Hide();
    }
    private void SelectFile()
    {
        if (Input.IsActionJustPressed("MouseLeftClick"))
            TakeTreeitem("File");

        string PreviousSelectedFile = _selectedFilePath;
        SetSelectedFile(_filesButtonsTree.GetSelected().GetMeta("FilePath").ToString());

        //UpdateVisibleProperties();

        if (Input.IsMouseButtonPressed(MouseButton.Right))
        {
            var filePopupMenu = GetNode<PopupMenu>("CanvasLayer/NodeModeGUI/FilePopupMenu");

            filePopupMenu.Position = (Vector2I)GetNode<Control>("CanvasLayer/NodeModeGUI").GetLocalMousePosition();
            filePopupMenu.Popup();
            _guiDelayTimer.Start(0.02f);
        }
        //else if (PreviousSelectedFile == _selectedFile)
        //NodePopupAction(4);
    }
    private void MoveFile(string file, string targetPath)
    {
        if (!file.Contains("."))
            Directory.Move(file, targetPath);
        else
            File.Move(file, targetPath);
    }
    public void AddFile(string[] files)
    {
        TreeItem SelectedFileItem = _filesButtonsTree.GetSelected();
        string SubString = @"\" + GetFileName(files[0]);

        DirAccess.CopyAbsolute(files[0], SelectedFileItem != null ? SelectedFileItem.GetMeta("FilePath").ToString() + SubString : _filesButtonsTree.GetRoot().GetMeta("FilePath").ToString() + SubString);

        UpdateVisibleFiles();
    }
    private void DeleteFileByItem(TreeItem fileItem)
    {
        if (fileItem.GetTree() != _filesButtonsTree) return;

        var path = (string)fileItem.GetMeta("FilePath");
        _allTheFiles = _allTheFiles.Where(x => x != path).ToArray();
        if (_selectedFilePath == path)
            _selectedFilePath = null;
        DirAccess.RemoveAbsolute(path);
        fileItem.Free();
    }

    TreeItem CreateFileItem(string path, TreeItem parent = null)
    {
        bool IsParentValid = parent != null && parent.GetTree() == _filesButtonsTree;

        if (IsParentValid)
            parent.Collapsed = false;

        TreeItem Item;
        Item = _filesButtonsTree.CreateItem(IsParentValid ? parent : _filesButtonsTree.GetSelected());
        Item.SetText(0, GetFileName(path));
        Item.SetMeta("FilePath", path);
        _allTheFiles = _allTheFiles.Append(path).ToArray();
        return Item;
    }

    //String Tweaks
    bool HasStringFormats(string value, string[] formats)
    {
        foreach (string format in formats)
            if (value.Contains("." + format))
                return true;
        return false;
    }
    bool DoesStringHaveFormat(string value, string format)
    {
        return value.Contains("." + format);
    }
    string GetFileName(string path)
    {
        return path.Remove(0, path.RFind(@"\") + 1);
    }
    string GetFilePath(string path)
    {
        return path.Remove(path.RFind(@"\")) + @"\";
    }
    //String Tweaks

    // Files end

    private string[] _propertyBlackList =
    {
        "global_position", "global_rotation", "global_transform", "global_scale", "global_skew", "global_rotation_degrees", "rotation", "import_path", "name", "owner", "multiplayer", "process"
    };
    
    private void UpdateVisibleProperties()
    {
        var propertiesGrid = GetNode<GridContainer>("CanvasLayer/NodeModeGUI/PropertiesGridContainer/VBoxContainer/PropertiesGrid");
        KILLCHILDREN(propertiesGrid);

        foreach (Godot.Collections.Dictionary item in _selectedNode.GetPropertyList())
        {
            AddPropertyToGrid(propertiesGrid, item);
        }
        void AddPropertyToGrid(GridContainer propertiesGrid, Godot.Collections.Dictionary property)
        {
            string propertyName = property["name"].ToString();

            if (_propertyBlackList.Contains(propertyName)) return;

            var propertyLabel = new Label { Text = propertyName.Capitalize(), Theme = GD.Load<Theme>("res://Content/Other/LevelEditor.theme") };
            propertiesGrid.AddChild(propertyLabel);

            var propertyValue = _selectedNode.Get(propertyName);
            var propertyType = (Variant.Type)(int)property["type"];


            var propertyEditor = CreatePropertyEditor(propertyName, propertyType, propertyValue);

            propertiesGrid.AddChild(propertyEditor);
                
        }

        Control CreatePropertyEditor(string propertyName, Variant.Type propertyType, Variant propertyValue)
        {
            return propertyType switch
            {
                Variant.Type.Bool => CreateCheckBox(propertyName, propertyValue.AsBool()),
                Variant.Type.Int => CreateSpinBox(propertyName, Variant.Type.Int, propertyValue.AsInt32()),
                Variant.Type.Float => CreateSpinBox(propertyName, Variant.Type.Float, (float)propertyValue.AsDouble()),
                Variant.Type.String => CreateLineEdit(propertyName, propertyValue.AsString()),
                Variant.Type.StringName => CreateLineEdit(propertyName, propertyValue.AsStringName()),
                Variant.Type.Vector2 => CreateVector2Editor(propertyName, propertyValue.AsVector2()),
                Variant.Type.Rect2 => CreateRect2Editor(propertyName, propertyValue.AsRect2()),
                Variant.Type.Color => CreateColorPicker(propertyName, propertyValue.AsColor()),
                _ => new Control()
            };
        }

        CheckBox CreateCheckBox(string propertyName, bool value)
        {
            var checkBox = new CheckBox { ButtonPressed = value };
            checkBox.Toggled += (v) => SetProperty(_selectedNode, propertyName, Variant.Type.Bool, v);

            return checkBox;
        }

        SpinBox CreateSpinBox(string propertyName, Variant.Type propertyType, double value, string optionalKey = null)
        {
            var spinBox = new SpinBox
            {
                Rounded = false,
                Step = propertyType == Variant.Type.Float ? 0.000001 : 1,
                MinValue = int.MinValue,
                MaxValue = int.MaxValue,
                AllowGreater = true,
                AllowLesser = true
            };
            spinBox.Value = value;
            spinBox.ValueChanged += (v) => SetProperty(_selectedNode, propertyName, propertyType, v, optionalKey);
            return spinBox;
        }

        LineEdit CreateLineEdit(string propertyName, string value)
        {
            var lineEdit = new LineEdit { Text = value };
            lineEdit.TextChanged += (v) => SetProperty(_selectedNode, propertyName, Variant.Type.String, v);

            return lineEdit;
        }

        ColorPickerButton CreateColorPicker(string propertyName, Color value)
        {
            var colorPickerButton = new ColorPickerButton { Color = value };
            colorPickerButton.ColorChanged += (c) => SetProperty(_selectedNode, propertyName, Variant.Type.Color, c);
            return colorPickerButton;
        }


        Control CreateVector2Editor(string propertyName, Vector2 value, string optionalKey = null)
        {
            var container = new HBoxContainer();
            container.AddChild(CreateSpinBox(propertyName, Variant.Type.Vector2, value.X, "x" + optionalKey));
            container.AddChild(CreateSpinBox(propertyName, Variant.Type.Vector2, value.Y, "y" + optionalKey));
            return container;
        }

        Control CreateRect2Editor(string propertyName, Rect2 value, bool? doBindProperty = null)
        {
            var container = new VBoxContainer();
            container.AddChild(CreateVector2Editor(propertyName, value.Position));
            container.AddChild(CreateVector2Editor(propertyName, value.Size, "Size"));

            return container;
        }
    }




    private void SetProperty(Node node, string name, Variant.Type varType, Variant value, string optionalKey = "")
    {
        var currentValue = node.Get(name);
        if (varType == Variant.Type.Vector2 || varType == Variant.Type.Vector2I)
            switch (optionalKey)
            {
                case "x":
                    GD.Print(value);
                    node.Set(name, new Vector2((float)value, ((Vector2)currentValue).Y));
                    break;
                case "y":
                    node.Set(name, new Vector2(((Vector2)currentValue).X, (float)value));
                    break;
            }
        else if (varType == Variant.Type.Rect2 || varType == Variant.Type.Rect2I)
            switch (optionalKey)
            {
                case "x":
                    node.Set(name, new Rect2(new Vector2((float)value, ((Rect2)currentValue).Position.Y), new Vector2(((Rect2)currentValue).Size.X, ((Rect2)currentValue).Size.Y)));
                    break;
                case "y":
                    node.Set(name, new Rect2(new Vector2(((Rect2)currentValue).Position.X, (float)value), new Vector2(((Rect2)currentValue).Size.X, ((Rect2)currentValue).Size.Y)));
                    break;
                case "xSize":
                    node.Set(name, new Rect2(new Vector2(((Rect2)currentValue).Position.X, ((Rect2)currentValue).Position.Y), new Vector2((float)value, ((Rect2)currentValue).Size.Y)));
                    break;
                case "ySize":
                    node.Set(name, new Rect2(new Vector2(((Rect2)currentValue).Position.X, ((Rect2)currentValue).Position.Y), new Vector2(((Rect2)currentValue).Size.X, (float)value)));
                    break;
            }
        else if (varType == Variant.Type.Color)
        {
            node.Set(name, value);
        }
        else
            node.Set(name, value);
    }
    private void OnNodesTreeGuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent)
        {
            if (mouseEvent.ButtonIndex == MouseButton.Right && mouseEvent.Pressed)
            {
                TreeItem clickedItem = GetTargetedTreeItem(_nodesButtonsTree);

                if (clickedItem != null)
                {
                    _nodesButtonsTree.SetSelected(clickedItem, 0);

                    var popupMenu = GetNode<PopupMenu>("CanvasLayer/NodeModeGUI/NodePopupMenu");
                    popupMenu.SetPosition((Vector2I)mouseEvent.GlobalPosition);
                    popupMenu.CallDeferred("popup");
                }
            }
        }
    }
    private void OnFilesTreeGuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent)
        {
            if (mouseEvent.ButtonIndex == MouseButton.Right && mouseEvent.Pressed)
            {
                TreeItem clickedItem = GetTargetedTreeItem(_filesButtonsTree);

                if (clickedItem != null)
                {
                    _filesButtonsTree.SetSelected(clickedItem, 0);

                    var popupMenu = GetNode<PopupMenu>("CanvasLayer/NodeModeGUI/FilePopupMenu");
                    popupMenu.SetPosition((Vector2I)mouseEvent.GlobalPosition);
                    popupMenu.CallDeferred("popup");
                }
            }
        }
    }
    private List<string> GetAllNodeTypes()
    {
        List<string> nodeTypes = new List<string>();

        foreach (string className in ClassDB.GetClassList())
        {
            if (ClassDB.IsParentClass(className, "Node"))
            {
                nodeTypes.Add(className);
            }
        }

        return nodeTypes;
    }
    public void CreateNode(Node node, string name)
    {
        TreeItem Item;

        if (_selectedNode != null)
        {
            _selectedNode.AddChild(node);
            Item = _nodesButtonsTree.CreateItem(_nodesButtonsTree.GetSelected());
            _nodesButtonsTree.GetSelected().Collapsed = false;
        }
        else
        {
            TreeItem RootItem = _nodesButtonsTree.GetRoot();
            RootItem.GetMeta("CorrespondingNode").As<Node>().AddChild(node);
            Item = _nodesButtonsTree.CreateItem(_nodesButtonsTree.GetRoot());
            RootItem.Collapsed = false;
        }


        node.Name = name;
        if (node is Control || node is Node2D)
            node.Set("position", new Vector2(0, 0));


        Item.SetText(0, node.Name);
        Item.SetMeta("CorrespondingNode", node);
        Item.Select(0);
    }

    TreeItem GetTargetedTreeItem(Tree tree)
    {
        return tree.GetItemAtPosition(tree.GetLocalMousePosition());
    }

    // CodeMode Section (Demo pre-alpha beta lambda)
    private string _currentScriptPath = "";
    CodeEdit _codeEdit;
    Script _script;

    public void LoadCode()
    {
        if (_selectedNode.GetScript().As<Script> != null)
        {
            _script = _selectedNode.GetScript().As<CSharpScript>();
            _currentScriptPath = _script.ResourcePath;

            _codeEdit.Text = _script.SourceCode;
        }
    }
    public void SaveCode()
    {
        if (_selectedNode == null) return;

        _script.SourceCode = _codeEdit.Text;

        Node parent = _selectedNode.GetParent();
        int index = _selectedNode.GetIndex();

        if (_script != null)
        {
            ResourceSaver.Save(_script, "C:\\Users\\Jiofef\\AppData\\Roaming\\Godot\\app_userdata\\Little Slippey\\mods\\CustomMap1\\Scripts\\Player.cs");
        }

        var packedScene = new PackedScene();
        packedScene.Pack(_selectedNode);
        _selectedNode.QueueFree();

        Node newNode = packedScene.Instantiate();

        parent.AddChild(newNode);
        parent.MoveChild(newNode, index);

        _selectedNode = newNode;
    }

    public void SetEditingScript(string path)
    {

    }

    // Save Section
    private bool[] _editorCrutches = { true, true, true, true};
    public void SetCrutch(int index, bool value)
    {
        _editorCrutches[index] = value;
    }
    public void SaveLevel()
    {
        _packedLevel = new PackedScene();
        var LevelClone = _level.Duplicate();
        if (_editorCrutches[0])
            LevelClone.ProcessMode = ProcessModeEnum.Always;
        if (_editorCrutches[1])
            LevelClone.GetNode<CanvasLayer>("Pause").Visible = true;
        if (_editorCrutches[2])
            LevelClone.GetNode<CanvasLayer>("EpicIntro").Visible = true;
        if (_editorCrutches[3])
            LevelClone.GetNode<Label>("Level/Player/Camera2D/GUI/Scores").Visible = true;

        _packedLevel.Pack(LevelClone);
        ResourceSaver.Save(_packedLevel, _mapPath);
    }
}