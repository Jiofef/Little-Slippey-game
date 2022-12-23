using Godot;
using System;
using System.IO;

public class Menu : Control
{
    private bool _easteregged;
    public override void _Ready()
    {
        GetNode<TextureButton>("Buttons/Play").GrabFocus();
        if (GetTree().Paused)
            GetTree().Paused = false;
    }
    public override void _Process(float delta)
    {
        if (Input.IsActionJustPressed("Cancel"))
            Quit();
    }
    public void Play()
    {
        Random random = new Random();
        G._currentlvl = random.Next(G._levelstotal) + 1;
        GetTree().ChangeScene("res://Content/Scenes/Other/Main.tscn");
    }
    public void ChooseLevel()
    {

    }
    public void Options()
    {
        GetTree().ChangeScene("res://Content/Scenes/Interface&Menu/OptionsMenu.tscn");
    }
    public void Titles()
    {
        GetTree().ChangeScene("res://Content/Scenes/Interface&Menu/Titles.tscn");
    }
    public void Quit()
    {
        GetTree().Quit();
    }
    public void EasterButtonPressed()
    {
        if (!_easteregged)
        {
            _easteregged = true;
            GetNode<Node2D>("CubicCross").Visible = true;
        }
    }
}
