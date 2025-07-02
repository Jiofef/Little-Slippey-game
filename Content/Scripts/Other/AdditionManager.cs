using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using static Additions;

public partial class AdditionManager : Node
{
	private Dictionary<string, AdditionNode> LoadedAdditions = new Dictionary<string, AdditionNode> { };
	[Export] public Node AdditionsWorkPlace;
	public override void _Ready()
	{
		// Registration of pre-created additions
		UpdateRegisteredAdditions();

		foreach (var addition in GetAdditions().Values.Where(v => v.IsActivated))
		{
			AddAddition(addition._PackedScene);
		}
	}

	public void UpdateRegisteredAdditions()
	{
		foreach (var addition in GetChildren().OfType<AdditionNode>())
		{
			RegisterAddition(addition);
		}
	}
	private bool RegisterAddition(AdditionNode addition)
	{
		string typeName = addition.GetType().Name;
		if (LoadedAdditions.ContainsKey(typeName)) return false;

		LoadedAdditions.Add(typeName, addition);
		return true;
	}
	public bool IsAdditionRegistered(string name)
	{
		return LoadedAdditions.ContainsKey(name);
	}
	public void AddAddition(PackedScene addition)
	{
		AddAddition(addition.Instantiate<AdditionNode>());
	}
	public void AddAddition(AdditionNode addition)
	{
		if (RegisterAddition(addition))
			AddChild(addition);
	}
}
