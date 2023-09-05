using Godot;

public partial class PlayPart : Node2D
{
    public override void _Ready()
    {
        LevelLoad();
    }

    public void LevelLoad()
    {
        if (G.CurrentLevel == 9 && G.IsLevel9PlatformSectionSkips)
        {
            AddChild(ResourceLoader.Load<PackedScene>("res://Content/Scenes/Levels/FullParts/Level9WithoutPlatformSection.tscn").Instantiate());
            return;
        }

        AddChild(ResourceLoader.Load<PackedScene>("res://Content/Scenes/Levels/FullParts/Level" + G.CurrentLevel + ".tscn").Instantiate());
    }
}