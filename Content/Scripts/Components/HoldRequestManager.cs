using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

public partial class HoldRequestManager : GodotObject
{
	[Signal] public delegate void RequestsRanOutEventHandler();
	[Signal] public delegate void FirstRequestAppearedEventHandler();
	[Signal] public delegate void NewRequestAppearedEventHandler();
	[Signal] public delegate void RequestRemovedEventHandler();
	[Signal] public delegate void RequestCountChangedEventHandler();
	private int _requests = 0;
	private Dictionary<Node, bool> _requestersStatusesDic = new Dictionary<Node, bool>();
	public void Subscribe(Node node, string requestSignalName, string removeRequestSignalName, bool requestInitialStatus = false)
	{
		if (_requestersStatusesDic.ContainsKey(node))
			return;

		if (!node.HasSignal(requestSignalName))
		{
			GD.PushError($"{node.Name} doesn't have a signal \"{requestSignalName}\"");
			return;
		}

		if (!node.HasSignal(removeRequestSignalName))
		{
			GD.PushError($"{node.Name} doesn't have a signal \"{removeRequestSignalName}\"");
			return;
		}

		///
		_requestersStatusesDic.Add(node, requestInitialStatus);
		node.Connect(Node.SignalName.TreeExited, new Callable(this, nameof(Unsubscribe)));

	}
	public void Unsubscribe(Node node)
	{
		if (!_requestersStatusesDic.ContainsKey(node))
			return;

		_requestersStatusesDic.Remove(node);
		node.Disconnect(Node.SignalName.TreeExited, new Callable(this, nameof(Unsubscribe)));
	}
	public int GetRequestsCount() => _requests;

	private void Update()
	{

	}
}