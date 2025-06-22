
using Godot;
using System;
using System.Reflection.Metadata.Ecma335;
using static Skins;

[Tool]
// For correct work it is required to add a skin with the name AnimatedSprite2D of the corresponding type.
public partial class SkinButton : EnhancedButton
{
    [Signal] public delegate void TryingToBuySkinEventHandler();
    // Properties
    private string _skinName = "Skin";
    [Export(PropertyHint.MultilineText)] public string SkinName 
    {
        get => _skinName;
        set
        {
            _skinName = value;

            CallDeferred("UpdateSkinName");
        }
    }
    public void UpdateSkinName()
    {
        if (IsSkinUnlocked())
            GetNode<Label>("SkinName").Text = _skinName;
    }
    [Export] public string SkinKey = "";
    [Export] public string NeededAchievementKey = "";
    [Export] public int SkinPrice = 50;
    const string PRICE_LABEL_PART_1 = "[center]", PRICE_LABEL_PART_2 = " [img]res://Content/Sprites/Interface/4XMiniGoldenCross.png[/img]";

    // Nodes
    public Sprite2D QuestionMark;
    public RichTextLabel SkinPriceLabel;
    public Label SkinNameLabel;
    public AnimatedSprite2D SkinAnimation;

    public bool IsSkinUnlocked()
    {
        if (NeededAchievementKey == "")
            return true;

        return Achievements.AllTheAchievements.TryGetValue(NeededAchievementKey, out Achievements.Data data) && data.IsReceived;
    }
    public bool IsSkinBought()
    {
        if (SkinPrice == 0)
            return true;

        return IsSkinBoughtDic.TryGetValue(SkinKey, out bool value) && value;
    }


    // Only for buying animation
    private float _skinAppearCoeff = 0;
    private const float SKIN_APPEAR_SPEED = 0.001f;

    public enum State {Locked, IsntBought, Bought}

    public void SetVisualState(State state)
    {
        switch(state)
        {
            case State.Locked:
                Disabled = true;

                SkinNameLabel.Text = "???";

                QuestionMark.Visible = true;

                SkinAnimation.Visible = false;
                break;

            case State.IsntBought:
                Disabled = false;

                UpdateSkinName();
                SkinNameLabel.Visible = false;
                SkinPriceLabel.Visible = true;

                QuestionMark.Visible = false;

                SkinAnimation.Visible = true;
                SkinAnimation.Modulate = new Color(0, 0, 0);
                break;

            case State.Bought:
                Disabled = false;

                UpdateSkinName();
                SkinPriceLabel.Visible = false;
                SkinNameLabel.Visible = true;

                QuestionMark.Visible = false;

                SkinAnimation.Visible = true;
                SkinAnimation.Modulate = new Color(1, 1, 1);
                break;
        }
    }

    public override void _Ready()
    {
        base._Ready();

        // Initializing the nodes
        QuestionMark = GetNode<Sprite2D>("?");
        SkinPriceLabel = GetNode<RichTextLabel>("SkinPrice");
        SkinNameLabel = GetNode<Label>("SkinName");
        SkinAnimation = GetNode<AnimatedSprite2D>("AnimatedSprite2D");

        UpdateMainInfo();

        Color hiddenColor = new Color("2f2e31");

        // If skin is locked
        if (!IsSkinUnlocked())
        {
            SetVisualState(State.Locked);
        }
        // If skin isn't bought
        else if (!IsSkinBought())
        {
            SetVisualState(State.IsntBought);
        }
        // If skin is bought
        else
        {
            SetVisualState(State.Bought);
        }
    }

    public string GetPriceRichText()
    {
        return PRICE_LABEL_PART_1 + SkinPrice + PRICE_LABEL_PART_2;
    }

    public void UpdateMainInfo()
    {
        GetNode<RichTextLabel>("SkinPrice").Text = GetPriceRichText();
    }

    #region Action
    public override void _Pressed()
    {
        base._Pressed();
        if (!IsSkinBought())
            TryBuySkin();
        else
            SetSkin();
    }

    public void TryBuySkin()
    {
        EmitSignal(nameof(TryingToBuySkin));
        ButtonPressed = false;
        if (UnchangableMeta.TryBuy(SkinPrice))
            BuySkin();
    }

    public void BuySkin()
    {
        IsSkinBoughtDic[SkinKey] = true;

        SetVisualState(State.Bought);

        // Because SetVisualState makes the skin visible.
        SkinAnimation.Modulate = new Color(0, 0, 0);

        // Smooth animation of skin appearance
        Tween appearingTween = GetTree().CreateTween();
        appearingTween.TweenProperty(SkinAnimation, "modulate", new Color(1, 1, 1), 1).SetEase(Tween.EaseType.Out);
    }

    public void SetSkin()
    {
        Skins.SetSkin(SkinKey);
    }
    #endregion
}
