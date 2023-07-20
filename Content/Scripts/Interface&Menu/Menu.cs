using Godot;
using System;

public partial class Menu : Control
{
    private bool _easterEgged;

    public override void _Ready()
    {
        GetNode<TextureButton>("Foreground/Buttons/ChooseLevel").GrabFocus();
        if (GetTree().Paused)
            GetTree().Paused = false;

        Random random = new Random();
        var PresentedLevel = (Node2D)ResourceLoader.Load<PackedScene>("res://Content/Scenes/Levels/PresentedParts/PresentedLevel" + (random.Next(G.LevelsInGameTotal) + 1) + ".tscn").Instantiate();
        GetNode<SubViewport>("Background/LevelPresenter/SubViewport").AddChild(PresentedLevel);
    }
    public void ChooseLevel()
    {
        GetTree().ChangeSceneToFile("res://Content/Scenes/Interface&Menu/LevelsMenu.tscn");
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
