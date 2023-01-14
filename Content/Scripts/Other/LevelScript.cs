using Godot;
using System;

public class LevelScript : Node2D
{
    PackedScene[] _defaultCross = new PackedScene[G.CrossesInGameTotal];
    KinematicBody2D _player;
    Label _scores;
    private Random _random = new Random();
    private int _intScores;

    public override void _Ready()
    {
        G.FitToDefaultValues();
        _player = GetNode<KinematicBody2D>("Player");
        _scores = GetNode<Label>("Player/DeadPlayer/Camera2D/GUI/Scores");
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
        _scores.Text = _intScores.ToString();

        int RandomRange = _intScores < (15 - Meta.Instance.Dificulty * 4) * 15 ? 20 - _intScores / 15 - Meta.Instance.Dificulty * 5 : 5 - Meta.Instance.Dificulty;
        if (_random.Next(RandomRange) == 0)
        {
            Node2D Cross = (Node2D)_defaultCross[_random.Next(_defaultCross.Length)].Instance();
            float CrossGathering = _random.Next(100) < (1 - G.PlayerMoveCoeff) * 50 ? 3 - G.PlayerMoveCoeff * 2 : 1;
            Cross.Position = new Vector2(_player.Position.x + (-750 + _random.Next(1500)) / CrossGathering, _player.Position.y + (-450 + _random.Next(900)) / CrossGathering);
            if (Cross.Name == "BlumCross")
            {
                Vector2 CrossPositionRelativeToPlayer = new Vector2(Cross.Position.x - _player.Position.x, Cross.Position.y - _player.Position.y);

                float DistancingToDesiredDistance (float coordinate, float desiredDistance)
                {
                    if (coordinate < desiredDistance && coordinate >= 0)
                        return desiredDistance;
                    else if (coordinate > -desiredDistance && coordinate < 0)
                        return -desiredDistance;
                    else return coordinate;
                }
                Cross.Position = new Vector2(Cross.Position.x + DistancingToDesiredDistance(CrossPositionRelativeToPlayer.x, 200), Cross.Position.y + DistancingToDesiredDistance(CrossPositionRelativeToPlayer.y, 200));
            }
            AddChild(Cross);
        }
    }
}