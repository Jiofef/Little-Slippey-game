using Godot;

public partial class PlayPart : Node2D
{
    public override void _Ready()
    {
        AddChild(ResourceLoader.Load<PackedScene>("res://Content/Scenes/Levels/FullParts/Level" + G.CurrentLevel + ".tscn").Instantiate());
    }
}