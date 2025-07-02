using Godot;
using System;

public partial class JiofefsHeadAddition : AdditionNode
{
	public override void Activate()
	{
		var level9JiofefHead = (Node2D)ResourceLoader.Load<PackedScene>("res://Content/Scenes/Other/Level9JiofefHead.tscn").Instantiate();
		AddToLevel(level9JiofefHead);

		level9JiofefHead.GlobalPosition = G.CameraLimits.Position + G.CameraLimits.Size / 2;

		if (G.CameraLimits.Size.X > 25600 || G.CameraLimits.Size.Y > 12800)
			level9JiofefHead.GlobalPosition = G.CameraLimits.Position + new Vector2(1280, 320);
	}
}
