using Godot;

public partial class LevelsMenu : Control
{
    Node2D _presentedLevel;

    private readonly string[] _dificultiesNames = { "Hard", "Insane", "Inferno" };
    private int _chosenLevel, _currentLevelsRow = 1;
    private string _additionalLevelLink;

    public override void _Ready()
	{
        GetNode<TextureButton>("LevelsList/LevelsIconsContainer/SubContainer/Level1Button").GrabFocus();

        string[] ButtonNames = { "Rain", "OldFilm", "EnhancedCrosses"};
        int[] NeededLevelIndexes = { 4, 6, 9 };
        for (int i = 0; i < ButtonNames.Length; i++)
        {
            if (UnchangableMeta.LevelCompleteStatus[NeededLevelIndexes[i]] > 0)
            {
                GetNode<ColorRect>("Visual/AdittionalButtons/Lock" + (i + 1)).QueueFree();
                var toggleButton = GetNode<CheckBox>("Visual/AdittionalButtons/Toggle" + ButtonNames[i] + "Button");
                toggleButton.Disabled = false;
                toggleButton.ButtonPressed = Meta.Instance.AdditionStatuses[i];
            }
        }

        for (int i = 0; i < _dificultiesNames.Length; i++)
            GetNode<CheckBox>("Visual/DifficultyButtons/" + _dificultiesNames[i] + "ModeButton").ButtonPressed = Meta.Instance.Dificulty == i;

        UpdateGUIForCurrentDificulty();

        for (int i = 0; i < G.LevelsInGameTotal; i++)
            if (UnchangableMeta.LevelCompleteStatus[i] > 0)
            GetNode<Sprite2D>("LevelsList/LevelsIconsContainer/SubContainer/Level" + (i + 1) + "Button/Sprite2D").RegionRect = new Rect2(new Vector2(45 * i, 45 * (UnchangableMeta.LevelCompleteStatus[i] - 1)), new Vector2(45, 45));
        if (UnchangableMeta.LevelCompleteStatus[4] > 0)
        {
            string link = "LevelsList/LevelsIconsContainer/SubContainer/Level5Button/WindowView";
            GetNode<Node2D>(link).Visible = true;
            GetNode<AnimationPlayer>(link + "/AnimationPlayer").CurrentAnimation = "LightingAndBlackoutAnimation" + UnchangableMeta.LevelCompleteStatus[4];
            if (UnchangableMeta.LevelCompleteStatus[4] >= 2)
            {
                GetNode<CpuParticles2D>(link + "/Rain").Color = new Color(0.79f, 0, 0);
                GetNode<CpuParticles2D>(link + "/Window/Rain").Color = new Color(0.79f, 0, 0);
                GetNode<CpuParticles2D>(link + "/Fog").Color = new Color(1f, 0, 0);
                GetNode<ColorRect>(link + "/Fog?").Color = new Color(0.7f, 0, 0);
                GetNode<ColorRect>(link + "/Fog?Interior").Color = new Color(0.08f, 0.04f, 0.04f);
            }
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (Input.IsActionJustPressed("Cancel"))
            Cancel();
    }

    public void Cancel()
    {
        GetTree().ChangeSceneToFile("res://Content/Scenes/Interface&Menu/Menu.tscn");
    }

    public void SetPresentedLevel(int value, string additionalLinkValue = "")
    {
        if(_chosenLevel != value || additionalLinkValue != _additionalLevelLink)
        {
            var noiseAnimationPlayer = GetNode<AnimationPlayer>("Visual/LevelPresenter/WhiteNoise/AnimationPlayer");
            noiseAnimationPlayer.CurrentAnimation = null;
            noiseAnimationPlayer.Play("NoiseDisappearing");
            
            _chosenLevel = value;
            _additionalLevelLink = additionalLinkValue;
            LevelPresenterViewportUpdate();
        }
    }

    public void TryToOpenLevel(int value, string additionalLevelLink = "", bool doOpenLevelPossibleStartOptions = false)
    {
        void OpenStartOptions(string link)
        {
            GetNode<Control>(link + "/StartOption").Visible = true;
            GetNode<TextureButton>(link).Disabled = true;
            GetNode<TextureButton>(link + "/StartOption/Yes").Disabled = false;
            GetNode<TextureButton>(link + "/StartOption/No").Disabled = false;
        }

        if (value == 9 && UnchangableMeta.IsLevel9PlatformSectionSkipAllowed && doOpenLevelPossibleStartOptions)
        {
            OpenStartOptions("LevelsList/LevelsIconsContainer/SubContainer/Level9Button");
            return;
        }
        else if (value == 10 && UnchangableMeta.IsFakeLevel10SkipAllowed && doOpenLevelPossibleStartOptions)
        {
            OpenStartOptions("LevelsList/LevelsIconsContainer/SubContainer/Level10Button");
            return;
        }

        G.LevelAdditionalLink = additionalLevelLink;
        G.CurrentLevel = value;
        GetNode<Node2D>("Visual").QueueFree();
        GetNode<TextureButton>("CancelButton").QueueFree();
        GetNode<Control>("LevelsList").QueueFree();
        var openLevelNoise = GetNode<AnimatedSprite2D>("OpenLevelNoise");
        openLevelNoise.Visible = true;
        openLevelNoise.Play();
        GetNode<AudioStreamPlayer>("OpenLevelNoise/AudioStreamPlayer").Play();
    }
    private void OpenLevel()
    {
        GetTree().ChangeSceneToFile("res://Content/Scenes/Other/Main.tscn");
    }

    public void ChangeLevelRowToPast()
    {
        _currentLevelsRow--;

        var animationPlayer = GetNode<AnimationPlayer>("LevelsList/LevelsIconsContainer/SubContainer/AnimationPlayer");
        animationPlayer.CurrentAnimation = "Scroll " + (_currentLevelsRow + 1) + "-" + _currentLevelsRow;
        animationPlayer.Play();

        var leftScrollButton = GetNode<TextureButton>("LevelsList/LeftScrollButton");
        if (_currentLevelsRow <= 1)
            leftScrollButton.Disabled = true;
        else
            leftScrollButton.Disabled = false;
        GetNode<TextureButton>("LevelsList/RightScrollButton").Disabled = false;
    }

    public void ChangeLevelRowToNext()
    {
        _currentLevelsRow++;

        var animationPlayer = GetNode<AnimationPlayer>("LevelsList/LevelsIconsContainer/SubContainer/AnimationPlayer");
        animationPlayer.CurrentAnimation = "Scroll " + (_currentLevelsRow - 1) + "-" + _currentLevelsRow;
        animationPlayer.Play();

        var rightScrollButton = GetNode<TextureButton>("LevelsList/RightScrollButton");
        if (_currentLevelsRow >= 2)
            rightScrollButton.Disabled = true;
        else
            rightScrollButton.Disabled = false;
        GetNode<TextureButton>("LevelsList/LeftScrollButton").Disabled = false;
    }

    private void LevelPresenterViewportUpdate()
    {
        if (_presentedLevel != null)
            _presentedLevel.QueueFree();

        _presentedLevel = (Node2D)ResourceLoader.Load<PackedScene>("res://Content/Scenes/Levels/PresentedParts/PresentedLevel" + _chosenLevel + _additionalLevelLink + ".tscn").Instantiate();

        GetNode("Visual/LevelPresenter/SubViewport").AddChild(_presentedLevel);
        for (int i = 0; i < _dificultiesNames.Length; i++)
            GetNode<Label>("Visual/" + _dificultiesNames[i] + "BestResult").Text = UnchangableMeta.LevelRecords[i][_chosenLevel - 1].ToString();
    }

    public void SetDifficulty(int value)
    {
        Meta.Instance.Dificulty = value;
        UpdateGUIForCurrentDificulty();
        Meta.Instance.SaveToFile();
    }
    public void SetAdditionStatus(bool value, int AdditionIndex)
    {
        Meta.Instance.AdditionStatuses[AdditionIndex] = value;
        Meta.Instance.SaveToFile();
    }
    private void UpdateGUIForCurrentDificulty()
    {
        for (int i = 0; i < _dificultiesNames.Length; i ++)
            GetNode<Label>("Visual/" + _dificultiesNames[i] + "BestResult").Visible = Meta.Instance.Dificulty == i;
    }
}
