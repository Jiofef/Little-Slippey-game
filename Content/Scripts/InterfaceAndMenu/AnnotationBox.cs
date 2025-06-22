using Godot;
using System;

public partial class AnnotationBox : Control
{
	// Parent node
	private Control _screenControl;

    [Export] float _sizeCoeff = 0;
    Vector2 _requiredSize;

    public override void _Ready()
    {
        base._Ready();
		_screenControl = GetParent<Control>();
        Hidden += OnHidden;
    }
	private void OnHidden()
	{
		SetProcess(false);
		UnPin();
	}
	public override void _Process(double delta)
    {
        Size = _requiredSize * _sizeCoeff;
        Position = IsPinned ? PinPosition : _screenControl.GetLocalMousePosition();
		// To keep the annotation from going off the screen
        Position = new Vector2(Math.Min(Position.X, 1280 - Size.X), Math.Min(Position.Y, 720 - Size.Y));
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

	#region Pinning
	public Vector2 PinPosition {get; private set;}
	public bool IsPinned {get; private set;} = false;
	public void PinTo(Vector2 position)
	{
		PinPosition = position;
		IsPinned = true;
	}
	public void UnPin()
	{
		PinPosition = Vector2.Zero;
		IsPinned = false;
	}
	#endregion
}
