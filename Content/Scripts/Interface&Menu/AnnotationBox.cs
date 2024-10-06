using Godot;
using System;

public partial class AnnotationBox : Control
{
    [Export] float _sizeCoeff = 0;
    Vector2 _requiredSize;

    public override void _Ready()
    {
        base._Ready();
        Hidden += () => SetProcess(false);
    }
    public void PopupWithText(string text)
    {
        Show();
        SetProcess(true);



        _sizeCoeff = 0;
        var richTextLabel = GetNode<RichTextLabel>("RichTextLabel");
        richTextLabel.Text = text;
        richTextLabel.UpdateMinimumSize();
        var animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        animationPlayer.Stop();
        animationPlayer.Play("Appearing");
    }

    public void UpdateRequiredSize()
    {
        _requiredSize = GetNode<RichTextLabel>("RichTextLabel").Size + new Vector2(8, 12);
    }
    public override void _Process(double delta)
    {
        Size = _requiredSize * _sizeCoeff;
        Position = GetNode<Control>("../NodeModeGUI").GetLocalMousePosition();
        Position = new Vector2(Math.Min(Position.X, 1280 - Size.X), Math.Min(Position.Y, 720 - Size.Y));
    }
}
