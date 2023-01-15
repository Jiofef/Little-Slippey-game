using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

public class LevelScript : Node2D
{
    PackedScene[] _defaultCross = new PackedScene[G.CrossesInGameTotal];
    KinematicBody2D _player;
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

        if (!G.PlayerDead)
            G.Scores += delta;
        else return;

        int IntScores = (int)G.Scores;
        var scores = GetNode<Label>("Player/DeadPlayer/Camera2D/GUI/Scores");
        scores.Text = IntScores.ToString();
        Random random = new Random();
        int RandomRange = IntScores < (15 - Meta.Instance.Dificulty * 4) * 15 ? 20 - IntScores / 15 - Meta.Instance.Dificulty * 5 : 5 - Meta.Instance.Dificulty;
        if (random.Next(RandomRange) == 0)
        {
            Node2D Cross = (Node2D)_defaultCross[random.Next(_defaultCross.Length)].Instance();
            Cross = (Node2D)_defaultCross[3].Instance();
            float CrossGathering = random.Next(100) < (1 - G.PlayerMoveCoeff) * 50 ? 3 - G.PlayerMoveCoeff * 2 : 1;
            Cross.Position = new Vector2(_player.Position.x + (-750 + random.Next(1500)) / CrossGathering, _player.Position.y + (-450 + random.Next(900)) / CrossGathering);
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
            AddChild(Cross);
        }
    }
}