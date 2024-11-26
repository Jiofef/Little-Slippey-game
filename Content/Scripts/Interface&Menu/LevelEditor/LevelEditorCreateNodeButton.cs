using Godot;
using System;

public partial class LevelEditorCreateNodeButton : EnhancedButton
{
	[Export] string _nodeTypeName = "Node", _nodeFileName = "", _description = ""; // if node has prepared layout, you (I) should write its name from the appropriate folder

    public override void _Ready()
    {
        base._Ready();

        var annotationBox = GetTree().Root.GetNode<AnnotationBox>("/root/LevelEditor/GUILayer/AnnotationBox");
        MouseEntered += () => annotationBox.PopupWithText(_description);
        MouseExited += () => annotationBox.Hide();
    }
    public override void _Pressed()
    {
        base._Pressed();

        bool HasNodePath = _nodeFileName != "";
        Node node = !HasNodePath ? (Node)ClassDB.Instantiate(_nodeTypeName) : GD.Load<PackedScene>("res://Content/Scenes/DefaultLevelEditorObjects/" + _nodeFileName + ".tscn").Instantiate();
        GetTree().Root.GetNode("/root/LevelEditor").Call("CreateNode", node, !HasNodePath ? _nodeTypeName : _nodeFileName);

        GetNode<Control>("../../../../").Hide();
    }
}
