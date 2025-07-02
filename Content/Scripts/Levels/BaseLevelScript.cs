using Godot;
using GodotSteam;
using System;
using System.Linq;
using static Crosses;
using static OtherExtension.RandomTools;

public partial class BaseLevelScript : Node2D
{
    [Export] public float BeforeResetCrossesSpawnStartTimer = 0f;

    private readonly float _floatDelta = 0.016667f;

	[Signal] public delegate void LevelFinishedEventHandler();
	public bool IsLevelFinished { get; private set; } = false;

    public override void _Ready()
	{		
		AudioServer.SetBusMute(2, Meta.Instance.Sound.BusVolumes[2] <= -30);

		Steam.InputDeviceDisconnected += (junk) => GetParent().Call("ChangePause", true);
	}

    public void StartLevel()
    {
        GetTree().Paused = false;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (G.IsDebugEnabled)
        {
            if (Input.IsActionPressed("TeleportDebug"))
                G.Player.GlobalPosition = GetGlobalMousePosition();

            if (Input.IsActionJustReleased("GetScoreDebug"))
            {
                G.Scores += 5;
                Crosses.UpdateCrossesWeight();
            }

            if (Input.IsActionJustReleased("DecreaseScoreDebug"))
            {
                G.Scores -= 5;
                Crosses.UpdateCrossesWeight();
            }

            if (Input.IsActionJustPressed("InvincibilityDebug"))
            {
                var playerDamageDetector = GetNode<Area2D>("Player/Areas/PlayerDamageDetector");
                playerDamageDetector.Monitoring = !playerDamageDetector.Monitoring;
                GD.Print("Invincibility: " + !playerDamageDetector.Monitoring);
            }

            if (Input.IsActionJustPressed("PlayerPhysicsDebug"))
            {
                G.Player.SetPhysicsProcess(!G.Player.IsPhysicsProcessing());
                GD.Print("PlayerPhysics: " + G.Player.IsPhysicsProcessing());
            }

            if (Input.IsActionJustPressed("CrossesEnablingDebug"))
            {
                G.IsCrossesEnabled = !G.IsCrossesEnabled;
                GD.Print("CrossesEnabled: " + G.IsCrossesEnabled);
            }
        }

        if (!G.IsProgressPaused)
        {
            G.Scores += _floatDelta;
			if (G.Scores > G.LevelCompleteTime && !IsLevelFinished)
			{
				IsLevelFinished = true;
				UnchangableMeta.SaveRecords();

				EmitSignal(nameof(LevelFinished));
			}
        }
    }
}