using Godot;
using System;

public partial class Level5Button : LevelButton
{
	public override void _Ready()
	{
		base._Ready();

		InitVisuals();
	}

	public void InitVisuals()
	{
		if (UnchangableMeta.LevelCompleteStatus[4] > 0)
		{
			GetNode<Control>("WindowView").Visible = true;
			GetNode<AnimationPlayer>("WindowView/AnimationPlayer").CurrentAnimation = "LightingAndBlackoutAnimation" + UnchangableMeta.LevelCompleteStatus[4];
			if (UnchangableMeta.LevelCompleteStatus[4] >= 2)
			{
				GetNode<CpuParticles2D>("WindowView/Rain").Color = new Color(0.79f, 0, 0);
				GetNode<CpuParticles2D>("WindowView/Window/Rain").Color = new Color(0.79f, 0, 0);
				GetNode<CpuParticles2D>("WindowView/Fog").Color = new Color(1f, 0, 0);
				GetNode<ColorRect>("WindowView/Fog?").Color = new Color(0.7f, 0, 0);
				GetNode<ColorRect>("WindowView/Fog?Interior").Color = new Color(0.08f, 0.04f, 0.04f);
			}
		}
	}
}
