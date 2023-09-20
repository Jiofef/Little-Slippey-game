using Godot;
using System;
using System.Linq;

public partial class BaseLevelScript : Node2D
{
    [Signal] public delegate void LevelReloadEventHandler();
    PackedScene[] _crosses = new PackedScene[G.CrossesInGameTotal];
    CharacterBody2D _player;

    Random _random = new Random();

    private float[] _crossWeight = new float[G.CrossesInGameTotal];
    private int _lastAviableCrossNumber = 0;
    private float _weightMultiplierExtenderToCurrentCross = 0;
    private bool _doAllCrossWeigthsSetted;
    private readonly float _floatDelta = 0.016667f;

    public override void _Ready()
    {
        G.ResetValues();
        AudioServer.SetBusMute(2, Meta.Instance.BusVolumes[2] <= -30);
        GetNode<AudioStreamPlayer>("../../LevelMusicPlayer").StreamPaused = false;
        _player = GetNode<CharacterBody2D>("Player");
        for (int i = 0; i < _crosses.Length; i++)
            _crosses[i] = ResourceLoader.Load<PackedScene>("res://Content/Scenes/Crosses/" + (G.CurrentLevel == 10 && G.LevelAdditionalLink == "True" || Meta.Instance.EnhancedCrossesAtAllLevels ? "Enhanced" : "") + "Cross" + (i + 1) + ".tscn");
        Connect("LevelReload", new Callable(GetNode(".."), "LevelLoad"));
        Input.MouseMode = !GetTree().Paused ? Input.MouseModeEnum.Hidden : Input.MouseModeEnum.Visible;
    }

    private bool _crossesEnabledDebug = true;

    public override void _PhysicsProcess(double delta)
    {
        if (Input.IsActionPressed("TeleportDebug"))
            _player.GlobalPosition = GetGlobalMousePosition();

        if (Input.IsActionJustReleased("ScoreDebug"))
            G.Scores += 5;

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



        if (Input.IsActionPressed("Reset"))
        {
            G.ResetTimer += _floatDelta;
            if (G.ResetTimer > 1.5f)
            {
                Reset();
            }
        }
        else G.ResetTimer = G.ResetTimer > 0 ? G.ResetTimer - _floatDelta : 0;

        if (G.IsPlayerDead) return;

        if (!G.IsProgressPaused)
        {
            G.Scores += _floatDelta;
            _weightMultiplierExtenderToCurrentCross += (_floatDelta * G.DefaultCrossWeight[_lastAviableCrossNumber]) / 30;
        }

        if (G.IsCrossesEnabled)
        {
            int RandomRange = (int)G.Scores < 150 ? 20 - (int)G.Scores / 30 - Meta.Instance.Dificulty * 5 : 15 - Meta.Instance.Dificulty * 5;
            RandomRange -= (int)(RandomRange / 2 - G.PlayerMoveCoeff * RandomRange / 2);
            RandomRange = (int)(RandomRange / G.CrossSpawnMultiplier);
            if (_random.Next(RandomRange) == 0)
            {
                if (!_doAllCrossWeigthsSetted)
                {
                    if (_crossWeight[_lastAviableCrossNumber] + _weightMultiplierExtenderToCurrentCross < G.DefaultCrossWeight[_lastAviableCrossNumber])
                    {
                        _crossWeight[_lastAviableCrossNumber] += _weightMultiplierExtenderToCurrentCross;
                        _weightMultiplierExtenderToCurrentCross = 0;
                    }
                    else
                    {
                        _crossWeight[_lastAviableCrossNumber] = G.DefaultCrossWeight[_lastAviableCrossNumber];
                        _lastAviableCrossNumber++;
                        _weightMultiplierExtenderToCurrentCross = 0;
                    }
                    if (_lastAviableCrossNumber >= 5)
                    {
                        _lastAviableCrossNumber = 4;
                        _doAllCrossWeigthsSetted = true;
                    }
                }

                int SelectedCrossNumber;
                int RandomNumber = _random.Next((int)_crossWeight.Sum());
                for (int i = 0; ; i++)
                {
                    if (RandomNumber < G.DefaultCrossWeight[i])
                    {
                        SelectedCrossNumber = i;
                        break;
                    }
                    else RandomNumber -= G.DefaultCrossWeight[i];
                }
                SelectedCrossNumber = 1;

                Node2D Cross = (Node2D)_crosses[SelectedCrossNumber].Instantiate();
                float CrossGathering = _random.Next(100) < (1 - G.PlayerMoveCoeff) * 50 ? 3 - G.PlayerMoveCoeff * 2 : 1;
                Cross.Position = new Vector2(_player.Position.X + (-750 + _random.Next(1500)) / CrossGathering, _player.Position.Y + (-450 + _random.Next(900)) / CrossGathering);
                switch (Cross.Name)
                {
                    case "RestlessCross":
                        float XPos = _random.Next(
                            _player.Position.X - 425 > G.CameraLimits.W ? (int)_player.Position.X - 425 : 0,
                            _player.Position.X + 425 < G.CameraLimits.Y ? (int)_player.Position.X + 425 : (int)_player.Position.X + 425
                            );
                        float YPos = _random.Next(
                            _player.Position.Y - 240 > G.CameraLimits.X ? (int)_player.Position.Y - 240 : 0,
                            _player.Position.Y + 240 < G.CameraLimits.Z ? (int)_player.Position.Y + 240 : (int)_player.Position.Y + 240
                            );
                        Cross.Position = new Vector2(XPos, YPos);
                        break;

                    case "BlumCross":
                        if ((Cross.Position - _player.Position).X < 300)
                            Cross.Position = new Vector2(
                                Cross.Position.X,
                                _random.Next(100) < 50 ?
                                _random.Next((int)_player.Position.Y - 750, (int)_player.Position.Y - 250) :
                                _random.Next((int)_player.Position.Y + 250, (int)_player.Position.Y + 750)
                                );
                        break;

                    case "CannonCross":
                        if (G.CurrentLevel == 8)
                        {
                            Cross.RotationDegrees = _random.Next(100) <= 50 ? 90 : -90;
                            Cross.Position = new Vector2(Cross.Position.X + 1280, Cross.RotationDegrees == 90 ? -25 : 760);
                            break;
                        }

                        Cross.Scale = new Vector2(_random.Next(100) <= 50 ? 1 : -1, 1);
                        Cross.Position = Cross.Scale.X == -1 ? new Vector2(G.LevelXYSizes[G.CurrentLevel].X + 25, Cross.Position.Y) : new Vector2(-25, Cross.Position.Y);
                        break;
                }
                AddChild(Cross);

                if (G.CurrentLevel == 9)
                    Cross.AddToGroup("Crosses");
            }
        }
    }

    public void Reset()
    {
        G.IsProgressPaused = false;
        G.CrossSpawnMultiplier = 1;
        UnchangableMeta.SaveRecords();
        EmitSignal("LevelReload");
        QueueFree();
    }

    public void SetCrossEnabled(bool value)
    {
        G.IsCrossesEnabled = value;
    }
}