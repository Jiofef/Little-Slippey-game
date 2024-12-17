using Godot;
using System;

public partial class HintCanvasLayer : CanvasLayer
{
    [Signal] public delegate void HintHiddenEventHandler();
    [Signal] public delegate void HintShowedEventHandler();
    [Signal] public delegate void HintWillBeShownEventHandler();
    [Signal] public delegate void HintWontBeShownEventHandler();
    [Signal] public delegate void OnLevelStartHintWillBeShownEventHandler();
    [Signal] public delegate void OnLevelStartHintWontBeShownEventHandler();

    [Export] public int HintID;

    private G.LevelStartedEventHandler _onLevelStartedHandler;
    #region Sorry for shitcode :P
    public override void _Ready()
    {
        _onLevelStartedHandler = (bool wasIntroShown) => OnLevelStarted(wasIntroShown);
        G.OnLevelStarted += _onLevelStartedHandler;


        if (UnchangableMeta.HintsStatus[HintID] == true)
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

        if (UnchangableMeta.HintsStatus[HintID] == true)
        {
            CallDeferred("emit_signal", "OnLevelStartHintWontBeShown");
            QueueFree();
        }
        else
            CallDeferred("emit_signal", "OnLevelStartHintWillBeShown");
    }
    public void ExitTree()
    {
        G.OnLevelStarted -= _onLevelStartedHandler;
    }
    #endregion



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
    }
    public void HideHint()
    {
        G.MusicPlayer.Playing = true;
        G.Main.SetPauseDisabled(false);
        GetTree().Paused = false;

        EmitSignal("HintHidden");

        QueueFree();
    }
}
