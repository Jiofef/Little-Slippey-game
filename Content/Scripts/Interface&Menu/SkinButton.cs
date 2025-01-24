using Godot;
using System;
using static Skins;

[Tool]
// For correct work it is required to add a skin with the name AnimatedSprite2D of the corresponding type.
public partial class SkinButton : EnhancedButton
{
    // Properties
    private string _skinName = "Skin";
    [Export] public string SkinName 
    {
        get => _skinName;
        set
        {
            _skinName = value;
            var skinName = GetNode<Label>("SkinName");
            skinName.Text = Tr(value);
        }
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
        if (Achievements.AllTheAchievements.TryGetValue(NeededAchievementKey, out Achievements.Data data) && !data.IsReceived)
        {
            Disabled = true;

            SkinAnimation.Visible = false;
            SkinNameLabel.Text = "???";
            QuestionMark.Visible = true;
        }
        // If skin isn't bought
        else if (IsSkinBoughtDic.TryGetValue(SkinKey, out bool value) && !value)
        {
            SkinNameLabel.Visible = false;
            SkinPriceLabel.Visible = true;

            SkinAnimation.Modulate = hiddenColor;
        }
        // If skin is bought
        else
        {
            QuestionMark.QueueFree();
            SkinPriceLabel.QueueFree();
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

        if (IsSkinBoughtDic.TryGetValue(SkinKey, out bool value) && !value)
            TryBuySkin();
        else
            SetSkin();
    }

    public void TryBuySkin()
    {
        if (UnchangableMeta.GoldenCrossesAmount >= SkinPrice)
            BuySkin();
        else
        {
            G.PlayOneshotSound("Interface&Menu/CantBuy.mp3", GetTree().Root, "Interface");
        }
    }

    public void BuySkin()
    {
        G.PlayOneshotSound("Interface&Menu/Buy.mp3", GetTree().Root, "Interface");
        UnchangableMeta.GoldenCrossesAmount -= SkinPrice;

        IsSkinBoughtDic[SkinKey] = true;
    }

    public void SetSkin()
    {
        Meta.Instance.Gameplay.ChosenSkinKey = SkinKey;
        Meta.Instance.SaveToFile();
    }
    #endregion
}
