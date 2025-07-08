using Godot;
using System;
using OtherExtension;
using GodotPlugins.Game;
using static IActivatableLevelObj;
using System.Threading.Tasks;

public abstract partial class AdditionNode : Node, IActivatableLevelObj
{
	public virtual ActivationConditionEnum ActivationCondition { get; set; }
	public AdditionNode()
	{
		ActivationCondition = ActivationConditionEnum.OnLevelStarted;
	}
	protected AdditionManager ParentManager;
	public override void _Ready()
	{
		ParentManager = GetParentOrNull<AdditionManager>();
		if (ParentManager == null)
		{
			GD.PrintErr($"Additions can be only \"AdditionManager\"'s children. The {Name} cannot be loaded");
			QueueFree();
			return;
		}
		((IActivatableLevelObj)this).InitIActivatableLevelObj();
	}

	public void AddToLevel(Node node)
	{
		ParentManager.AdditionsWorkPlace.AddChild(node);
	}
	public virtual void Activate() { }
}
