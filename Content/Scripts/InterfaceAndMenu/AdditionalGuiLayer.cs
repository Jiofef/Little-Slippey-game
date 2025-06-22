using Godot;
using System;
using System.Threading.Tasks;
using static OtherExtension.GodotExtensions;

public partial class AdditionalGuiLayer : CanvasLayer
{
	// Nodes
	public Sprite2D ExtraCursor;
	public AnnotationBox AnnotationBox;


	// Other variables
    private bool _alwaysShowGoldenCrossesAmount = false;
    public bool AlwaysShowGoldenCrossesAmount
    {
        get => _alwaysShowGoldenCrossesAmount;
        set
        {
            _alwaysShowGoldenCrossesAmount = value;

            var goldenCrossesAmount = GetNode<GoldenCrossesAmount>("ScreenControl/MarginC/GoldenCrossesAmount");
            var animationPlayer = GetNode<AnimationPlayer>("ScreenControl/MarginC/GoldenCrossesAmount/AnimationPlayer");

            if (goldenCrossesAmount.Visible && !value)
            {
                animationPlayer.Stop();
                animationPlayer.Play("Disappearing");
            }

            if ((!goldenCrossesAmount.Visible || animationPlayer.CurrentAnimation == "Disappearing") && value)
            {
                if (animationPlayer.CurrentAnimation == "Disappearing")
                    animationPlayer.Stop();

                goldenCrossesAmount.SetDeferred("visible", true);
                goldenCrossesAmount.SetDeferred("modulate", new Color(1, 1, 1));
            }
        }
    }


    public override void _Ready()
	{
        // Initializing the nodes
		ExtraCursor = GetNode<Sprite2D>("ScreenControl/ExtraCursor");
		AnnotationBox = GetNode<AnnotationBox>("ScreenControl/AnnotationBox");

        _goldenCrossesCountAnimation = GetNode<AnimationPlayer>("ScreenControl/MarginC/GoldenCrossesAmount/AnimationPlayer");
		WhiteNoiseGlitchLayer = GetNode<Parallax2D>("WhiteNoiseGlitchLayer");
    }

    public void OnGoldenCrossesRecieved(int value)
    {
        // Updating visible amount
        GetNode<Label>("ScreenControl/MarginC/GoldenCrossesAmount/HBoxContainer/AmountLabel").Text = UnchangableMeta.GoldenCrossesAmount.ToString();

        // Effects when amount gets visible
        var animationPlayer = GetNode<AnimationPlayer>("ScreenControl/MarginC/GoldenCrossesAmount/AnimationPlayer");
        animationPlayer.Stop();
        animationPlayer.Play("OnRecieved");
    }

    private AnimationPlayer _goldenCrossesCountAnimation;
    public void OnGoldenCrossesAnimationFinished(string name)
    {
        if (name == "OnRecieved" && !AlwaysShowGoldenCrossesAmount)
            _goldenCrossesCountAnimation.Play("Disappearing");
    }

    public void PlayGoldenCrossesRecievedAdditionalSound()
    {
        GetNode<AudioStreamPlayer>("ScreenControl/MarginC/GoldenCrossesAmount/GoldenCrossesRecievedAdditionalSound").Play();
    }

	public Parallax2D WhiteNoiseGlitchLayer;
    public async Task Glitch()
    {
		WhiteNoiseGlitchLayer.Show();

        var whiteNoiseGlitch = WhiteNoiseGlitchLayer.GetNode<AnimatedSprite2D>("WhiteNoiseGlitch");
        whiteNoiseGlitch.Play();

        whiteNoiseGlitch.GetNode<AudioStreamPlayer>("AudioStreamPlayer").Play();

		if (G.Player != null)
		{
			WhiteNoiseGlitchLayer.Reparent(G.Player.GetParent());

			G.Player.ZIndex += 200;
			bool playerGuiVisible = G.Player.GUI.Visible;
			G.Player.SetGUIVisible(false);

			try { 
			await ToSignal(whiteNoiseGlitch, "animation_finished");} catch (ObjectDisposedException) {return;}

			WhiteNoiseGlitchLayer.Reparent(this);
			G.Player.ZIndex -= 200;
			G.Player.SetGUIVisible(playerGuiVisible);
		}
    }
}
