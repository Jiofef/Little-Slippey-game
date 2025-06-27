using Godot;
using System;

public partial class EnhancedButton : TextureButton
{
	Vector2 _previousFrameMousePos;
	ColorRect _focusRect;
	public override void _Ready()
	{
		_focusRect = GetNode<ColorRect>("FocusRect");

		var downSoundCallable = new Callable(GetNode<AudioStreamPlayer>("DownSound"), "play");
		if (!IsConnected("button_down", downSoundCallable))
			Connect("button_down", downSoundCallable);

		var upSoundCallable = new Callable(GetNode<AudioStreamPlayer>("UpSound"), "play");
		if (!IsConnected("button_up", upSoundCallable))
			Connect("button_up", upSoundCallable);


		FocusEntered += OnFocusEntered;
		FocusExited += OnFocusExited;
		MouseEntered += OnMouseEntered;
		MouseExited += OnMouseExited;
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 MousePos = GetLocalMousePosition();

		if (IsHovered() && !HasFocus() && !Disabled && _previousFrameMousePos != MousePos)
			GrabFocus();

		_previousFrameMousePos = MousePos;

		_focusRect.Modulate = new Color(
		_focusRect.Modulate.R, _focusRect.Modulate.G, _focusRect.Modulate.B,
		Mathf.Clamp(HasFocus() ? _focusRect.Modulate.A + 0.2f : _focusRect.Modulate.A - 0.2f, 0, !Disabled ? 1 : 0.33f));
		float Brightness = ButtonPressed ? 0.5f : 1;
		Modulate = new Color(Brightness, Brightness, Brightness);

		// Annotations
		if (AnnotationEnabled && HasFocus() && !_wasAnnotationShowed && !Disabled)
		{
			_annotationAppearingTimer -= G.FLOAT_DELTA;

			if (_annotationAppearingTimer < 0)
				ShowAnnotation();
		}
	}

	#region Annotation Segment
	[ExportGroup("Annotation properties")]
	[Export] public bool AnnotationEnabled = false;
	[Export(PropertyHint.MultilineText)] public string AnnotationText;
	[Export] public Vector2 AnnotationBoxSizeOverride = Vector2.Zero;
	[Export] public float AnnotationAppearingDelay = 0.5f;
	private float _annotationAppearingTimer;
	private bool _annotationShowed = false, _wasAnnotationShowed;
	private void ShowAnnotation()
	{
		G.AdditionalGuiLayer.AnnotationBox.PopupWithText(AnnotationText, AnnotationBoxSizeOverride != Vector2.Zero ? AnnotationBoxSizeOverride : null);
		_annotationShowed = true;
		_wasAnnotationShowed = true;
		if (!IsHovered())
			G.AdditionalGuiLayer.AnnotationBox.PinTo(GlobalPosition + Size * GetGlobalTransform().Scale / 2);
	}
	private void HideAnnotation()
	{
		_annotationShowed = false;
		G.AdditionalGuiLayer.AnnotationBox.Hide();
		_annotationAppearingTimer = AnnotationAppearingDelay;
	}

	private void OnFocusEntered()
	{
		if (AnnotationEnabled && !Disabled)
		{
			_annotationAppearingTimer = AnnotationAppearingDelay;
		}
	}
	private void OnFocusExited()
	{
		if (AnnotationEnabled && !Disabled)
		{
			_wasAnnotationShowed = false;
			HideAnnotation();
		}
	}
	private void OnMouseEntered()
	{
		if (AnnotationEnabled && !Disabled)
		{
			if (_annotationShowed)
			{
				G.AdditionalGuiLayer.AnnotationBox.UnPin();
			}
			else
			{
				// Forcing to show annotation
				_wasAnnotationShowed = false;
			}
		}
	}
	private void OnMouseExited()
	{
		if (AnnotationEnabled)
		{
			HideAnnotation();
		}
	}
	#endregion

	#region Buy segment
	[ExportGroup("Buying functions")]
	[Export] public bool EnableBuying = false;
	[Export] public int BuyingPrice = 0;
	[Export] public bool DisableWhenBought = false;
	[ExportSubgroup("Buying sound")]
	[Export] public AudioStream BuyingSound = null;
	[Export] public float BuyingSoundVolumeDB = 0;
	[Export] public string BuyingSoundBus = "Interface";
	[Signal] public delegate void OnBoughtEventHandler();
	public static Action AOnBought;

	public override void _Pressed()
	{

		if (EnableBuying)
			TryBuy();
	}

	public void TryBuy()
	{
		if (UnchangableMeta.TryBuy(BuyingPrice))
		{
			EmitSignal("OnBought");
			AOnBought?.Invoke();

			if (BuyingSound != null)
				G.PlayOneshotSound(BuyingSound, GetTree().Root, BuyingSoundBus, BuyingSoundVolumeDB);

			if (DisableWhenBought)
				Disabled = true;
		}
	}

	#endregion
}
