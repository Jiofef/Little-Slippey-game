using Godot;
using System;

public partial class LevelButton : EnhancedButton
{
	[Signal] public delegate void LevelButtonPressedEventHandler(int levelId, string additionalLevelLink);
	[Signal] public delegate void LevelButtonFocusEnteredEventHandler(int levelId, string additionalLevelLink);

	// Properties
	[Export] private int _levelId = 1;
	public int GetLevelId() => _levelId;
	[Export] private string _additionalLevelLink = "";

	[ExportGroup("Start Options")]
	[Export] private Control _startOption = null;
	public bool HasStartOption() => _startOption != null;
	[Export] public bool StartOptionDisabled = false;
	[Export] public LevelButton[] OptionButtons { get; private set; } = null;

	public override void _Ready()
	{
		base._Ready();
		FocusEntered += () => EmitSignal(nameof(LevelButtonFocusEntered), _levelId, _additionalLevelLink);

		if (HasStartOption())
		{
			var closeOption = _startOption.GetNode<EnhancedButton>("CloseOption");
			closeOption.Pressed += () => ToggleStartOptions(false);
		}
	}

	public override void _Pressed()
	{
		base._Pressed();

		if (!HasStartOption() && !StartOptionDisabled)
			EmitSignal(nameof(LevelButtonPressed), _levelId, _additionalLevelLink);
		else
			ToggleStartOptions(true);
	}

	private bool _isStartOptionsOn = false;
	public void ToggleStartOptions(bool value)
	{
		_isStartOptionsOn = value;
		GetNode<Control>("StartOption").Visible = value;
		Disabled = value;
		FocusMode = value ? FocusModeEnum.None : FocusModeEnum.All;
		GetNode<TextureButton>("StartOption/CloseOption").Disabled = !value;
		GetNode<TextureButton>("StartOption/Yes").Disabled = !value;
		GetNode<TextureButton>("StartOption/No").Disabled = !value;

		if (value)
			GetNode<TextureButton>("StartOption/No").GrabFocus();
		else
			GrabFocus();
	}
	public void SetImage(int imageId, float imageSizeX = 45, float imageSizeY = 45)
	{
		GetNode<Sprite2D>("Sprite2D").RegionRect = new Rect2(new Vector2(imageSizeX * (_levelId - 1), imageSizeY * imageId), new Vector2(imageSizeX, imageSizeY));
	}
}
