using System;
using System.Threading.Tasks;
using Godot;
using GodotSteam;

public partial class WelcomeToGOS : Control
{
	Control _openedMenu;
	AnimationPlayer _animationPlayer;

	[Signal] public delegate void CameraOpenFinishedEventHandler();
	

    int _openedMenuNumber; // if value == 0, there are no opened menus
    public override void _Ready()
	{
		// Initializing the nodes
		_animationPlayer = GetNode<AnimationPlayer>("Camera2D/AnimationPlayer");

        G.BlockSavingSomeValues = false; //A small crutch to fix the engine bug. Read the assignment of a variable in G.


		if (!G.IsSystemInitiated)
		{
			_animationPlayer.Play("Initialization");

            G.IsSystemInitiated = true;
        }
		else
            _animationPlayer.Play("BackToGOS");

		_animationPlayer.AnimationFinished += (animName) =>
		{
			if (animName == "Open")
				EmitSignal(nameof(CameraOpenFinished));
		};


        if (ModManager.IsModsDisabled) return; // Below are things related to mods

        // StandartTimerLib checking
        ModManager.UpdateIsStandartTimerLibLoaded();
        if (!ModManager.IsStandartTimerLibExists())
        {
            var timerLibWarning = GD.Load<PackedScene>("res://Content/Scenes/Interface&Menu/RareScenes/TimerLibNotFound.tscn").Instantiate();
            AddChild(timerLibWarning);
        }
        else if (!ModManager.IsStandartTimerLibLoaded)
        {
            var timerLibWarning = GD.Load<PackedScene>("res://Content/Scenes/Interface&Menu/RareScenes/TimerLibIsDisabled.tscn").Instantiate();
            AddChild(timerLibWarning);
        }

        // Message about emergency shutdown of mods
        if (UnchangableMeta.DidModsCrushedTheGame)
        {
            var modsDisabledWarning = (ConfirmationWindow)GD.Load<PackedScene>("res://Content/Scenes/Interface&Menu/RareScenes/ModsDisabledWarning.tscn").Instantiate();
            AddChild(modsDisabledWarning);
            modsDisabledWarning.Accepted += () =>
            {
                UnchangableMeta.DidModsCrushedTheGame = false;
            };
        }
    }
    public void IconClick(int iconNumber)
    {
        if (_openedMenu != null)
        {
            _openedMenu.TreeExited -= OpenedMenuClosed;
            RemoveChild(_openedMenu);
            _openedMenu = null;

            if (iconNumber == _openedMenuNumber)
            {
                _openedMenuNumber = 0;
                return;
            }
        }

        string[] menuNames = { "Levels", "Options", "Skins", "Achievements", "RecycleBin", "TurnOff", "WorkShop" };

        if (iconNumber > 0 && iconNumber <= menuNames.Length)
        {
            var scenePath = $"res://Content/Scenes/Interface&Menu/{menuNames[iconNumber - 1]}Menu.tscn";
            var scene = ResourceLoader.Load<PackedScene>(scenePath);
            if (scene == null) return;

            _openedMenu = (Control)scene.Instantiate();
            AddChild(_openedMenu);

            _openedMenu.TreeExited += OpenedMenuClosed;
            _openedMenuNumber = iconNumber;
        }
    }

    public void OpenedMenuClosed()
    {
        _openedMenu = null;
        _openedMenuNumber = 0;
        GetNode<TextureButton>("Buttons/LevelsMenuButton").GrabFocus();
    }

	public void OpenCameraEffect()
	{
		_animationPlayer.Play("Open");
	}
}
