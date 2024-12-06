using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

public partial class NodeMode : Control
{
    public LevelEditor L;

    [Signal] public delegate void NodeMovedByMouseEventHandler();
    public NodeMovedByMouseEventHandler _lastConnectedPositionPropertyUpdate;

    public Vector2 _selectedNodeGlobalPosition, _smartPositioningDistance = new Vector2(32, 32); bool _doSmartPos = false; // for "smart" positioning

    public Tree _nodesButtonsTree;
    public override void _Ready()
    {
        GetTree().Root.FilesDropped += AddFile;
        TreeExiting += () => GetTree().Root.FilesDropped -= AddFile;
        GetNode<Tree>("NodesButtonsTree").Connect("gui_input", new Callable(this, "OnNodesTreeGuiInput"));
        GetNode<Tree>("FilesButtonsTree").Connect("gui_input", new Callable(this, "OnFilesTreeGuiInput"));

        _filesButtonsTree = GetNode<Tree>("FilesButtonsTree");
        _nodesButtonsTree = GetNode<Tree>("NodesButtonsTree");

        UpdateVisibleNodesButtons();
        UpdateVisibleFiles();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (L._screenButton.ButtonPressed && Input.IsActionJustPressed("MouseLeftClick"))
        {
            if (L._selectedNode is Node2D or Control)
                _selectedNodeGlobalPosition = (Vector2)L._selectedNode.Get("global_position");
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
        else if (L._screenButton.ButtonPressed && Input.IsMouseButtonPressed(MouseButton.Left) && L._selectedNode != null)
        {

            if (L._selectedNode is Node2D or Control)
            {
                _selectedNodeGlobalPosition -= L._globalMouseLastFramePos - L._globalMousePos;
                L._selectedNode.Set("global_position", _selectedNodeGlobalPosition - _selectedNodeGlobalPosition % _smartPositioningDistance);
            }
            EmitSignal("NodeMovedByMouse");
        }
    }

    private void SetSelectedNode(Node node)
    {
        L._selectedNode = node;
        if (L._selectedNode is Node2D or Control)
            _selectedNodeGlobalPosition = (Vector2)L._selectedNode.Get("global_position");
    }
    private void SelectNode()
    {
        if (Input.IsActionJustPressed("MouseLeftClick"))
            L.OnTreeitemTaken("Node");

        Node PreviousSelectedNode = L._selectedNode;
        SetSelectedNode((Node)_nodesButtonsTree.GetSelected().GetMeta("CorrespondingNode"));
        UpdateVisibleProperties();

        if (Input.IsMouseButtonPressed(MouseButton.Right))
        {
            var nodePopupMenu = GetNode<PopupMenu>("NodePopupMenu");

            nodePopupMenu.Position = (Vector2I)GetLocalMousePosition();
            nodePopupMenu.Popup();
            L._guiDelayTimer.Start(0.02f);
        }
        else if (PreviousSelectedNode == L._selectedNode)
            NodePopupAction(4);
    }
    public void NodePopupAction(int actionIndex)
    {
        Node node;

        TreeItem SelectedItem = _nodesButtonsTree.GetSelected();

        switch (actionIndex)
        {
            case 0:
                GetNode<Control>("NodeCreateMenu").Show();
                break;
            case 1:
                G.NodeCopyBuffer.Clear();
                G.NodeCopyBuffer.Add(L._selectedNode.Duplicate());
                L._selectedNode.QueueFree();
                L._selectedNode = null;
                _nodesButtonsTree.GetSelected().Free();
                break;
            case 2:
                G.NodeCopyBuffer.Clear();
                G.NodeCopyBuffer.Add(L._selectedNode.Duplicate());
                break;
            case 3:
                node = G.NodeCopyBuffer[0].Duplicate();
                L._selectedNode.AddChild(node);
                node.Name = G.NodeCopyBuffer[0].Name;

                CreateNodeItem(node);
                break;
            case 4:
                var lineEditPopup = GetNode<PopupPanel>("NodeNameEditPopup");

                lineEditPopup.Position = (Vector2I)GetLocalMousePosition();
                lineEditPopup.Popup();
                lineEditPopup.GetNode<LineEdit>("LineEdit").Text = L._selectedNode.Name;
                L._guiDelayTimer.Start(0.02f);
                break;
            case 5:
                var Duplicate = L._selectedNode.Duplicate();
                L._selectedNode.GetParent().AddChild(Duplicate);
                Duplicate.Name = L._selectedNode.Name;

                CreateNodeItem(Duplicate, SelectedItem.GetParent());
                break;
            case 6:
                SaveNodeTo(L._selectedNode, L._mapFolder);
                break;
            case 7:
                DisplayServer.ClipboardSet(L._level.GetPathTo(L._selectedNode));
                break;
            case 8:
                DeleteNodeByItem(SelectedItem);
                break;
        }
    }
    private void SetNodeName(string value)
    {
        if (L._selectedNode != null)
        {
            L._selectedNode.Name = value;
            GetNode<Tree>("NodesButtonsTree").GetSelected().SetText(0, L._selectedNode.Name);
        }
        GetNode<PopupPanel>("NodeNameEditPopup").Hide();
    }
    private void UpdateVisibleNodesButtons()
    {
        _nodesButtonsTree.Clear();

        ButtonGroup buttonGroup = new ButtonGroup();

        void Recursion(Node rootNode, TreeItem item)
        {
            foreach (Node node in rootNode.GetChildren())
            {
                var NextItem = item.CreateChild();
                NextItem.SetText(0, node.Name);
                NextItem.SetMeta("CorrespondingNode", node);
                NextItem.Collapsed = true;
                L._allTheNodes = L._allTheNodes.Append(node).ToArray();
                Recursion(node, NextItem);
            }
        }
        var Item = _nodesButtonsTree.CreateItem();
        Item.SetText(0, L._mainLevelScene.Name);
        Item.SetMeta("CorrespondingNode", L._mainLevelScene);
        Recursion(L._mainLevelScene, Item);

    }
    public void SaveNodeTo(Node node, string directory)
    {
        PackedScene packedScene = new PackedScene();
        packedScene.Pack(node.Duplicate());

        string path = directory + @"\" + node.Name + ".tscn";

        ResourceSaver.Save(packedScene, path);
        CreateFileItem(path, L.GetTargetedTreeItem(_filesButtonsTree));
    }
    private void DeleteNodeByItem(TreeItem nodeItem)
    {
        if (nodeItem.GetTree() != _nodesButtonsTree) return;

        var node = (Node)nodeItem.GetMeta("CorrespondingNode");
        L._allTheNodes = L._allTheNodes.Where(x => x != node).ToArray();
        if (L._selectedNode == node)
            L._selectedNode = null;
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

    private void OnNodesTreeGuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent)
        {
            if (mouseEvent.ButtonIndex == MouseButton.Right && mouseEvent.Pressed)
            {
                TreeItem clickedItem = L.GetTargetedTreeItem(_nodesButtonsTree);

                if (clickedItem != null)
                {
                    _nodesButtonsTree.SetSelected(clickedItem, 0);

                    var popupMenu = GetNode<PopupMenu>("NodePopupMenu");
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

        if (L._selectedNode != null)
        {
            L._selectedNode.AddChild(node);
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

    public string[] _propertyBlackList =
{
        "global_position", "global_rotation", "global_transform", "global_scale", "global_skew", "global_rotation_degrees", "rotation", "import_path", "name", "owner", "multiplayer", "process"
    };
    private void UpdateVisibleProperties()
    {
        var propertiesGrid = GetNode<GridContainer>("PropertiesGridContainer/VBoxContainer/PropertiesGrid");
        L.KILLCHILDREN(propertiesGrid);

        foreach (Godot.Collections.Dictionary item in L._selectedNode.GetPropertyList())
        {
            AddPropertyToGrid(propertiesGrid, item);
        }
        void AddPropertyToGrid(GridContainer propertiesGrid, Godot.Collections.Dictionary property)
        {
            string propertyName = property["name"].ToString();

            if (_propertyBlackList.Contains(propertyName)) return;

            var propertyLabel = new Label { Text = propertyName.Capitalize(), Theme = GD.Load<Theme>("res://Content/Other/LevelEditor.theme") };
            propertiesGrid.AddChild(propertyLabel);

            var propertyValue = L._selectedNode.Get(propertyName);
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
            checkBox.Toggled += (v) => SetProperty(L._selectedNode, propertyName, Variant.Type.Bool, v);

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
            spinBox.ValueChanged += (v) => SetProperty(L._selectedNode, propertyName, propertyType, v, optionalKey);
            return spinBox;
        }

        LineEdit CreateLineEdit(string propertyName, string value)
        {
            var lineEdit = new LineEdit { Text = value };
            lineEdit.TextChanged += (v) => SetProperty(L._selectedNode, propertyName, Variant.Type.String, v);

            return lineEdit;
        }

        ColorPickerButton CreateColorPicker(string propertyName, Color value)
        {
            var colorPickerButton = new ColorPickerButton { Color = value };
            colorPickerButton.ColorChanged += (c) => SetProperty(L._selectedNode, propertyName, Variant.Type.Color, c);
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

    public void SetDoSmartPos(bool value)
    {
        _doSmartPos = value;
        _smartPositioningDistance = _doSmartPos ? new Vector2(32, 32) : new Vector2(1, 1);
    }
    public void SetSeeAllTheNodes(bool value)
    {
        L._seeAllTheNodes = value;
        L._mainLevelScene = L._seeAllTheNodes ? L._level : L._level.GetNode(L._mainLevelSceneName);
        UpdateVisibleNodesButtons();
    }

    //Files Section
    public Tree _filesButtonsTree;

    public void UpdateVisibleFiles()
    {
        _filesButtonsTree.Clear();

        ButtonGroup buttonGroup = new ButtonGroup();


        void Recursion(string rootFile, TreeItem item)
        {
            foreach (string folder in Directory.GetDirectories(rootFile))
            {
                var NextItem = item.CreateChild();
                NextItem.SetText(0, folder.Remove(0, rootFile.Length + 1));
                NextItem.SetMeta("FilePath", folder);
                NextItem.Collapsed = true;
                L._allTheFiles = L._allTheFiles.Append(folder).ToArray();
                Recursion(folder, NextItem);
            }

            foreach (string file in Directory.GetFiles(rootFile))
            {
                if (HasStringFormats(file, new string[] { "tscn", "png", "jpg", "cs", "gd", "json", "ogv", "gdshader", "mp3", "mp4", "ogg", "wav", "theme", "ttf", "tres" }))
                {
                    var NextItem = item.CreateChild();
                    NextItem.SetText(0, file.Remove(0, rootFile.Length + 1));
                    NextItem.SetMeta("FilePath", file);
                    NextItem.Collapsed = true;
                    L._allTheFiles = L._allTheFiles.Append(file).ToArray();
                }
            }
        }
        var Item = _filesButtonsTree.CreateItem();
        Item.SetText(0, L._mapFolder.Remove(0, L._defaultPath.Length));
        Item.SetMeta("FilePath", L._mapFolder);
        Recursion(L._mapFolder, Item);
    }
    public void SetSelectedFile(string path)
    {
        L._selectedFilePath = path;
    }
    public void FilePopupAction(int actionIndex)
    {
        TreeItem SelectedItem = _filesButtonsTree.GetSelected();

        switch (actionIndex)
        {
            case 0:
                for (int i = 0; ; i++)
                {
                    var SuggestedPath = L._selectedFilePath + @"\NewFolder" + i;
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
                DisplayServer.ClipboardSet(ProjectSettings.LocalizePath(ProjectSettings.LocalizePath(L._selectedFilePath)));
                break;
            case 3:
                DisplayServer.ClipboardSet(L._selectedFilePath);
                break;
            case 4:
                var lineEditPopup = GetNode<PopupPanel>("FileNameEditPopup");

                lineEditPopup.Position = (Vector2I)GetLocalMousePosition();
                lineEditPopup.Popup();
                lineEditPopup.GetNode<LineEdit>("LineEdit").Text = GetNode<Tree>("FilesButtonsTree").GetSelected().GetText(0);
                L._guiDelayTimer.Start(0.02f);
                break;
            case 5:

                break;
            case 6:
                Process.Start("explorer.exe", L._selectedFilePath.Remove(L._selectedFilePath.RFind(@"\")));
                break;
            case 7:
                Process.Start("explorer.exe", L._selectedFilePath);
                break;
        }
    }
    public void SetFileName(string value)
    {
        if (L._selectedFilePath != null)
        {
            string NewPath = GetFilePath(L._selectedFilePath) + value;

            MoveFile(L._selectedFilePath, NewPath);

            var treeItem = _filesButtonsTree.GetSelected();
            treeItem.SetText(0, value);
            treeItem.SetMeta("FilePath", NewPath);

            L._selectedFilePath = NewPath;
        }
        GetNode<PopupPanel>("FileNameEditPopup").Hide();
    }
    public void SelectFile()
    {
        if (Input.IsActionJustPressed("MouseLeftClick"))
            L.OnTreeitemTaken("File");

        string PreviousSelectedFile = L._selectedFilePath;
        SetSelectedFile(_filesButtonsTree.GetSelected().GetMeta("FilePath").ToString());

        //UpdateVisibleProperties();

        if (Input.IsMouseButtonPressed(MouseButton.Right))
        {
            var filePopupMenu = GetNode<PopupMenu>("FilePopupMenu");

            filePopupMenu.Position = (Vector2I)GetLocalMousePosition();
            filePopupMenu.Popup();
            L._guiDelayTimer.Start(0.02f);
        }
        //else if (PreviousSelectedFile == _selectedFile)
        //NodePopupAction(4);
    }
    public void MoveFile(string file, string targetPath)
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
    public void DeleteFileByItem(TreeItem fileItem)
    {
        if (fileItem.GetTree() != _filesButtonsTree) return;

        var path = (string)fileItem.GetMeta("FilePath");
        L._allTheFiles = L._allTheFiles.Where(x => x != path).ToArray();
        if (L._selectedFilePath == path)
            L._selectedFilePath = null;
        DirAccess.RemoveAbsolute(path);
        fileItem.Free();
    }

    private void OnFilesTreeGuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent)
        {
            if (mouseEvent.ButtonIndex == MouseButton.Right && mouseEvent.Pressed)
            {
                TreeItem clickedItem = L.GetTargetedTreeItem(_filesButtonsTree);

                if (clickedItem != null)
                {
                    _filesButtonsTree.SetSelected(clickedItem, 0);

                    var popupMenu = GetNode<PopupMenu>("FilePopupMenu");
                    popupMenu.SetPosition((Vector2I)mouseEvent.GlobalPosition);
                    popupMenu.CallDeferred("popup");
                }
            }
        }
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
        L._allTheFiles = L._allTheFiles.Append(path).ToArray();
        return Item;
    }

    //String Tweaks
    public bool HasStringFormats(string value, string[] formats)
    {
        foreach (string format in formats)
            if (value.Contains("." + format))
                return true;
        return false;
    }
    public bool DoesStringHaveFormat(string value, string format)
    {
        return value.Contains("." + format);
    }
    public string GetFileName(string path)
    {
        return path.Remove(0, path.RFind(@"\") + 1);
    }
    public string GetFilePath(string path)
    {
        return path.Remove(path.RFind(@"\")) + @"\";
    }
    //String Tweaks
}
