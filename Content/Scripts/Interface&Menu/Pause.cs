using Godot;
using System;

public partial class Pause : CanvasLayer
{
    TextureButton _rewindButton;

    private bool _isPaused = false;

    public override void _Ready()
    {
        _rewindButton = GetNode<TextureButton>("Interface/ButtonsFrame/Rewind");
    }
    private bool _subMenusOpened;
    public override void _PhysicsProcess(double delta)
	{
        if (Input.IsActionJustPressed("Cancel") && !_subMenusOpened && G.DidLevelIntroPassed && !G.Main.IsPauseDisabled)
            UnPause();
        if (_rewindButton.ButtonPressed && !G.Main.IsResetDisabled)
            G.ResetTimer += 0.016667f * 2;

        if (Input.IsActionPressed("Reset") && G.DidLevelIntroPassed && !G.Main.IsResetDisabled)
            G.ResetTimer += 0.016667f;
        else G.ResetTimer = G.ResetTimer > 0 ? G.ResetTimer - 0.016667f : 0;

        if (G.ResetTimer > 1.5f)
        {
            G.IsCrossesEnabled = true;
            G.IsProgressPaused = false;
            G.CrossSpawnMultiplier = 1;
            G.Main.EmitSignal("LevelReseting");
            if (G.IsLevelVanilla)
            {
                UnchangableMeta.SaveRecords();
                G.Main.LoadScene("res://Content/Scenes/Levels/FullParts/Level" + G.CurrentLevel + G.LevelAdditionalLink + ".tscn");
            }
            else
            {
                G.Main.LoadScene(G.ModMapPath);
            }
        }
    }

    private void UnPause()
    {
        ChangePause(GetTree().Paused);
    }

    public override void _ExitTree()
    {
        if (_isPaused)
        {
            AudioServer.SetBusEffectEnabled(2, 0, false);
            AudioServer.SetBusEffectEnabled(6, 0, false);
        }
    }

    private void ChangePause(bool IsPaused)
    {
        _isPaused = !IsPaused;
        AudioServer.SetBusEffectEnabled(2, 0, !IsPaused);
        AudioServer.SetBusEffectEnabled(6, 0, !IsPaused);
        GetNode<TextureButton>("Interface/ButtonsFrame/Resume").GrabFocus();
        GetNode<TextureButton>("Interface/ButtonsFrame/Rewind").Disabled = G.Main.IsResetDisabled;

        var animationPlayer = GetNode<AnimationPlayer>("Interface/AnimationPlayer");
        if (!IsPaused)
            animationPlayer.Play("Pause");
        else
            animationPlayer.PlayBackwards("Pause");

        Input.MouseMode = IsPaused ? Input.MouseModeEnum.Hidden : Input.MouseModeEnum.Visible;

        GetTree().Paused = !IsPaused;
    }

    private void Options()
    {
        GetNode<Control>("Interface").ProcessMode = ProcessModeEnum.Disabled;
        AddChild(ResourceLoader.Load<PackedScene>("res://Content/Scenes/Interface&Menu/OptionsMenu.tscn").Instantiate<Control>());
        GetNode<AnimationPlayer>("Interface/AnimationPlayer").Play("OpeningSubMenu");
        _subMenusOpened = true;
    }
    private void Menu()
    {
        GetTree().Paused = false;
        if (G.IsLevelVanilla)
        {
            UnchangableMeta.SaveRecords();
            UnchangableMeta.SaveToFile();
            GetTree().ChangeSceneToFile("res://Content/Scenes/Interface&Menu/WelcomeToGOS.tscn");
        }
        else
            GetTree().ChangeSceneToFile("res://Content/Scenes/Interface&Menu/LevelEditor/LevelEditor.tscn");

        G.CompletelyResetValues();
    }


    private void OptionsClosing()
    {
        GetNode<Control>("Interface").ProcessMode = ProcessModeEnum.WhenPaused;
        GetNode<TextureButton>("Interface/ButtonsFrame/Options").GrabFocus();
        GetNode<AnimationPlayer>("Interface/AnimationPlayer").PlayBackwards("OpeningSubMenu");
        _subMenusOpened = false;
    }
}
