using Godot;
using System;

public partial class LevelScript : Node2D
{
    PackedScene[] _defaultCross = new PackedScene[G.CrossesInGameTotal];
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
        _player = GetNode<CharacterBody2D>("Player");
        for (int i = 0; i < _defaultCross.Length; i++)
            _defaultCross[i] = ResourceLoader.Load<PackedScene>("res://Content/Scenes/Crosses/Cross" + (i + 1) + ".tscn");
        Input.MouseMode = !GetTree().Paused ? Input.MouseModeEnum.Hidden : Input.MouseModeEnum.Visible;
    }
    public override void _PhysicsProcess(double delta)
    {
        if (Input.IsActionPressed("Reset"))
        {
            G.ResetTimer += _floatDelta;
            if (G.ResetTimer > 1.5f)
            {
                G.SaveRecords();
                GetTree().ReloadCurrentScene();
            }
        }
        else G.ResetTimer = G.ResetTimer > 0 ? G.ResetTimer - _floatDelta : 0;

        if (G.PlayerDead) return;
        G.Scores += _floatDelta;
        _weightMultiplierExtenderToCurrentCross += (_floatDelta * G._defaultCrossWeight[_lastAviableCrossNumber]) / 30;
        

        int IntScores = (int)G.Scores;
        GetNode<Label>("Player/DeadPlayer/Camera2D/GUI/Scores").Text = IntScores.ToString();
        int RandomRange1 = IntScores < (15 - Meta.Instance.Dificulty * 4) * 15 ? 20 - IntScores / 15 - Meta.Instance.Dificulty * 5 : 5 - Meta.Instance.Dificulty;
        if (_random.Next(RandomRange1) == 0)
        {
            if (!_doAllCrossWeigthsSetted)
            {
                if (_crossWeight[_lastAviableCrossNumber] + _weightMultiplierExtenderToCurrentCross < G._defaultCrossWeight[_lastAviableCrossNumber])
                {
                    _crossWeight[_lastAviableCrossNumber] += _weightMultiplierExtenderToCurrentCross;
                    _weightMultiplierExtenderToCurrentCross = 0;
                }
                else
                {
                    _crossWeight[_lastAviableCrossNumber] = G._defaultCrossWeight[_lastAviableCrossNumber];
                    _lastAviableCrossNumber++;
                    _weightMultiplierExtenderToCurrentCross = 0;
                }
                if (_lastAviableCrossNumber >= 5)
                {
                    _lastAviableCrossNumber = 4;
                    _doAllCrossWeigthsSetted = true;
                }
            }
            float RandomRange2 = 0;
            for (int i = 0; i < _crossWeight.Length && _crossWeight[i] != 0; i++)
                RandomRange2 += _crossWeight[i];

            int SelectedCrossNumber;
            int RandomNumber = _random.Next((int)RandomRange2);
            for (int i = 0; ; i++)
            {
                if (RandomNumber < G._defaultCrossWeight[i])
                {
                    SelectedCrossNumber = i;
                    break;
                }
                else RandomNumber -= G._defaultCrossWeight[i];
            }

            Node2D Cross = (Node2D)_defaultCross[SelectedCrossNumber].Instantiate();
            float CrossGathering = _random.Next(100) < (1 - G.PlayerMoveCoeff) * 50 ? 3 - G.PlayerMoveCoeff * 2 : 1;
            Cross.Position = new Vector2(_player.Position.X + (-750 + _random.Next(1500)) / CrossGathering, _player.Position.Y + (-450 + _random.Next(900)) / CrossGathering);
            switch (Cross.Name)
            {
                case "RestlessCross":
                    float XPos = _random.Next(
                        _player.Position.X - 426 > 0 ? (int)_player.Position.X - 426 : 0,
                        _player.Position.X + 426 < G.LevelXYSizes[G.CurrentLevel].X ? (int)_player.Position.X + 426 : (int)_player.Position.X + 456
                        ) ;
                    float YPos = _random.Next(
                        _player.Position.Y - 239 > 0 ? (int)_player.Position.Y - 239 : 0, 
                        _player.Position.Y + 239 < G.LevelXYSizes[G.CurrentLevel].Y ? (int)_player.Position.Y + 239 : (int)_player.Position.Y + 239
                        );
                    Cross.Position = new Vector2(XPos, YPos);
                    break;

                case "BlumCross":
                    Vector2 CrossPositionRelativeToPlayer = new Vector2(Cross.Position.X - _player.Position.X, Cross.Position.Y - _player.Position.Y);

                    float DistancingToDesiredDistance(float coordinate, float desiredDistance)
                    {
                        if (coordinate < desiredDistance && coordinate >= 0)
                            return desiredDistance;
                        else if (coordinate > -desiredDistance && coordinate < 0)
                            return -desiredDistance;
                        else return coordinate;
                    }
                    Cross.Position = new Vector2(Cross.Position.X + DistancingToDesiredDistance(CrossPositionRelativeToPlayer.X, 200), Cross.Position.Y + DistancingToDesiredDistance(CrossPositionRelativeToPlayer.Y, 100));
                    break;

                case "CannonCross":
                    Cross.Scale = new Vector2(_random.Next(100) <= 50 ? 1 : -1, 1);
                    Cross.GlobalPosition = Cross.Scale.X == -1 ? new Vector2(G.LevelXYSizes[G.CurrentLevel].X + 120, Cross.Position.Y) : new Vector2(-120, Cross.Position.Y);
                    break;
            }
            AddChild(Cross);
        }

    }
}