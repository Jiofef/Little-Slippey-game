using Godot;
using System;

public partial class LevelEditorCreateNodeButton : EnhancedButton
{
	[Export] string _nodeTypeName = "Node", _nodeFileName = ""; // if node has prepared layout, you (I) should write its name from the appropriate folder

    public override void _Pressed()
    {
        base._Pressed();

        bool HasNodePath = _nodeFileName != "";
        Node node = !HasNodePath ? (Node)ClassDB.Instantiate(_nodeTypeName) : GD.Load<PackedScene>("res://Content/Scenes/DefaultLevelEditorObjects/" + _nodeFileName + ".tscn").Instantiate();
        GetTree().Root.GetNode("/root/LevelEditor").Call("CreateNode", node, !HasNodePath ? _nodeTypeName : _nodeFileName);

        GetNode<Control>("../../../../").Hide();
    }
}
