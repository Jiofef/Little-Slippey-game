using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class DisposeController : GodotObject
{
	private Node _controlledNode;
	public bool IsDisposed { get; private set; }
	public DisposeController(Node node)
	{
		_controlledNode = node;
		node.Connect(Node.SignalName.TreeExited, new Callable(this, nameof(OnTreeExited)));
		node.Connect(Node.SignalName.TreeEntered, new Callable(this, nameof(OnTreeEntered)));
	}

	private void OnTreeExited()
	{
		IsDisposed = true;
	}
	private void OnTreeEntered()
	{
		IsDisposed = false;
	}
}