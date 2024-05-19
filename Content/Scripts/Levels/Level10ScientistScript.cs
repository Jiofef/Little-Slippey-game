using Godot;
using System;
using System.Linq;

public partial class Level10ScientistScript : Node2D
{
    [Signal] public delegate void SetScoresEventHandler();
    [Signal] public delegate void SetResetDisabledEventHandler();
    AudioStreamPlayer2D _gramophone;
    CharacterBody2D _player;
    public override void _Ready()
    {
        Connect("SetResetDisabled", new Callable(GetNode("../../"), "SetResetDisabled"));
        _gramophone = GetNode<AudioStreamPlayer2D>("Gramophone");
        _player = GetNode<CharacterBody2D>("../Player");
        if ((bool)G.TransitiveVariant[0] == true)
            G.TransitiveVariant[0] = "";
        else
            GetNode<ColorRect>("../CanvasLayer/ColorRect").QueueFree();

        if (true)
        {
            Connect("SetScores", new Callable(GetNode("../.."), "SetScores"));
            CallDeferred("emit_signal", "SetScores", G.TransitiveVariant[4]);
        }
    }
    public override void _PhysicsProcess(double delta)
    {
        if ((int)G.TransitiveVariant[1] >= 5)
        {
            var AllCrossesOnScreen = GetTree().GetNodesInGroup("Crosses");
            if (AllCrossesOnScreen.Count() > 0)
            {
                var FirstCross = (Node2D)AllCrossesOnScreen.First();
                FirstCross.Position -= FirstCross.GlobalPosition.DirectionTo(_player.GlobalPosition) * 5;
                var LastCross = (Node2D)AllCrossesOnScreen.Last();
                LastCross.Position -= LastCross.GlobalPosition.DirectionTo(_player.GlobalPosition) * 5;
            }
        }
    }
    public void PlayerDied()
    {
        G.TransitiveVariant[1] = (int)G.TransitiveVariant[1] + 1;
        if (G.Scores > 150 || (bool)G.TransitiveVariant[3])
        {
            EmitSignal("SetResetDisabled", true);
            G.TransitiveVariant[3] = true;
            G.TransitiveVariant[4] = G.Scores;
            GetTree().ReloadCurrentScene();
        }
        if (G.Scores > 300)
        {
            GetNode<Node2D>("../Player/Camera2D/GUI/EmergingElements").Visible = false;
            GetNode<Node2D>("../../").SetPhysicsProcess(false);
        }
    }
}
