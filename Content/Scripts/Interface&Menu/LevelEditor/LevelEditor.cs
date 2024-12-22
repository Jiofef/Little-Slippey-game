using Godot;
using System;
using System.IO;
using System.Linq;

public partial class LevelEditor : Control
{
    // General using variables
    #region

    public string _mapPath = "", _mapFolder = "";

    public PackedScene _packedLevel;

    public Node _main, _level;
    public string _mainLevelSceneName = "Level";

    public Node[] _allTheNodes = new Node[0];
    public string[] _allTheFiles = new string[0];


    public bool _seeAllTheNodes = false;

    /// <summary>
    /// <br>1 - Disable the ProcessMode of Main when entering the editor and enable it when loading the level</br>
    /// <br>2 - Disable the ProcessMode of EpicIntro when entering the editor and enable it when loading the level</br>
    /// <br>3 - Disable the visibility of the pause when entering the editor and enable it when loading the level</br>
    /// <br>4 - Disable the visibility of the intro level when entering the editor and enable it when loading the level</br>
    /// <br>5 - Disable the visibility of scores when entering the editor and enable it when loading a level</br>
    /// </summary>
    public bool[] _editorCrutches = { true, true, true, true, true };


    public enum EditorMode { TileMode, NodeMode, CodeMode, SavePage }
    public EditorMode _editorMode = EditorMode.TileMode;

    public Control _currentModeGui, _modeGuiControl;

    public Camera2D _camera;
    public Timer _guiDelayTimer;
    public TextureButton _screenButton;


    public Node _selectedNode;
    public string _selectedFilePath = "";


    public Sprite2D _extraCursor;

    public enum ExtraCursorMode { Null, HoldingNode, HoldingFile}
    public ExtraCursorMode _extraCursorMode = ExtraCursorMode.Null;

    public string _pickedTreeItem;
    public Vector2 _itemPickMousePos;
    #endregion


    public override void _Ready()
    {
        _mapPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + @"\Godot\app_userdata\Little Slippey\mods\" + G.ModMapPath;
        _mapFolder = G.ModMapFolder;

        G.IsLevelVanilla = false;
        _main = (Node2D)ResourceLoader.Load<PackedScene>(_mapPath).Instantiate();

        GetTree().Paused = true;

        GetNode("LevelContainer").AddChild(_main);

        _level = _main.GetNode(_mainLevelSceneName);
        _camera = GetNode<Camera2D>("Camera2D");
        _screenButton = GetNode<TextureButton>("GUILayer/ScreenButton");
        _modeGuiControl = GetNode<Control>("GUILayer/ModeGUIControl");
        _extraCursor = GetNode<Sprite2D>("GUILayer/ExtraCursor");
        _guiDelayTimer = GetNode<Timer>("GUILayer/GUIDelayTimer");


        if (_editorCrutches[0])
            _main.ProcessMode = ProcessModeEnum.Disabled;
        if (_editorCrutches[1])
            _main.GetNode("EpicIntro").ProcessMode = ProcessModeEnum.Disabled;
        if (_editorCrutches[2])
            _main.GetNode<CanvasLayer>("Pause").Visible = false;
        if (_editorCrutches[3])
            _main.GetNode<CanvasLayer>("EpicIntro").Visible = false;
        if (_editorCrutches[4])
            _main.GetNode<Label>("Level/Player/Camera2D/GUI/Scores").SetDeferred("visible", false);
    }

    public Vector2 _globalMousePos, _localMousePos, _guiMousePos, _globalMouseLastFramePos = new Vector2(), _localMouseLastFramePos;
    public override void _PhysicsProcess(double delta)
    {
        //Mouse processing
        {
            _globalMouseLastFramePos = _globalMousePos;
            _localMouseLastFramePos = _localMousePos;

            _globalMousePos = GetGlobalMousePosition();
            _localMousePos = _camera.GetLocalMousePosition();
            _guiMousePos = _modeGuiControl.GetLocalMousePosition();

            if (Input.IsActionJustPressed("MouseRightClick"))
            {
                if (_guiDelayTimer.IsStopped() && _editorMode == EditorMode.NodeMode)
                {
                    var nodeMode = (NodeMode)_currentModeGui;
                    if (_currentModeGui.GetNode<PopupMenu>("NodePopupMenu").Visible)
                        _currentModeGui.GetNode<PopupMenu>("NodePopupMenu").CallDeferred("hide");
                    else if (_currentModeGui.GetNode<PopupPanel>("NodeNameEditPopup").Visible)
                        _currentModeGui.GetNode<PopupPanel>("NodeNameEditPopup").Hide();
                    else if (_currentModeGui.GetNode<PopupMenu>("FilePopupMenu").Visible)
                        _currentModeGui.GetNode<PopupMenu>("FilePopupMenu").CallDeferred("hide");
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
            else if (Input.IsActionJustReleased("MouseLeftClick") && _editorMode == EditorMode.NodeMode)
            {
                var nodeMode = (NodeMode)_currentModeGui;
                _pickedTreeItem = "";
                _extraCursorMode = ExtraCursorMode.Null;
                _extraCursor.Texture = null;

                var NodesButtonsContainer = nodeMode.GetNode<Tree>("NodesButtonsTree");
                var FilesButtonsContainer = nodeMode.GetNode<Tree>("FilesButtonsTree");

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

                    string OldPath = SelectedFilePath, NewPath = TargetItem.GetMeta("FilePath").ToString() + @"\" + nodeMode.GetFileName(_selectedFilePath);
                    nodeMode.MoveFile(OldPath, TargetItem.GetMeta("FilePath").ToString() + @"\" + nodeMode.GetFileName(_selectedFilePath));
                    SelectedItem.SetMeta("FilePath", NewPath);
                    _selectedFilePath = NewPath;

                    MoveTreeItem(SelectedItem, TargetItem);
                    Recursion(SelectedItem);

                    void Recursion(TreeItem rootItem)
                    {
                        foreach (TreeItem childItem in rootItem.GetChildren())
                        {
                            childItem.SetMeta("FilePath", rootItem.GetMeta("FilePath").ToString() + @"\" + nodeMode.GetFileName(childItem.GetMeta("FilePath").ToString()));
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
                                nodeMode.CreateNode(node, node.Name);

                                if (node is Node2D or Control)
                                    node.Set("global_position", _globalMousePos);
                                break;

                            case ".png" or ".jpg":
                                var sprite = new Sprite2D
                                {
                                    Texture = FileSystemExtension.LoadNonResourceImage(_selectedFilePath),
                                    Scale = new Vector2(4, 4)
                                };

                                nodeMode.CreateNode(sprite, nodeMode.GetFileName(_selectedFilePath));
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
                                nodeMode.CreateNode(node, node.Name);
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

                                    nodeMode.CreateNode(sprite, nodeMode.GetFileName(SelectedFilePath));
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
                    nodeMode.SaveNodeTo(_selectedNode, TargetFilePath);
                }
            }
        }

    }

    // General functions

    public void UpdateAllNodesArray() // Updates array with all nodes from the level scene
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
        Recursion(_main);
    }

    public void SetOption(Variant value, string option) // A little crutch for godot signals that gives the value argument first
    {
        Set(option, value);
    }

    public void ChangeZoom(float value, bool stickToTheCursor = true) // Changing main camera zoom
    {
        float NewZoom = Mathf.Clamp(_camera.Zoom.X * value, 1f / 16, 1 * 16);

        if (stickToTheCursor && _camera.Zoom != new Vector2(NewZoom, NewZoom))
            _camera.Position += _camera.GetLocalMousePosition() * 0.25f * (value > 1 ? 1 : -1);

        _camera.Zoom = new Vector2(NewZoom, NewZoom);
    }

    public void MoveTreeItem(TreeItem item, TreeItem target, bool UnCollapseTarget = true) // Moves item to another parent in any Tree
    {
        item.GetParent().RemoveChild(item);

        target.AddChild(item);

        if (UnCollapseTarget)
            target.Collapsed = false;
    }

    public void SetEditorMode(int value) // Does what it says
    {
        if (_currentModeGui != null)
            _currentModeGui.QueueFree();

        _editorMode = (EditorMode)value;
        _currentModeGui = ((PackedScene)GD.Load("res://Content/Scenes/Interface&Menu/LevelEditor/" + _editorMode.ToString() + ".tscn")).Instantiate<Control>();
        _currentModeGui.Set("L", this);
        GetNode<TileMap>("AssistiveTileMap").Visible = _editorMode == EditorMode.TileMode;
        GetNode<TileMap>("ErasingAssistiveTileMap").Visible = _editorMode == EditorMode.TileMode;
        UpdateAllNodesArray();

        _modeGuiControl.AddChild(_currentModeGui);
    }

    public void KILLCHILDREN(Node node) // Deletes all child nodes from given node
    {
        foreach (Node child in node.GetChildren())
            child.QueueFree();
    }

    public TreeItem GetTargetedTreeItem(Tree tree) // Returns TreeItem under the cursor in given tree
    {
        return tree.GetItemAtPosition(tree.GetLocalMousePosition());
    }

    public void OnTreeitemTaken(string name) //Name is the type of picked item. Used for setting ExtraCursorMode
    {
        _pickedTreeItem = name;
        _itemPickMousePos = _localMousePos;
    }


    public void TestLevel() // Doesn't work well yet
    {
        GetTree().Paused = false;
        SaveLevel();
        GetTree().ChangeSceneToPacked(_packedLevel);
    }
    public void SaveLevel() // Doesn't work well as well
    {
        _packedLevel = new PackedScene();
        var LevelClone = _main.Duplicate();
        if (_editorCrutches[0])
            LevelClone.ProcessMode = ProcessModeEnum.Pausable;
        if (_editorCrutches[1])
            LevelClone.GetNode("EpicIntro").ProcessMode = ProcessModeEnum.Always;
        if (_editorCrutches[2])
            LevelClone.GetNode<CanvasLayer>("Pause").Visible = true;
        if (_editorCrutches[3])
            LevelClone.GetNode<CanvasLayer>("EpicIntro").Visible = true;
        if (_editorCrutches[4])
            LevelClone.GetNode<Label>("Level/Player/Camera2D/GUI/Scores").Visible = true;


        _packedLevel.Pack(LevelClone);
        ResourceSaver.Save(_packedLevel, _mapPath);
    }
    public void Leave()
    {
        GetTree().Paused = false;
        GetTree().ChangeSceneToFile("res://Content/Scenes/Interface&Menu/WelcomeToGOS.tscn");
    }
    public void Crutch()
    {
        ColorRect colorRect1 = new ColorRect();
        ColorRect colorRect2 = new ColorRect();
        ColorRect colorRect3 = new ColorRect();
        colorRect2.Owner = colorRect1;
        colorRect3.Owner = colorRect2;

        //PackedScene packedScene = new PackedScene();
        //packedScene.
        //packedScene.Pack(colorRect1);

        //ResourceSaver.Save(packedScene, @"C:\Users\Jiofef\AppData\Roaming\Godot\app_userdata\Little Slippey\mods\CustomMap1\Other\sas.tscn");
    }
}