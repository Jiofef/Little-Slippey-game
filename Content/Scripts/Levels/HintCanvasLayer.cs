using Godot;
using static OtherExtension.ActionTools;

public partial class HintCanvasLayer : CanvasLayer
{
    [Signal] public delegate void HintHiddenEventHandler();
    [Signal] public delegate void HintShowedEventHandler();
    [Signal] public delegate void HintWillBeShownEventHandler();
    [Signal] public delegate void HintWontBeShownEventHandler();
    [Signal] public delegate void OnLevelStartHintWillBeShownEventHandler();
    [Signal] public delegate void OnLevelStartHintWontBeShownEventHandler();

    [Export] public int HintID;
    [Export] public bool CanSpeedUp = true;
    [Export] AnimationPlayer AnimationPlayer;
    [Export] AppearingText[] AppearingTextsToSpeedUp;
    [Export] AudioStreamPlayer[] SoundsToSpeedUp;
    [Export] float SpeedUpMultiplier = 5f;

    #region Sorry for shitcode :P
    public override void _Ready()
    {
        BindEventToNodeSafelyWithoutArgs(this, "OnLevelStarted", G.OnLevelStarted, true);

        if (UnchangableMeta.HintsStatus[HintID] == 1)
        {
            EmitSignal("HintWontBeShown");
            if (G.DidLevelIntroPassed)
                EmitSignal("OnLevelStartHintWontBeShown");
        }
        else
        {
            EmitSignal("HintWillBeShown");
            if (G.DidLevelIntroPassed)
                EmitSignal("OnLevelStartHintWillBeShown");
        }
    }
    public void OnLevelStarted(bool wasIntroShown)
    {
        if (!wasIntroShown) return;

        if (UnchangableMeta.HintsStatus[HintID] == 1)
        {
            CallDeferred("emit_signal", "OnLevelStartHintWontBeShown");
            QueueFree();
        }
        else
            CallDeferred("emit_signal", "OnLevelStartHintWillBeShown");
    }
    #endregion

    public override void _Input(InputEvent @event)
    {
        if (!Visible) return;

        if (Input.IsActionJustPressed("MouseLeftClick"))
        {
            ToggleSpeedUp(true);
        }
        if (Input.IsActionJustReleased("MouseLeftClick"))
        {
            ToggleSpeedUp(false);
        }
    }

    // Should be disabled by default
    private bool _isSpedUp = false;
    public void ToggleSpeedUp(bool enable) // Speedup must first be turned off to work properly, and so make the change at each switchover (I'm just too lazy to save all the speed values from the controlled nodes.)
    {
        if (_isSpedUp == enable || !CanSpeedUp) return;
        _isSpedUp = enable;

        if (enable)
        {
            AnimationPlayer.SpeedScale *= SpeedUpMultiplier;
            foreach(AppearingText text in AppearingTextsToSpeedUp)
            {
                text.CharactersPerSecond *= SpeedUpMultiplier;
            }
            foreach (AudioStreamPlayer sound in SoundsToSpeedUp)
            {
                sound.PitchScale *= SpeedUpMultiplier;
            }
        }
        else
        {
            AnimationPlayer.SpeedScale /= SpeedUpMultiplier;
            foreach (AppearingText text in AppearingTextsToSpeedUp)
            {
                text.CharactersPerSecond /= SpeedUpMultiplier;
            }
            foreach (AudioStreamPlayer sound in SoundsToSpeedUp)
            {
                sound.PitchScale /= SpeedUpMultiplier;
            }
        }
    }

    public void Glitch()
    {
        Show();
        G.MusicPlayer.StreamPaused = true;
        G.Main.SetPauseDisabled(true);
        GetTree().Paused = true;

        var glitch = GetNode<AnimatedSprite2D>("Glitch");
        glitch.Visible = true;
        glitch.Play();
        GetNode<AudioStreamPlayer>("Glitch/GlitchSound").Play();
    }

    public void ShowHint()
    {
        Show();
        G.MusicPlayer.Stop();
        G.Main.SetPauseDisabled(true);
        GetTree().Paused = true;

        GetNode<AudioStreamPlayer>("DarkBackground/Appearing").Play();
        GetNode<ColorRect>("DarkBackground").Visible = true;

        EmitSignal("HintShowed");

        UnchangableMeta.HintsStatus[HintID] = 1;
        UnchangableMeta.SaveToFile();
    }
    public void HideHint()
    {
        G.MusicPlayer.Playing = true;
        G.Main.SetPauseDisabled(false);
        GetTree().Paused = false;

        EmitSignal("HintHidden");

        G.PlayOneshotSound("Other/Snare.mp3", GetTree().Root, "Interface");
        GetNode<AudioStreamPlayer>("DarkBackground/Appearing").Play();

        QueueFree();
    }
}
