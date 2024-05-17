using Godot;
using System;
using System.Linq;

public partial class Level10ScientistScript : Node2D
{
    AudioStreamPlayer2D _gramophone;
    CharacterBody2D _player;
    public override void _Ready()
    {
        _gramophone = GetNode<AudioStreamPlayer2D>("Gramophone");
        _player = GetNode<CharacterBody2D>("../Player");
        if ((bool)G.TransitiveVariant[0] == true)
            G.TransitiveVariant[0] = "";
        else
            GetNode<ColorRect>("../CanvasLayer/ColorRect").QueueFree();
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
    }
}
