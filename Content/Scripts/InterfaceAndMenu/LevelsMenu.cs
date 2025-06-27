using Godot;
using static OtherExtension.FastInstanceCreator;

public partial class LevelsMenu : DraggableWindow
{
	[Export] private int _levelRowsAmount = 1;
	[Export] private LevelButton[] LevelButtons = new LevelButton[0];
	public enum LevelsMenuTab { Levels, BonusLevels, Difficulties, Additions }
    Node2D _presentedLevel;
	Control _selectedTab;

	WelcomeToGOS _welcomeToGOS;

    private readonly string[] _difficultiesNames = { "Hard", "Insane", "Inferno" };
    private string _additionalLevelLink;

	private const string LEVEL_BUTTONS_LINK = "MarginContainer/VBoxContainer/HBC/Levels/LevelsList/C/HBC/";

    public override void _Ready()
	{
		// Initializing nodes
		_welcomeToGOS = GetParentOrNull<WelcomeToGOS>();
		_selectedTab = GetNode<Control>("MarginContainer/VBoxContainer/HBC/Levels");

		InitLevelsTab();
        // string[] ButtonNames = { "Rain", "OldFilm", "JiofefHead", "EnhancedCrosses"};
        // int[] NeededLevelIndexes = { 4, 6, 8, 9 };
        // for (int i = 0; i < ButtonNames.Length; i++)
        // {
        //     if (UnchangableMeta.LevelCompleteStatus[NeededLevelIndexes[i]] > 0)
        //     {
        //         GetNode<Sprite2D>("Visual/AdittionalButtons/Lock" + (i + 1)).QueueFree();
        //         GetNode<Label>("Visual/AdittionalButtons/" + ButtonNames[i]).Visible = true;
        //         var toggleButton = GetNode<CheckBox>("Visual/AdittionalButtons/Toggle" + ButtonNames[i] + "Button");
        //         toggleButton.Disabled = false;
        //         toggleButton.ButtonPressed = Meta.Instance.Gameplay.AdditionStatuses[i];
        //         toggleButton.FocusMode = FocusModeEnum.All;
        //     }   
        // }

        // for (int i = 0; i < _difficultiesNames.Length; i++)
        //     GetNode<CheckBox>("Visual/DifficultyButtons/" + _difficultiesNames[i] + "ModeButton").ButtonPressed = Meta.Instance.Gameplay.Dificulty == i;

        // UpdateGUIForCurrentDificulty();


    }
	public void SelectTab(LevelsMenuTab tab)
	{
		_selectedTab.Hide();
		_selectedTab.ProcessMode = ProcessModeEnum.Disabled;

		_selectedTab = GetNode<Control>("MarginContainer/VBoxContainer/HBC/" + tab.ToString());
		_selectedTab.Show();
		_selectedTab.ProcessMode = ProcessModeEnum.Inherit;

		switch (tab)
		{
			case LevelsMenuTab.Levels:
				UpdateLevelsTab();
				break;
			case LevelsMenuTab.BonusLevels:
				UpdateBonusLevelsTab();
				break;
			case LevelsMenuTab.Difficulties:
				UpdateDifficultiesTab();
				break;
			case LevelsMenuTab.Additions:
				UpdateAdditionsTab();
				break;
		}
	}

	#region Levels tab

	public void UpdateLevelsTab()
	{
		UpdateRecordsDifficultyLabel();
	}
	private string GetRecordsDifficultyText()
	{
		int[] records = UnchangableMeta.GetLevelRecords(_chosenLevel - 1);
		string text = $"{Tr(
			"Records")}\n\n{Tr(
			"Hard")}:\n{records[0]}\n\n{Tr(
			"Insane")}:\n{records[1]}\n\n{Tr(
			"Inferno")}:\n{records[2]}\n\n {Tr(
			"Selected\ndifficulty")}:\n{Tr(
			Meta.Instance.Gameplay.GetDifficultyName()
			)}";
		return text;
	}
	private void InitLevelsTab()
	{
		int levelId;
		void BindButtonSignals(LevelButton button)
		{
			levelId = button.GetLevelId();

			button.Disabled = false;

			button.LevelButtonFocusEntered += SetPresentedLevel;

			button.LevelButtonPressed += StartLevelOpening;
		}

		foreach (var levelButton in LevelButtons)
		{
			if (UnchangableMeta.LevelCompleteStatus[levelButton.GetLevelId() - 1] > 0)
			{
				BindButtonSignals(levelButton);
				int imageId = UnchangableMeta.LevelCompleteStatus[levelId - 1] - 1;
				levelButton.SetImage(imageId);

				// If level has variations for startup
				if (levelButton.OptionButtons != null)
					foreach (var optionLevelButton in levelButton.OptionButtons)
						BindButtonSignals(optionLevelButton);
			}
			else
				break;
		}

		SetPresentedLevel(1, "");
	}
	private int _chosenLevel;
	public void SetPresentedLevel(int value, string additionalLinkValue = "")
    {
        if (_chosenLevel != value || additionalLinkValue != _additionalLevelLink)
        {
			_chosenLevel = value;
            _additionalLevelLink = additionalLinkValue;

            var noiseAnimationPlayer = ContentContainer.GetNode<AnimationPlayer>("HBC/Levels/HBC/C/MC/LevelPresenter/WhiteNoise/AnimationPlayer");
            noiseAnimationPlayer.CurrentAnimation = null;
            noiseAnimationPlayer.Play(UnchangableMeta.LevelPlayedStatus[value - 1] == 1 ? "NoiseDisappearing" : "Noise");

			UpdateRecordsDifficultyLabel();

            LevelPresenterViewportUpdate();
        }
    }
	public void UpdateRecordsDifficultyLabel()
	{
		var recordsDifficultyLabel = GetNode<Label>("MarginContainer/VBoxContainer/HBC/Levels/HBC/RecordsDifficulty");
		recordsDifficultyLabel.Text = GetRecordsDifficultyText();
	}
	public void StartLevelOpening(int value, string additionalLevelLink = "")
	{
		G.LevelAdditionalLink = additionalLevelLink;
		G.CurrentLevel = value;

		_welcomeToGOS.OpenCameraEffect();
		_welcomeToGOS.Connect(nameof(WelcomeToGOS.CameraOpenFinished), new Callable(this, "OpenLevel"));
	}
    public void OpenLevel()
    {
        G.ResetValues();
        GetTree().ChangeSceneToFile("res://Content/Scenes/Levels/FullParts/Level" + G.CurrentLevel + G.LevelAdditionalLink + ".tscn");
    }
	[Export] private NodePath LevelRowHBC;
	const float LEVEL_ROW_LENGTH = 200, LEVEL_ROW_OFFSET = 0;
	private int _currentLevelsRow = 0;
	private void SetLevelRow(int value)
	{
		_currentLevelsRow = value;

		var row = GetNode<Control>(LevelRowHBC);

		Vector2 neededRawPosition = new Vector2(LEVEL_ROW_OFFSET - LEVEL_ROW_LENGTH * _currentLevelsRow, row.Position.Y);
		var tween = row.CreateTween().TweenProperty(row, "position", neededRawPosition, 0.75);
		tween.SetEase(Tween.EaseType.Out);
		tween.SetTrans(Tween.TransitionType.Spring);

		// Disabling buttons that lead to a non-existing row
		var leftScrollButton = GetNode<TextureButton>("MarginContainer/VBoxContainer/HBC/Levels/LevelsList/LeftScrollButton");
		leftScrollButton.Disabled = _currentLevelsRow <= 0;

		var rightScrollButton = GetNode<TextureButton>("MarginContainer/VBoxContainer/HBC/Levels/LevelsList/RightScrollButton");
		rightScrollButton.Disabled = _currentLevelsRow >= _levelRowsAmount - 1;
	}
    public void ChangeLevelRowToPast()
	{
		SetLevelRow(_currentLevelsRow - 1);
	}

    public void ChangeLevelRowToNext()
    {
		SetLevelRow(_currentLevelsRow + 1);
    }

    private void LevelPresenterViewportUpdate()
    {
        if (_presentedLevel != null)
            _presentedLevel.QueueFree();
        _presentedLevel = (Node2D)ResourceLoader.Load<PackedScene>("res://Content/Scenes/Levels/PresentedParts/PresentedLevel" + _chosenLevel + _additionalLevelLink + ".tscn").Instantiate();

        GetNode("MarginContainer/VBoxContainer/HBC/Levels/HBC/C/MC/LevelPresenter/SubViewport").AddChild(_presentedLevel);
        //for (int i = 0; i < _difficultiesNames.Length; i++)
            // ! GetNode<Label>("Visual/" + _difficultiesNames[i] + "BestResult").Text = UnchangableMeta.LevelRecords[i][_chosenLevel - 1].ToString();
    }

	private bool _hasThereBeenLevel10Confirmation = false;
    private void Level10Confirmation()
    {
        _hasThereBeenLevel10Confirmation = true;

        var confirmationWindow = LoadResScene<ConfirmationWindow>("Interface&Menu/RareScenes/Level10StandardSkinWarning.tscn");
        confirmationWindow.Accepted += Skins.SetDefaultSkin;
        confirmationWindow.ZIndex = 1;

        AddChild(confirmationWindow);
    }
	#endregion

	#region Bonus Levels tab
	public void UpdateBonusLevelsTab()
	{

	}
	#endregion

	#region Difficulties tab
	public void UpdateDifficultiesTab()
	{
		string difficultyName = Meta.Instance.Gameplay.GetDifficultyName();
		var button = GetNode<EnhancedButton>($"MarginContainer/VBoxContainer/HBC/Difficulties/{difficultyName}/ToggleButton");
		button.SetPressedNoSignal(true);
	}
	public void SetDifficulty(int value)
    {
        Meta.Instance.Gameplay.Difficulty = value;
        Meta.Instance.SaveToFile();
    }
	#endregion

	#region Additions tab
	public void UpdateAdditionsTab()
	{

	}
	public void SetAdditionEnabled(bool value, int AdditionIndex)
    {
        Meta.Instance.Gameplay.AdditionStatuses[AdditionIndex] = value;
        Meta.Instance.SaveToFile();
    }

	public void BuyAddition()
	{

	}
	#endregion
}
