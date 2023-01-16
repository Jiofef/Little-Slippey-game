using Godot;
using System;

public class LevelScript : Node2D
{
    PackedScene[] _defaultCross = new PackedScene[G.CrossesInGameTotal];
    KinematicBody2D _player;

    Random _random = new Random();

    private readonly int[] _defaultCrossWeight = {100, 40, 20, 15, 30};
    private float[] _crossWeight = new float[G.CrossesInGameTotal];
    private int _lastAviableCrossNumber = 0;
    private float _weightMultiplierExtenderToCurrentCross = 0;

    public override void _Ready()
    {
        G.FitToDefaultValues();
        _player = GetNode<KinematicBody2D>("Player");
        for (int i = 0; i < _defaultCross.Length; i++)
            _defaultCross[i] = ResourceLoader.Load<PackedScene>("res://Content/Scenes/Crosses/Cross" + (i + 1) + ".tscn");
        Input.MouseMode = !GetTree().Paused ? Input.MouseModeEnum.Hidden : Input.MouseModeEnum.Visible;
    }
    public override void _PhysicsProcess(float delta)
    {
        if (Input.IsActionPressed("Reset"))
        {
            G.ResetTimer += delta;
            if (G.ResetTimer > 1.5f)
            {
                G.SaveRecords();
                GetTree().ReloadCurrentScene();
            }
        }
        else G.ResetTimer = G.ResetTimer > 0 ? G.ResetTimer - delta : 0;

        if (G.PlayerDead) return;
        G.Scores += delta;
        _weightMultiplierExtenderToCurrentCross += (delta * _defaultCrossWeight[_lastAviableCrossNumber]) / 30;

        int IntScores = (int)G.Scores;
        GetNode<Label>("Player/DeadPlayer/Camera2D/GUI/Scores").Text = IntScores.ToString();

        int RandomRange1 = IntScores < (15 - Meta.Instance.Dificulty * 4) * 15 ? 20 - IntScores / 15 - Meta.Instance.Dificulty * 5 : 5 - Meta.Instance.Dificulty;
        if (_random.Next(RandomRange1) == 0)
        {
            if (_lastAviableCrossNumber <= G.CrossesInGameTotal)
            {
                if (_crossWeight[_lastAviableCrossNumber] + _weightMultiplierExtenderToCurrentCross < _defaultCrossWeight[_lastAviableCrossNumber])
                {
                    _crossWeight[_lastAviableCrossNumber] += _weightMultiplierExtenderToCurrentCross;
                    _weightMultiplierExtenderToCurrentCross = 0;
                }
                else
                {
                    _crossWeight[_lastAviableCrossNumber] = _defaultCrossWeight[_lastAviableCrossNumber];
                    _lastAviableCrossNumber++;
                    _weightMultiplierExtenderToCurrentCross = 0;
                }
            }
            float RandomRange2 = 0;
            for (int i = 0; i < _crossWeight.Length && _crossWeight[i] != 0; i++)
                RandomRange2 += _crossWeight[i];

            int SelectedCrossNumber;
            int RandomNumber = _random.Next((int)RandomRange2);
            for (int i = 0; ; i++)
            {
                if (RandomNumber < _defaultCrossWeight[i])
                {
                    SelectedCrossNumber = i;
                    break;
                }
                else RandomNumber -= _defaultCrossWeight[i];
            }

            Node2D Cross = (Node2D)_defaultCross[SelectedCrossNumber].Instance();
            float CrossGathering = _random.Next(100) < (1 - G.PlayerMoveCoeff) * 50 ? 3 - G.PlayerMoveCoeff * 2 : 1;
            Cross.Position = new Vector2(_player.Position.x + (-750 + _random.Next(1500)) / CrossGathering, _player.Position.y + (-450 + _random.Next(900)) / CrossGathering);
            if (Cross.Name == "BlumCross")
            {
                Vector2 CrossPositionRelativeToPlayer = new Vector2(Cross.Position.x - _player.Position.x, Cross.Position.y - _player.Position.y);

                float DistancingToDesiredDistance(float coordinate, float desiredDistance)
                {
                    if (coordinate < desiredDistance && coordinate >= 0)
                        return desiredDistance;
                    else if (coordinate > -desiredDistance && coordinate < 0)
                        return -desiredDistance;
                    else return coordinate;
                }
                Cross.Position = new Vector2(Cross.Position.x + DistancingToDesiredDistance(CrossPositionRelativeToPlayer.x, 200), Cross.Position.y + DistancingToDesiredDistance(CrossPositionRelativeToPlayer.y, 100));
            }
            else if (Cross.Name == "CannonCross")
            {
                Cross.Scale = new Vector2(_random.Next(100) <= 50 ? 1 : -1, 1);
                if (Cross.Scale.x == 1)
                    Cross.Position = new Vector2(G.LevelXYSizes[G.CurrentLevel - 1].x, Cross.Position.y);
                else if (Cross.Scale.x == -1)
                    Cross.Position = new Vector2(0, Cross.Position.y);
            }
            AddChild(Cross);
        }
    }
}