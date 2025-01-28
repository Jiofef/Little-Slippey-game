using Godot;
using GodotSteam;
using System;
using System.Linq;
using static Crosses;
using static OtherExtension.RandomTools;

public partial class BaseLevelScript : Node2D
{
    [Export] public float BeforeResetCrossesSpawnStartTimer = 0f;
    Random _random = new Random();
    private int[] _crossDefaultWeight = { 600, 170, 80, 40, 110 };
    private float[] _crossWeight = new float[G.CrossesInGameTotal];
    private int _lastAviableCrossNumber = 0;
    private float _weightMultiplierExtenderToCurrentCross = 0;
    private bool _doAllCrossWeigthsSetted;
    private readonly float _floatDelta = 0.016667f;

    public override void _Ready()
    {
        G.ResetValues();
        Input.MouseMode = !GetTree().Paused ? Input.MouseModeEnum.Hidden : Input.MouseModeEnum.Visible;
        AudioServer.SetBusMute(2, Meta.Instance.Sound.BusVolumes[2] <= -30);

        bool IsCrossesEnhanced = G.CurrentLevel == 10 && G.LevelAdditionalLink == "True" || Meta.Instance.Gameplay.AdditionStatuses[3];
        if (IsCrossesEnhanced)
            SetEnhancedCrossesPack();
        else
            SetDefaultCrossesPack();


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
            level9JiofefHead.Position = G.LevelXYSizes[G.CurrentLevel] / 2;

            if (G.CurrentLevel == 8)
                level9JiofefHead.Position = new Vector2(1280, 320);

            AddChild(level9JiofefHead);
        }



        //
        Steam.OverlayToggled += (bool active, bool userInitiated, uint appId) => GetParent().Call("ChangePause", active);
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
                RecalculateCrossWeight();
            }

            if (Input.IsActionJustReleased("DecraseScoreDebug"))
            {
                G.Scores -= 5;
                RecalculateCrossWeight();
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
            _weightMultiplierExtenderToCurrentCross += (_floatDelta * _crossDefaultWeight[_lastAviableCrossNumber]) / 30 * G.CrossesProgressCoeff;
            if(G.Scores > G.LevelCompleteTime && UnchangableMeta.LevelCompleteStatus[G.CurrentLevel - 1] < 1 + Meta.Instance.Gameplay.Dificulty && G.IsLevelVanilla)
                UnchangableMeta.SaveRecords();
        }


        if (G.IsCrossesEnabled && (G.Scores >= BeforeResetCrossesSpawnStartTimer || G.WasTheLevelRestarted))
        {
            int RandomRange = 20 - Meta.Instance.Gameplay.Dificulty * 5;
            RandomRange = (int)((RandomRange - (RandomRange / 2 - G.PlayerMoveCoeff * RandomRange / 2)) / G.CrossSpawnMultiplier);

            if (_random.Next(RandomRange) == 0)
            {
                if (!_doAllCrossWeigthsSetted)
                {
                    if (_crossWeight[_lastAviableCrossNumber] + _weightMultiplierExtenderToCurrentCross < _crossDefaultWeight[_lastAviableCrossNumber])
                    {
                        _crossWeight[_lastAviableCrossNumber] += _weightMultiplierExtenderToCurrentCross;
                        _weightMultiplierExtenderToCurrentCross = 0;
                    }
                    else
                    {
                        _crossWeight[_lastAviableCrossNumber] = _crossDefaultWeight[_lastAviableCrossNumber];
                        _lastAviableCrossNumber++;
                        _weightMultiplierExtenderToCurrentCross = 0;
                    }
                    if (_lastAviableCrossNumber >= 5)
                    {
                        _lastAviableCrossNumber = 4;
                        _doAllCrossWeigthsSetted = true;
                    }
                }
            }
        }
    }
    public void RecalculateCrossWeight()
    {
        float _weightMultiplierExtender = G.Scores / 30 * G.CrossesProgressCoeff;
        _crossWeight = new float[G.CrossesInGameTotal];
        _lastAviableCrossNumber = 0;
        for (int i = 0; _weightMultiplierExtender > 0; i++)
        {
            if (i >= 5)
            {
                _doAllCrossWeigthsSetted = true;
                _lastAviableCrossNumber = 4;
                break;
            }
            if (_crossWeight[_lastAviableCrossNumber] + _weightMultiplierExtender * _crossDefaultWeight[_lastAviableCrossNumber] < _crossDefaultWeight[_lastAviableCrossNumber])
            {
                _crossWeight[_lastAviableCrossNumber] += _weightMultiplierExtender * _crossDefaultWeight[_lastAviableCrossNumber];
                _weightMultiplierExtender = 0;
            }
            else
            {
                _crossWeight[_lastAviableCrossNumber] = _crossDefaultWeight[_lastAviableCrossNumber];
                _lastAviableCrossNumber++;
                _weightMultiplierExtender -= 1;
            }
        }
    }
}