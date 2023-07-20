using Godot;

public partial class LevelsMenu : Control
{
    int _chosenLevel = 1;
    Node2D PresentedLevel;
	public override void _Ready()
	{
        GetNode<TextureButton>("Computer/Buttons/Play").GrabFocus();
        LevelWasChanged();
	}

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("Cancel"))
            Cancel();
    }
    public void Play()
    {
        G.CurrentLevel = _chosenLevel;
        GetTree().ChangeSceneToFile("res://Content/Scenes/Other/Main.tscn");
    }
    public void Cancel()
    {
        GetTree().ChangeSceneToFile("res://Content/Scenes/Interface&Menu/Menu.tscn");
    }
    public void ChangeToPastLevel()
    {
        _chosenLevel--;
        if (_chosenLevel < 1)
            _chosenLevel = G.LevelsInGameTotal;

        LevelWasChanged();
    }
    public void ChangeToNextLevel()
    {
        _chosenLevel++;
        if (_chosenLevel > G.LevelsInGameTotal)
            _chosenLevel = 1;

        LevelWasChanged();
    }

    private void LevelWasChanged()
    {
        if (PresentedLevel != null)
            PresentedLevel.QueueFree();

        PresentedLevel = (Node2D)ResourceLoader.Load<PackedScene>("res://Content/Scenes/Levels/PresentedParts/PresentedLevel" + _chosenLevel + ".tscn").Instantiate();
        GetNode<Label>("Computer/CameraNumber").Text = "Cam " + _chosenLevel.ToString();
        GetNode<Label>("Computer/MaxLivetime").Text = "MaxLivetime: " + UnchangableMeta.LevelRecords[Meta.Instance.Dificulty][_chosenLevel - 1].ToString();
        var subViewport = GetNode("Computer/Screen/SubViewport");
        subViewport.AddChild(PresentedLevel);
        subViewport.MoveChild(PresentedLevel, 0);
    }
}
