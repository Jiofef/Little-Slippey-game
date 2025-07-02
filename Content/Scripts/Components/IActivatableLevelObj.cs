// This interface is not directly related to objects at the level. This was primarily done for add-ons such as thunderstorm, which should only be activated after the level opening screen. However, based on this interface, many things can be scripted easily
using Godot;
using OtherExtension;

public interface IActivatableLevelObj
{
	public enum ActivationConditionEnum { OnReady, CustomActivate, OnTreeExited, OnLevelStarted, OnLevelFinished, OnPlayedDead }
	public ActivationConditionEnum ActivationCondition { get; }
	public virtual void Activate() { }
	public void InitIActivatableLevelObj(Node node)
	{
		switch (ActivationCondition)
		{
			case ActivationConditionEnum.OnReady:
				node.CallDeferred(nameof(Activate));
				break;
			case ActivationConditionEnum.OnTreeExited:
				node.Connect(Node.SignalName.TreeExited, new Callable(node, nameof(Activate)));
				break;
			case ActivationConditionEnum.OnLevelStarted:
				ActionTools.BindEventToNodeSafelyWithoutArgs(node, nameof(Activate), G.OnLevelStarted);
				break;
		}
	}
}