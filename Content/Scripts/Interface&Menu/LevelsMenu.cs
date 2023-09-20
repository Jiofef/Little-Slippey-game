using Godot;

public partial class LevelsMenu : Control
{
    Node2D _presentedLevel;

    private int _chosenLevel, _currentLevelsRow = 1;
    private string _additionalLevelLink;

    public override void _Ready()
	{
        GetNode<TextureButton>("LevelsList/LevelsIconsContainer/SubContainer/Level1Button").GrabFocus();
        for (int i = 0; i < G.LevelsInGameTotal; i++)
            if (UnchangableMeta.LevelCompleteStatus[i] > 0)
            GetNode<Sprite2D>("LevelsList/LevelsIconsContainer/SubContainer/Level" + (i + 1) + "Button/Sprite2D").RegionRect = new Rect2(new Vector2(45 * i, 45 * (UnchangableMeta.LevelCompleteStatus[i] - 1)), new Vector2(45, 45));
        if (UnchangableMeta.LevelCompleteStatus[4] > 0)
        {
            GetNode<Node2D>("LevelsList/LevelsIconsContainer/SubContainer/Level5Button/WindowView").Visible = true;
            GetNode<AnimationPlayer>("LevelsList/LevelsIconsContainer/SubContainer/Level5Button/WindowView/AnimationPlayer").CurrentAnimation = "LightingAndBlackoutAnimation" + UnchangableMeta.LevelCompleteStatus[4];
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
            var noiseAnimationPlayer = GetNode<AnimationPlayer>("Visual/LevelPresenterOutline/LevelPresenter/WhiteNoise/AnimationPlayer");
            noiseAnimationPlayer.CurrentAnimation = null;
            noiseAnimationPlayer.Play("NoiseDisappearing");
            
            _chosenLevel = value;
            _additionalLevelLink = additionalLinkValue;
            LevelPresenterViewportUpdate();
        }
    }

    public void PlayLevel(int value, string additionalLevelLink = "", bool doOpenLevelPossibleStartOptions = false)
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

        GetNode("Visual/LevelPresenterOutline/LevelPresenter/SubViewport").AddChild(_presentedLevel);

        GetNode<Label>("Visual/SubMenu/ColorRect/HardBestResult").Text = "Hard: " + UnchangableMeta.LevelRecords[0][_chosenLevel - 1];
        GetNode<Label>("Visual/SubMenu/ColorRect/InsaneBestResult").Text = "Insane: " + UnchangableMeta.LevelRecords[1][_chosenLevel - 1];
        GetNode<Label>("Visual/SubMenu/ColorRect/InfernalBestResult").Text = "Infernal: " + UnchangableMeta.LevelRecords[2][_chosenLevel - 1];
    }
}
