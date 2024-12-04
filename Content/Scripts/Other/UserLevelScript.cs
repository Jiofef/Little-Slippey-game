using Godot;
using System;

public partial class UserLevelScript : Node2D
{
    CharacterBody2D _player;
    public void StartLevel()
    {
        G.ResetValues();
        Input.MouseMode = !GetTree().Paused ? Input.MouseModeEnum.Hidden : Input.MouseModeEnum.Visible;
        AudioServer.SetBusMute(2, Meta.Instance.Sound.BusVolumes[2] <= -30);
        GetNode<AudioStreamPlayer>("../LevelMusicPlayer").StreamPaused = false;
        _player = GetNode<CharacterBody2D>("Player");

        ProcessMode = ProcessModeEnum.Pausable;
    }
    public override void _Ready()
	{
        if (ProcessMode != ProcessModeEnum.Disabled || G.DidLevelIntroPassed)
            StartLevel();
    }

	public override void _PhysicsProcess(double delta)
	{
        if (G.IsDebugEnabled)
        {
            if (Input.IsActionPressed("TeleportDebug"))
                _player.GlobalPosition = GetGlobalMousePosition();

            if (Input.IsActionJustReleased("GetScoreDebug"))
            {
                G.Scores += 5;
                GetNode<CrossSpawner>("../CrossSpawner").Spawner.RecalculateCrossWeight();
            }

            if (Input.IsActionJustReleased("DecraseScoreDebug"))
            {
                G.Scores -= 5;
                GetNode<CrossSpawner>("../CrossSpawner").Spawner.RecalculateCrossWeight();
            }

            if (Input.IsActionJustPressed("InvincibilityDebug"))
            {
                var playerDamageDetector = GetNode<Area2D>("Player/Areas/PlayerDamageDetector");
                playerDamageDetector.Monitoring = !playerDamageDetector.Monitoring;
                GD.Print("Invinciblity: " + !playerDamageDetector.Monitoring);
            }

            if (Input.IsActionJustPressed("PlayerPhysicsDebug"))
            {
                _player.SetPhysicsProcess(!_player.IsPhysicsProcessing());
                GD.Print("PlayerPhysics: " + _player.IsPhysicsProcessing());
            }

            if (Input.IsActionJustPressed("CrossesEnablingDebug"))
            {
                G.IsCrossesEnabled = !G.IsCrossesEnabled;
                GD.Print("CrossesEnabled: " + G.IsCrossesEnabled);
            }
        }

        if (!G.IsProgressPaused)
        {
            G.Scores += 0.016667f;
            if (G.Scores > G.LevelCompleteTime && UnchangableMeta.LevelCompleteStatus[G.CurrentLevel - 1] < 1 + Meta.Instance.Gameplay.Dificulty && G.IsLevelVanilla)
                UnchangableMeta.SaveRecords();
        }
    }
}
