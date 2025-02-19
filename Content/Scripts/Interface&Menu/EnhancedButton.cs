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
	}

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
