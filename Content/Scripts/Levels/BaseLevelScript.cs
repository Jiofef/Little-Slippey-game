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

    public override void _Ready()
    {
        bool IsCrossesEnhanced = G.CurrentLevel == 10 && G.LevelAdditionalLink == "True" || Meta.Instance.Gameplay.AdditionStatuses[3];
        if (IsCrossesEnhanced)
            SetEnhancedCrossesPack();
        else
            SetDefaultCrossesPack();

        G.ResetValues();


        Input.MouseMode = !GetTree().Paused ? Input.MouseModeEnum.Hidden : Input.MouseModeEnum.Visible;
        AudioServer.SetBusMute(2, Meta.Instance.Sound.BusVolumes[2] <= -30);


        if (G.CurrentLevel == 5 || Meta.Instance.Gameplay.AdditionStatuses[0])
            AddChild((Node2D)ResourceLoader.Load<PackedScene>("res://Content/Scenes/Other/Level5Rain.tscn").Instantiate());
        if (G.CurrentLevel == 7 || Meta.Instance.Gameplay.AdditionStatuses[1])
        {
            var level7HopelessnesLayer = (CanvasLayer)ResourceLoader.Load<PackedScene>("res://Content/Scenes/Other/Level7HopelessnesLayer.tscn").Instantiate();
            if (G.CurrentLevel == 5 || Meta.Instance.Gameplay.AdditionStatuses[0])
                level7HopelessnesLayer.GetNode<VideoStreamPlayer>("VintageFilter").Modulate = new Color(1, 0.8f, 0.55f, 0.2f);
            AddChild(level7HopelessnesLayer);
        }
        if (Meta.Instance.Gameplay.AdditionStatuses[2])
        {
            var level9JiofefHead = (Node2D)ResourceLoader.Load<PackedScene>("res://Content/Scenes/Other/Level9JiofefHead.tscn").Instantiate();
            level9JiofefHead.Position = G.CameraLimits.Position + G.CameraLimits.Size / 2;

            if (G.CameraLimits.Size.X > 25600 || G.CameraLimits.Size.Y > 12800)
                level9JiofefHead.Position = G.CameraLimits.Position + new Vector2(1280, 320);

            AddChild(level9JiofefHead);
        }



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

            if (Input.IsActionJustReleased("DecraseScoreDebug"))
            {
                G.Scores -= 5;
                Crosses.UpdateCrossesWeight();
            }

            if (Input.IsActionJustPressed("InvincibilityDebug"))
            {
                var playerDamageDetector = GetNode<Area2D>("Player/Areas/PlayerDamageDetector");
                playerDamageDetector.Monitoring = !playerDamageDetector.Monitoring;
                GD.Print("Invinciblity: " + !playerDamageDetector.Monitoring);
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
            if(G.Scores > G.LevelCompleteTime && UnchangableMeta.LevelCompleteStatus[G.CurrentLevel - 1] < 1 + Meta.Instance.Gameplay.Dificulty && G.IsLevelVanilla)
                UnchangableMeta.SaveRecords();
        }
    }
}