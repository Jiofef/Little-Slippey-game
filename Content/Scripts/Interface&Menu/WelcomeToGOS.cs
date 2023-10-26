using Godot;

public partial class WelcomeToGOS : Control
{
	Control _openedMenu;

	int _openedMenuNumber; // if value == 0, there are no opened menus
	public override void _Ready()
	{
		if (!G.IsSystemInitiated)
		{
            Meta.Instance.LoadOptions();
            Meta.Instance.ApplyOptions();
            UnchangableMeta.LoadSave();
			GetNode<AnimationPlayer>("AnimationPlayer").Play("Initialization");

            G.IsSystemInitiated = true;
        }
		else
            GetNode<AnimationPlayer>("AnimationPlayer").Play("BackToGOS");
    }

	public void IconClick(int iconNumber)
	{
		if (_openedMenu != null)
		{
			_openedMenu.QueueFree();
			_openedMenu = null;
        }

		if (iconNumber != _openedMenuNumber)
		{
			string[] MenuNames = {"Levels", "Options", "Skins", "Achievements", "RecycleBin"};
			_openedMenu = (Control)ResourceLoader.Load<PackedScene>("res://Content/Scenes/Interface&Menu/" + MenuNames[iconNumber - 1] + "Menu.tscn").Instantiate();
            AddChild(_openedMenu);
			_openedMenuNumber = iconNumber;
		}
		else
			_openedMenuNumber = 0;
	}

	public void OpenedMenuClosed()
	{
		_openedMenu = null;
		_openedMenuNumber = 0;
		GetNode<TextureButton>("Buttons/LevelsMenuButton").GrabFocus();
	}
}
