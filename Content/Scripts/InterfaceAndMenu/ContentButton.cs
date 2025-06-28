using Godot;
using System;
using static ContentManager;

[Tool]
public partial class ContentButton : EnhancedButton
{
	[Export] protected ContentTypeEnum _contentType;
	[Export] protected string _contentName;
	[Export] protected string _displayedName;
	protected Texture2D _icon;
	[Export] public Texture2D Icon { get => _icon; set => CallDeferred(nameof(SetIcon), value); }
	public void SetIcon(Texture2D icon)
	{
		_icon = icon;
		GetNode<TextureRect>("Icon").Texture = _icon;
	}

	protected IContent _content;


	public void SetBindedContent(ContentTypeEnum contentType, string contentName)
	{
		_contentType = contentType;
		_contentName = contentName;

		_content = GetContent<IContent>(contentType, contentName);
	}

	[Signal] public delegate void TryingToBuyContentEventHandler();
	[Signal] public delegate void ActivatedEventHandler();
	// Properties
	const string DISPLAYED_NAME_PART_1 = "[center]";
	public void UpdateName()
	{
		if (_content is not IUnlockableContent content || content.IsUnlocked)
			GetNode<RichTextLabel>("ContentName/Text").Text = DISPLAYED_NAME_PART_1 + Tr(_displayedName);
		else
			GetNode<RichTextLabel>("ContentName/Text").Text = DISPLAYED_NAME_PART_1 + "???";
	}
	const string PRICE_LABEL_PART_1 = "[center]", PRICE_LABEL_PART_2 = " [img]res://Content/Sprites/Interface/4XMiniGoldenCross.png[/img]";

	// Nodes
	public Sprite2D QuestionMark;
	public RichTextLabel PriceLabel;
	public RichTextLabel NameLabel;
	public TextureRect IconNode;
	public ColorRect DarkeningRect;

	public enum State { Locked, IsntBought, Available }

	public void SetVisualState(State state)
	{
		switch (state)
		{
			case State.Locked:
				Disabled = true;

				UpdateName();

				QuestionMark.Visible = true;

				DarkeningRect.Modulate = new Color(1, 1, 1);

				IconNode.Visible = false;
				break;

			case State.IsntBought:
				Disabled = false;

				UpdateName();
				PriceLabel.Visible = true;

				QuestionMark.Visible = false;

				DarkeningRect.Modulate = new Color(1, 1, 1);

				IconNode.Visible = true;
				break;

			case State.Available:
				Disabled = false;

				UpdateName();
				PriceLabel.Visible = false;

				QuestionMark.Visible = false;

				DarkeningRect.Modulate = new Color(1, 1, 1, 0);

				IconNode.Visible = true;
				break;
		}
	}

	public override void _Ready()
	{
		base._Ready();

		// Initializing the nodes
		QuestionMark = GetNode<Sprite2D>("?");
		PriceLabel = GetNode<RichTextLabel>("BuyingPrice");
		NameLabel = GetNode<RichTextLabel>("ContentName/Text");
		IconNode = GetNode<TextureRect>("Icon");
		DarkeningRect = GetNode<ColorRect>("Darkening");

		SetBindedContent(_contentType, _contentName);

		UpdateMainInfo();

		// If content is locked
		if (_content is IUnlockableContent uContent && !uContent.IsUnlocked)
		{
			SetVisualState(State.Locked);
		}
		// If content isn't bought
		else if (_content is IPurchasableContent pContent && !pContent.IsBought)
		{
			SetVisualState(State.IsntBought);
		}
		// If content is available
		else
		{
			SetVisualState(State.Available);

			if (_content is IToggleableContent tContent)
				ButtonPressed = tContent.IsActivated;
		}
	}

	public string GetPriceRichText(IPurchasableContent content)
	{
		return PRICE_LABEL_PART_1 + content.Cost + PRICE_LABEL_PART_2;
	}

	public void UpdateMainInfo()
	{
		if (_content is IPurchasableContent pContent && !pContent.IsBought)
			GetNode<RichTextLabel>("BuyingPrice").Text = GetPriceRichText(pContent);
	}

	#region Action
	public override void _Pressed()
	{
		base._Pressed();
		if (_content is IPurchasableContent pContent && !pContent.IsBought)
			TryBuyContent(pContent);
		else
		{
			HandleActivate();
			EmitSignal(nameof(Activated));
		}

	}

	public void TryBuyContent(IPurchasableContent pContent)
	{
		EmitSignal(nameof(TryingToBuyContent));
		ButtonPressed = false;
		if (UnchangableMeta.TryBuy(pContent.Cost))
			BuyContent(pContent);
	}

	public void BuyContent(IPurchasableContent pContent)
	{
		pContent.IsBought = true;
		ContentManager.QueueSave();

		SetVisualState(State.Available);

		// Darkening disappearing
		DarkeningRect.Modulate = new Color(1, 1, 1, 1);
		Tween appearingTween = GetTree().CreateTween();
		appearingTween.TweenProperty(DarkeningRect, "modulate", new Color(1, 1, 1, 0), 1).SetEase(Tween.EaseType.Out);
	}

	public virtual void HandleActivate()
	{}
	#endregion
}
