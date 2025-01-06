using Godot;
using GodotSteam;

public partial class WelcomeToGOS : Control
{
	Control _openedMenu;

    int _openedMenuNumber; // if value == 0, there are no opened menus
    public override void _Ready()
	{
        G.BlockSavingSomeValues = false; //A small crutch to fix the engine bug. Read the assignment of a variable in G.

        Achievements.CurrentPopupAchievementsLayer = GetNode<CanvasLayer>("PopupAchievementsLayer");
		GetNode<Sprite2D>("Buttons/Titles/Notify").Visible = UnchangableMeta.IsThereNewContentInRecycleBin;
		if (!G.IsSystemInitiated)
		{
			GetNode<AnimationPlayer>("AnimationPlayer").Play("Initialization");

            G.IsSystemInitiated = true;
        }
		else
            GetNode<AnimationPlayer>("AnimationPlayer").Play("BackToGOS");


        // StandartTimerLib checking
        ModManager.UpdateIsStandartTimerLibLoaded();
        if (!ModManager.IsStandartTimerLibExists())
        {
            var timerLibWarning = GD.Load<PackedScene>("res://Content/Scenes/Interface&Menu/TimerLibNotFound.tscn").Instantiate();
            AddChild(timerLibWarning);
        }
        else if (!ModManager.IsStandartTimerLibLoaded)
        {
            var timerLibWarning = GD.Load<PackedScene>("res://Content/Scenes/Interface&Menu/TimerLibIsDisabled.tscn").Instantiate();
            AddChild(timerLibWarning);
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
}
