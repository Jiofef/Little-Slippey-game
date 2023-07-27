using Godot;
using System;

public partial class Menu : Control
{
    Node2D _foreground;
    Timer _buttonsFocusingTimer;
    ShaderMaterial _blurShaderMaterial;

    private int _crossesSpawnedByClick = 0;
    public override void _Ready()
    {
        _blurShaderMaterial = GetNode<ColorRect>("Background/Blur").Material as ShaderMaterial;
        _foreground = GetNode<Node2D>("Foreground");
        _buttonsFocusingTimer = GetNode<Timer>("Foreground/BottomRect/Buttons/ButtonsFocusingTimer");
        if (GetTree().Paused)
            GetTree().Paused = false;

        Random random = new Random();
        int PresentedLevelNumber = random.Next(G.LevelsInGameTotal) + 1;
        while (PresentedLevelNumber == 7)
            PresentedLevelNumber = random.Next(G.LevelsInGameTotal) + 1;
        var PresentedLevel = (Node2D)ResourceLoader.Load<PackedScene>("res://Content/Scenes/Levels/PresentedParts/PresentedLevel" + PresentedLevelNumber + ".tscn").Instantiate();
        GetNode<SubViewport>("Background/LevelPresenter/SubViewport").AddChild(PresentedLevel);
    }
    public override void _PhysicsProcess(double delta)
    {
        float NewForegroundModulate;
        if(_buttonsFocusingTimer.TimeLeft == 0)
        {
            float MouseRectMiddleXPos = _foreground.GetLocalMousePosition().X - 192;

            if (MouseRectMiddleXPos < 0)
                MouseRectMiddleXPos *= -1;

            NewForegroundModulate = 1 - (MouseRectMiddleXPos - 384) / 500;

            if (NewForegroundModulate < 0.42f)
                NewForegroundModulate = 0.42f;
            else if (NewForegroundModulate > 1)
                NewForegroundModulate = 1;
        }
        else
            NewForegroundModulate = 1;

        _foreground.Modulate = new Color(_foreground.Modulate.R, _foreground.Modulate.G, _foreground.Modulate.B, _foreground.Modulate.A - (_foreground.Modulate.A - NewForegroundModulate) / 10);
        _blurShaderMaterial.SetShaderParameter("lod", 2.5f * (_foreground.Modulate.A - 0.42f) * 1 / (1 - 0.42f));
    }
    public void Play()
    {
        GetTree().ChangeSceneToFile("res://Content/Scenes/Interface&Menu/LevelsMenu.tscn");
    }
    public void Options()
    {
        GetTree().ChangeSceneToFile("res://Content/Scenes/Interface&Menu/OptionsMenu.tscn");
    }
    public void Skins()
    {
        GetTree().ChangeSceneToFile("res://Content/Scenes/Interface&Menu/Titles.tscn");
    }
    public void Achievements()
    {
        GetTree().ChangeSceneToFile("res://Content/Scenes/Interface&Menu/Titles.tscn");
    }
    public void Titles()
    {
        GetTree().ChangeSceneToFile("res://Content/Scenes/Interface&Menu/Titles.tscn");
    }
    public void SpawnCross()
    {
        Node2D Cross = (Node2D)ResourceLoader.Load<PackedScene>("res://Content/Scenes/Crosses/Cross1.tscn").Instantiate();
        Cross.Position = GetGlobalMousePosition();
        AddChild(Cross);
        _crossesSpawnedByClick++;
        if (_crossesSpawnedByClick == 273)
        {
            var explosionAnimation = GetNode<AnimatedSprite2D>("ExplosionAnimation");
            explosionAnimation.Visible = true;
            explosionAnimation.Play();
            GetNode<AudioStreamPlayer>("ExplosionAnimation/ExplosionSound").Play();
        }
    }
}
