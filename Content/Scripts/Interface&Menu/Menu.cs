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

        Random random = new Random();
        int RandomLevel = random.Next(G.LevelsInGameTotal) + 1;

        GetNode<Label>("RightBackground/CameraNumber").Text = "Cam " + RandomLevel.ToString();

        var PresentedLevel = (Node2D)ResourceLoader.Load<PackedScene>("res://Content/Scenes/Levels/PresentedParts/PresentedLevel" + (RandomLevel) + ".tscn").Instantiate();
        GetNode<Node2D>("RightBackground/LevelPresenter").AddChild(PresentedLevel);
    }
    public void Play()
    {
        Random random = new Random();
        G.CurrentLevel = UnchangableMeta.DoFirstTimePlayed ? random.Next(G.LevelsInGameTotal) + 1 : 1;
        GetTree().ChangeSceneToFile("res://Content/Scenes/Other/Main.tscn");
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
