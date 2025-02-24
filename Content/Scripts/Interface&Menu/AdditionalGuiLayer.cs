using Godot;
using System;
using static OtherExtension.GodotExtensions;

public partial class AdditionalGuiLayer : CanvasLayer
{
    // Object with this script should be only 1 on the scene, otherwise the previous ones will stop working.

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
        G.AdditionalGuiLayer = this;

        // Initializing the nodes
        _goldenCrossesCountAnimation = GetNode<AnimationPlayer>("ScreenControl/MarginC/GoldenCrossesAmount/AnimationPlayer");
    }

    public override void _ExitTree()
    {
        if (G.AdditionalGuiLayer == this)
        {
            G.AdditionalGuiLayer = null;
        }
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

    public void Glitch()
    {
        var whiteNoiseGlitch = GetNode<AnimatedSprite2D>("WhiteNoiseGlitch");
        whiteNoiseGlitch.Show();
        whiteNoiseGlitch.Play();

        GetNode<AudioStreamPlayer>("WhiteNoiseGlitch/AudioStreamPlayer").Play();
    }
}
