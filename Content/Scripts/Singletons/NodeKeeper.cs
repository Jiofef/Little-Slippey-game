using Godot;
using System;
using System.Collections.Generic;

public partial class NodeKeeper : Node
{
	// Singleton
	public static NodeKeeper Instance {get; private set;}
	public override void _Ready()
	{
		Instance = this;
	}

	//
	private static Dictionary<string, Node> _storedNodes;
	private static Dictionary<string, SavedNodeState> _storedNodeStates;
	class SavedNodeState {
		public bool Visible;
		public ProcessModeEnum ProcessMode;

		public SavedNodeState(bool visible, ProcessModeEnum processMode)
		{
			Visible = visible;
			ProcessMode = processMode;
		}
	}
	public static Dictionary<string, Node> GetStoredNodes()
	{
		return _storedNodes;
	}	

	public static void SaveNode(Node node, string name = null)
	{
		string nodeName = name ?? node.Name;

		// Saving in dictionary
		_storedNodes.Add(nodeName, node);
		SavedNodeState state = new SavedNodeState(node.Get("visible").AsBool(), node.ProcessMode);
		_storedNodeStates.Add(nodeName, state);

		// Saving
		node.Reparent(Instance, false);

		// Disabling the physics and visuals
		node.Set("visible", false);
		node.ProcessMode = ProcessModeEnum.Disabled;
	}

	public static NodeType ReturnNode<NodeType>(string nodeName, Node newParent) where NodeType : Node
	{
		if (!_storedNodes.ContainsKey(nodeName)) return null;

		NodeType node = (NodeType)_storedNodes[nodeName];

		// Returning the saved state
		SavedNodeState nodeState = _storedNodeStates[nodeName];
		node.Set("visible", nodeState.Visible);
		node.ProcessMode = nodeState.ProcessMode;

		// Reparenting
		node.Reparent(newParent, false);

		_storedNodes.Remove(nodeName);
		_storedNodeStates.Remove(nodeName);

		return node;
	}

	public static void ClearAllStoredNodes()
	{
		foreach (var node in _storedNodes)
		{
			node.Value.QueueFree();
			_storedNodeStates.Remove(node.Key);
			_storedNodes.Remove(node.Key);
		}
	}
}
