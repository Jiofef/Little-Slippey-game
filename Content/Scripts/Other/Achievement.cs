using Godot;
using System.Linq;
using System.IO;
using System;

[Tool]
public partial class Achievement : Control
{
    public bool IsPopupVersion = false;
    [ExportGroup("Icon")]
    [Export] public TextureRect IconNode;
    private AtlasTexture _icon;
    [Export] public AtlasTexture Icon 
    {
        get => _icon;
        set {
            _icon = value;
            if (IconNode != null)
                IconNode.Texture = value;
        }
    }
    private bool _hideIcon = false;
    [Export] public bool HideIcon 
    {
        get => _hideIcon;
        set
        {
            _hideIcon = value;
            UpdateIconVisible();
        }
    }
    private Vector2 _positionInAtlas = new Vector2(0, 0);
    [Export] public Vector2 PositionInAtlas 
    {
        get => _positionInAtlas;
        set {  
            _positionInAtlas = value;
            CallDeferred("UpdateIconRegion");
        }
    }
    private Vector2 _atlasStep = new Vector2(45, 45);
    [Export] public Vector2 AtlasStep 
    {
        get => _atlasStep;
        set {
            _atlasStep = value;
            CallDeferred("UpdateIconRegion");
        }
    }


    public const int MAX_STARS = 3;
    [ExportGroup("Stars")]
    private int _starsCount = 1;
    [Export(PropertyHint.Range, "0,3")] public int StarsCount
    {
        get => _starsCount;
        set { 
            _starsCount = value;
            CallDeferred("UpdateStarsCount");
        }
    }
    private Texture2D _starTexture;
    [Export]
    public Texture2D StarTexture
    {
        get => _starTexture;
        set
        {
            _starTexture = value;
            CallDeferred("UpdateStarsTexture");
        }
    }
    private Color _starsColor = new Color(1, 1, 1);
    [Export]
    public Color StarsColor
    {
        get => _starsColor;
        set {
            _starsColor = value;

            CallDeferred("UpdateStarsModulate");
        }
    }

    private Achievements.Data _data;
    private string _achievementKey;


    #region Visual update
    // Stars
    public TextureRect[] GetStars()
    {
        return GetNode<VBoxContainer>("Texture/StarsC").GetChildren().OfType<TextureRect>().ToArray();
    }
    public void UpdateStarsCount()
    {
        for (int i = 1; i <= MAX_STARS; i++)
        {
            GetNode<TextureRect>("Texture/StarsC/Star" + i).Visible = i <= _starsCount;
        }
    }
    public void UpdateStarsModulate()
    {
        for (int i = 1; i <= MAX_STARS; i++)
        {
            GetNode<TextureRect>("Texture/StarsC/Star" + i).Modulate = StarsColor;
        }
    }
    public void UpdateStarsTexture()
    {
        for (int i = 1; i <= MAX_STARS; i++)
        {
            GetNode<TextureRect>("Texture/StarsC/Star" + i).Texture = StarTexture;
        }
    }

    // Icon
    public void UpdateIconRegion()
    {
        Icon.Region = new Rect2(_atlasStep * _positionInAtlas, _atlasStep);
    }
    public void UpdateIconVisible()
    {
        GetNode<TextureRect>("Texture/IconB").Visible = !HideIcon;
    }

    // Golden crosses reward
    public void UpdateReward()
    {
        var rewardText = GetNode<Label>("Texture/GoldenCrossesAnimation/RewardAmount");
        rewardText.Text = _data.RewardAmount.ToString();

        // Hiding the reward amount if there is no reward 
        GetNode<Control>("Texture/GoldenCrossesAnimation").Visible = _data.RewardAmount > 0;
    }

    // Other
    public void SetRecieved(bool value)
    {
        GetNode<Label>("NameScrollC/Name").Visible = value;
        GetNode<VBoxContainer>("Texture/StarsC").Visible = !value;
        GetNode<TextureRect>("Texture/IconB").Visible = value;
        Modulate = !value ? new Color(0.5f, 0.5f, 0.5f) : new Color(1, 1, 1);
    }

    private string _savedDesc;
    private bool _hidden = false;
    public void SetHiddenIfIsntRecieved(bool value)
    {
        var desc = GetNode<RichTextLabel>("DescScrollC/Text");

        if (!_hidden)
            _savedDesc = desc.Text;

        if (value)
        {
            desc.Text = "[HIDDEN]";
            _hidden = true;
        }
        else
        {
            desc.Text = _savedDesc;
            _hidden = false;
        }
    }
    #endregion

    private ColorRect _focusRect;
    private bool _isDisposed;
    public override async void _Ready()
    {
        // Initializing nodes
        _focusRect = GetNode<ColorRect>("FocusRect");
        _icon = (AtlasTexture)IconNode.Texture;

        _achievementKey = Path.GetFileNameWithoutExtension(SceneFilePath);
        _data = Achievements.AllTheAchievements[_achievementKey];
        UpdateReward();

        TreeExited += () => _isDisposed = true;

        // wait until IsPopupVersion is set
		try { 
		await ToSignal(GetTree(), "process_frame");} catch (ObjectDisposedException) {return;}

        if (IsPopupVersion)
        {
			try { 
			await ToSignal(GetTree().CreateTimer(2f), SceneTreeTimer.SignalName.Timeout);} catch (ObjectDisposedException) {return;}

            ScrollTexts();
            FocusMode = FocusModeEnum.None;
            MouseFilter = MouseFilterEnum.Ignore;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        _focusRect.Modulate = new Color(
        _focusRect.Modulate.R, _focusRect.Modulate.G, _focusRect.Modulate.B,
        Mathf.Clamp(HasFocus() ? _focusRect.Modulate.A + 0.2f : _focusRect.Modulate.A - 0.2f, 0, 1));
    }

    public void ScrollTexts()
    {
        var descScrollC = GetNode<AutoScrollContainer>("DescScrollC");
        var nameScrollC = GetNode<AutoScrollContainer>("NameScrollC");

        descScrollC.CallDeferred("ScrollDown");
        nameScrollC.CallDeferred("ScrollRight");
    }
    public void StopScrollingTexts()
    {
        var descScrollC = GetNode<AutoScrollContainer>("DescScrollC");
        var nameScrollC = GetNode<AutoScrollContainer>("NameScrollC");

        descScrollC.CallDeferred("Stop");
        nameScrollC.CallDeferred("Stop");
    }

    // Popup effects
    public void TimerDeleted()
    {
        Achievements.AchievementPopupTimerMultiplier--;
    }
}
