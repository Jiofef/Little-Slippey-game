using Godot;
using System;

public partial class CodeMode : Control // Doesn't work at all yet :D
{
    public LevelEditor L;

    public string _currentScriptPath = "";
    public CodeEdit _codeEdit;
    public Script _script;

    public override void _Ready()
    {
        _codeEdit = GetNode<CodeEdit>("CodeEdit");
    }

    public void LoadCode()
    {
        if (L._selectedNode.GetScript().As<Script> != null)
        {
            _script = L._selectedNode.GetScript().As<CSharpScript>();
            _currentScriptPath = _script.ResourcePath;

            _codeEdit.Text = _script.SourceCode;
        }
    }
    public void SaveCode()
    {
        if (L._selectedNode == null) return;

        _script.SourceCode = _codeEdit.Text;

        Node parent = L._selectedNode.GetParent();
        int index = L._selectedNode.GetIndex();

        if (_script != null)
        {
            ResourceSaver.Save(_script, "C:\\Users\\Jiofef\\AppData\\Roaming\\Godot\\app_userdata\\Little Slippey\\mods\\CustomMap1\\Scripts\\Player.cs");
        }

        var packedScene = new PackedScene();
        packedScene.Pack(L._selectedNode);
        L._selectedNode.QueueFree();

        Node newNode = packedScene.Instantiate();

        parent.AddChild(newNode);
        parent.MoveChild(newNode, index);

        L._selectedNode = newNode;
    }


}
