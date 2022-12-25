using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

public class LevelScript : Node2D
{
    PackedScene[] defaultCross = new PackedScene[G._crossestotal];
    KinematicBody2D player;
    public override void _Ready()
    {
        player = GetNode<KinematicBody2D>("Player");
        for (int i = 0; i < defaultCross.Length; i++)
            defaultCross[i] = ResourceLoader.Load<PackedScene>("res://Content/Scenes/Crosses/Cross" + (i + 1) + ".tscn");
        Input.MouseMode = !GetTree().Paused ? Input.MouseModeEnum.Hidden : Input.MouseModeEnum.Visible;
    }
    public override void _PhysicsProcess(float delta)
    {
        if (Input.IsActionPressed("Reset"))
        {
            G._resettimer += delta;
            if (G._resettimer > 1.5f)
                ResetLevel();
        }
        else G._resettimer = G._resettimer > 0 ? G._resettimer - delta : 0;

        if (!G._playerdead)
            G._scores += delta;
        else return;

        int IntScores = (int)G._scores;
        var scores = GetNode<Label>("Player/DeadPlayer/Camera2D/GUI/Scores");
        scores.Text = IntScores.ToString();
        Random random = new Random();
        int RandomRange = IntScores < (15 - Meta.Instance._dificulty * 4) * 15 ? 20 - IntScores / 15 - Meta.Instance._dificulty * 5 : 5 - Meta.Instance._dificulty;
        if (random.Next(RandomRange) == 0)
        {
            Node2D Cross = (Node2D)defaultCross[random.Next(defaultCross.Length - 1)].Instance();
            float CrossGathering = random.Next(100) < (1 - G._movecoeffplayer) * 50 ? 3 - G._movecoeffplayer * 2 : 1;
            Cross.Position = new Vector2(player.Position.x + (-750 + random.Next(1500)) / CrossGathering, player.Position.y + (-450 + random.Next(900)) / CrossGathering);
            AddChild(Cross);
        }
    }
    private void ResetLevel()
    {
        G.ResetData();
        GetTree().ReloadCurrentScene();
    }
}