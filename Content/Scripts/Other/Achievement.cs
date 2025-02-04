using Godot;
using System.Collections.Generic;
using System.Linq;

[Tool]
public partial class Achievement : Control
{
    [Export] public bool IsPopupVersion = false;
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

    }

    // Icon
    public void UpdateIconRegion()
    {
        Icon.Region = new Rect2(_atlasStep * _positionInAtlas, _atlasStep);
    }
    #endregion

    private ColorRect _focusRect;
    public override void _Ready()
    {
        // Initializing nodes
        _focusRect = GetNode<ColorRect>("FocusRect");
        _icon = (AtlasTexture)IconNode.Texture;
    }

    public override void _PhysicsProcess(double delta)
    {
        _focusRect.Modulate = new Color(
        _focusRect.Modulate.R, _focusRect.Modulate.G, _focusRect.Modulate.B,
        Mathf.Clamp(HasFocus() ? _focusRect.Modulate.A + 0.2f : _focusRect.Modulate.A - 0.2f, 0, 1));
    }

    public void TimerDeleted()
    {
        Achievements.AchievementPopupTimerMultiplier--;
    }
}
