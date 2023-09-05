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
            G.IsLevel9PlatformSectionSkips = true;
    }
    public void AhahahSilly()
    {
        UnchangableMeta.IsLevel9PlatformSectionSkipAllowed = true;
        G.IsLevel9PlatformSectionSkips = true;
        Connect("Reset", new Callable(GetNode(".."), "Reset"));
        EmitSignal("Reset");
    }
}
