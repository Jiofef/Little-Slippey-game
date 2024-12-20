using Godot;
using System;

public partial class ButtonWithText : EnhancedButton
{
    [Export] float _sizeCoeff = 0;
	private string _description;
	[Export] public string Description { get => _description; set => CallDeferred("SetDescription", value); }
	[Export] public bool DisableProcessDisabling = false;


    Vector2 _maximumSize, _sizeDifference, _defaultMinimumSize;
	RichTextLabel _descriptionLabel;
	AnimationPlayer _animaiton;
	Node _parent;

    public override void _Ready()
	{
		base._Ready();

		MouseEntered += () => ShowDescription();
		MouseExited += () => HideDescription();


        _descriptionLabel = GetNode<RichTextLabel>("Face/Description");
		_animaiton = GetNode<AnimationPlayer>("AnimationPlayer");
		_parent = GetParent();

        _defaultMinimumSize = CustomMinimumSize;
        _maximumSize.X = _defaultMinimumSize.X + _descriptionLabel.CustomMinimumSize.X;
        _sizeDifference = _maximumSize - _defaultMinimumSize;

        GetNode<TextureRect>("Face/TextureRect").Texture = TextureNormal;
        TextureNormal = null;


        TrySetProcess(false); // To avoid constantly processing the size
    }

	public void SetDescription(string value)
	{
        _descriptionLabel.Text = value; // Setting description
        _maximumSize.X = CustomMinimumSize.X + _descriptionLabel.CustomMinimumSize.X;
        _sizeDifference = _maximumSize - _defaultMinimumSize;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        CustomMinimumSize = new Vector2(_defaultMinimumSize.X + _sizeDifference.X * _sizeCoeff, _defaultMinimumSize.Y);
    }

    public void ShowDescription()
	{
        _animaiton.Play("Appearing");

        TrySetProcess(true);
	}

	public void HideDescription()
	{
		_animaiton.Play("Disappearing");
		TrySetProcess(true);
    }

	private void TrySetProcess(bool value)
	{
        if (!DisableProcessDisabling)
            SetProcess(value);
    }
}
