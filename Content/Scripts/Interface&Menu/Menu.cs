using Godot;
using System;

public partial class Menu : Control
{
    private bool _easterEgged;
    public override void _Ready()
    {
        GetNode<TextureButton>("Buttons/Play").GrabFocus();
        if (GetTree().Paused)
            GetTree().Paused = false;
    }
    public void Play()
    {
        Random random = new Random();
        G.CurrentLevel = random.Next(G.LevelsInGameTotal) + 1;
        GetTree().ChangeSceneToFile("res://Content/Scenes/Other/Main.tscn");
    }
    public void ChooseLevel()
    {

    }
    public void Options()
    {
        GetTree().ChangeSceneToFile("res://Content/Scenes/Interface&Menu/OptionsMenu.tscn");
    }
    public void Titles()
    {
        GetTree().ChangeSceneToFile("res://Content/Scenes/Interface&Menu/Titles.tscn");
    }
    public void EasterButtonPressed()
    {
        if (!_easterEgged)
        {
            _easterEgged = true;
            GetNode<Node2D>("CubicCross").Visible = true;
        }
    }
}
