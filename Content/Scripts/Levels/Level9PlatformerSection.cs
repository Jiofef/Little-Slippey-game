using Godot;

public partial class Level9PlatformerSection : TileMap
{
    [Signal] public delegate void ResetEventHandler();
    public override void _Ready()
    {
        if (UnchangableMeta.IsLevel9PlatformSectionFirstTimeCompleted && !UnchangableMeta.IsLevel9PlatformSectionSkipAllowed)
        {
            var ahahahSilly = (VideoStreamPlayer)ResourceLoader.Load<PackedScene>("res://Content/Scenes/Other/Level9AhahahSilly.tscn").Instantiate();
            ahahahSilly.Connect("finished", new Callable(this, "AhahahSilly"));
            GetNode<CanvasLayer>("CanvasLayer").AddChild(ahahahSilly);
        }
    }
    public void PlatformSectionCompleted()
    {
        if (!UnchangableMeta.IsLevel9PlatformSectionFirstTimeCompleted)
        UnchangableMeta.IsLevel9PlatformSectionFirstTimeCompleted = true;

        if (UnchangableMeta.IsLevel9PlatformSectionSkipAllowed)
            G.LevelAdditionalLink = "WithoutPlatformSection";
    }
    public void AhahahSilly()
    {
        G.GetAchievement(36);
        UnchangableMeta.IsLevel9PlatformSectionSkipAllowed = true;
        G.LevelAdditionalLink = "WithoutPlatformSection";
        Connect("Reset", new Callable(GetNode(".."), "Reset"));
        EmitSignal("Reset");
    }
}
