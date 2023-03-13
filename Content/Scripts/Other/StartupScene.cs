using Godot;

public partial class StartupScene : Node2D
{
	public override void _Ready()
	{
		Meta.Instance.LoadOptions();
		Meta.Instance.ApplyOptions();
		UnchangableMeta.LoadSave();
		GetTree().ChangeSceneToFile("res://Content/Scenes/Interface&Menu/Menu.tscn");
	}
}
