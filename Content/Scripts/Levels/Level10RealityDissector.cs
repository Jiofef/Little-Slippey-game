using Godot;
using System;

public partial class Level10RealityDissector : Node
{
    [Signal] public delegate void ChangeSceneEventHandler();

	public override void _Ready()
	{
		G.CrossSpawnMultiplier = 0.25f;
		G.IsProgressPaused = true;
    }


	public override void _PhysicsProcess(double delta)
	{
		if (!G.IsPlayerDead)
		{
            G.Scores += 0.016667f;
            if (G.Scores > 10)
                G.Scores = 1;
        }
		else 
			G.Scores = 0;
	}

	public void ChangeLevelToTrueVersion()
	{
		G.TransitiveVariantD.Add("ImFromTheFakeLevel10", true);
		if (!UnchangableMeta.IsFakeLevel10SkipAllowed)
			UnchangableMeta.IsFakeLevel10SkipAllowed = true;
		G.LevelAdditionalLink = "True";
		EmitSignal("ChangeScene");
		G.MusicStopTimeCode = 0;
    }
}
