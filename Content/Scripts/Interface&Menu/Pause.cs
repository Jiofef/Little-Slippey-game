using Godot;
using System;
using GodotSteam;
using System.Collections.Generic;
using System.Linq;

public partial class Pause : CanvasLayer
{
    const float RESET_SPEED_MULTIPLIER = 2;
    const float DEFAULT_RESET_TIMER = 1.5f;
    TextureButton _rewindButton;
    Control _lastFocusedControl;

    private bool _isPaused = false;
    public bool ResetProcessDisabled = false;

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
            G.ResetTimer += G.FLOAT_DELTA * 2 * RESET_SPEED_MULTIPLIER;

        if (ResetProcessDisabled) return;


        if (Input.IsActionPressed("Reset") && G.DidLevelIntroPassed && !G.Main.IsResetDisabled)
            G.ResetTimer += G.FLOAT_DELTA * RESET_SPEED_MULTIPLIER;
        else G.ResetTimer = G.ResetTimer > 0 ? G.ResetTimer - G.FLOAT_DELTA * RESET_SPEED_MULTIPLIER : 0;

        if (G.ResetTimer > DEFAULT_RESET_TIMER)
            Reset();
    }

    // For pausing when controller is disabled
    private List<int> _prevControllers = new List<int>();
    public override void _Process(double delta)
    {
        var currentControllers = Input.GetConnectedJoypads();

        if (currentControllers.Count < _prevControllers.Count && !_isPaused)
        {
            UnPause();
        }

        _prevControllers = currentControllers.ToList();
    }

    private void Reset()
    {
        G.WasTheLevelRestarted = true;

        G.IsCrossesEnabled = true;
        G.IsProgressPaused = false;
        G.CrossSpawnMultiplier = 1;
        G.Main.EmitSignal("OnLevelResetting");

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

    public void UnPause()
    {
        SetPause(!GetTree().Paused);
    }

    public override void _ExitTree()
    {
        if (_isPaused)
        {
            AudioServer.SetBusEffectEnabled(2, 0, false);
            AudioServer.SetBusEffectEnabled(6, 0, false);
        }
    }

    private void SetPause(bool value)
    {
        _isPaused = value;


        var animationPlayer = GetNode<AnimationPlayer>("Interface/AnimationPlayer");
        if (_isPaused)
        {
            G.AdditionalGuiLayer.AlwaysShowGoldenCrossesAmount = true;
            // Muting the music and environment
            AudioServer.SetBusEffectEnabled(2, 0, true);
            AudioServer.SetBusEffectEnabled(6, 0, true);

            // UI preparation
            _lastFocusedControl = GetViewport().GuiGetFocusOwner();

            if (_lastFocusedControl != null && !_lastFocusedControl.IsConnected("tree_exited", new Callable(this, "ResetLastFocusedControl")))
                _lastFocusedControl.Connect("tree_exited", new Callable(this, "ResetLastFocusedControl"));

            GetNode<TextureButton>("Interface/ButtonsFrame/Resume").GrabFocus();
            GetNode<TextureButton>("Interface/ButtonsFrame/Rewind").Disabled = G.Main.IsResetDisabled;

            // Pause animation
            animationPlayer.Play("Pause");

            // Showing the mouse
            Input.MouseMode = Input.MouseModeEnum.Visible;

            // Pausing
            GetTree().Paused = true;
        }
        else
        {
            G.AdditionalGuiLayer.AlwaysShowGoldenCrossesAmount = false;
            // Unmuting the music and environment
            AudioServer.SetBusEffectEnabled(2, 0, false);
            AudioServer.SetBusEffectEnabled(6, 0, false);

            // A return of focus, if there's anything to it
            _lastFocusedControl?.GrabFocus();

            // Unpause animation
            animationPlayer.PlayBackwards("Pause");

            // Hiding the mouse
            Input.MouseMode = Input.MouseModeEnum.Hidden;

            // Unpausing
            GetTree().Paused = false;
        }
    }

    private void ResetLastFocusedControl()
    {
        _lastFocusedControl = null;
    }

    private void Options()
    {
        GetNode<Control>("Interface").ProcessMode = ProcessModeEnum.Disabled;
        GetNode<AnimationPlayer>("Interface/AnimationPlayer").Play("OpeningSubMenu");

        var options = (Control)ResourceLoader.Load<PackedScene>("res://Content/Scenes/Interface&Menu/OptionsMenu.tscn").Instantiate();
        options.TreeExited += OptionsClosing;
        AddChild(options);

        _subMenusOpened = true;
    }
    public void Menu()
    {
        GetTree().Paused = false;
        if (G.IsLevelVanilla)
        {
            UnchangableMeta.SaveRecords();
            UnchangableMeta.SaveToFile(true);
            GetTree().ChangeSceneToFile("res://Content/Scenes/Interface&Menu/WelcomeToGOS.tscn");
        }
        else
            GetTree().ChangeSceneToFile("res://Content/Scenes/Interface&Menu/LevelEditor/LevelEditor.tscn");

        if (IsInsideTree()) //In very rare cases (e.g. with a level 7 black screen), the node is not deleted correctly when the scene is changed. This fixes this bug.
            G.Main.QueueFree();

        G.CompletelyResetValues();
        G.BlockSavingSomeValues = true; //Addition to the previous bug. Read the description of the variable in G.
    }


    private void OptionsClosing()
    {
        GetNode<Control>("Interface").ProcessMode = ProcessModeEnum.WhenPaused;
        GetNode<TextureButton>("Interface/ButtonsFrame/Options").GrabFocus();
        GetNode<AnimationPlayer>("Interface/AnimationPlayer").PlayBackwards("OpeningSubMenu");
        _subMenusOpened = false;
    }

    public override void _Notification(int what)
    {
        if (what == MainLoop.NotificationApplicationFocusOut && !GetTree().Paused)
        {
            SetPause(true);
        }
    }
}
