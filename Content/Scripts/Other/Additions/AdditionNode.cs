using Godot;
using System;
using OtherExtension;
using GodotPlugins.Game;
using static IActivatableLevelObj;

public abstract partial class AdditionNode : Node, IActivatableLevelObj
{
	public virtual ActivationConditionEnum ActivationCondition { get; set; }
	public AdditionNode()
	{
		ActivationCondition = ActivationConditionEnum.OnLevelStarted;
	}
	protected AdditionManager ParentManager => GetParent<AdditionManager>();
	public override void _Ready()
	{
		if (GetParentOrNull<AdditionManager>() == null)
		{
			GD.PrintErr($"Additions can be only \"AdditionManager\"'s children. The {Name} cannot be loaded");
			QueueFree();
			return;
		}

		((IActivatableLevelObj)this).InitIActivatableLevelObj(this);
	}

	public void AddToLevel(Node node)
	{
		ParentManager.AdditionsWorkPlace.AddChild(node);
	}
	public virtual void Activate() { }
}
