using Godot;
using System;
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

        QueueFree();
    }
}
