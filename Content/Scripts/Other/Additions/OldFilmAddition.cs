using Godot;
using System;

public partial class OldFilmAddition : AdditionNode
{
	public override void Activate()
	{
		var level7HopelessnessLayer = (CanvasLayer)ResourceLoader.Load<PackedScene>("res://Content/Scenes/Other/Level7HopelessnesLayer.tscn").Instantiate();
		if (ParentManager.IsAdditionRegistered(nameof(ThunderStormAddition)))
			level7HopelessnessLayer.GetNode<VideoStreamPlayer>("VintageFilter").Modulate = new Color(1, 0.8f, 0.55f, 0.2f);
		AddToLevel(level7HopelessnessLayer);
	}

}
