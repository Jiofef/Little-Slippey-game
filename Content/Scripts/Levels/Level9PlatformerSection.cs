using Godot;

public partial class Level9PlatformerSection : Node2D
{
    [Signal] public delegate void LoadSceneEventHandler();
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
        // The possibility of skipping the platform part in the future
        UnchangableMeta.IsLevel9PlatformSectionSkipAllowed = true;
        UnchangableMeta.SaveToFile();

        // Preparing to load the next scene
        G.LevelAdditionalLink = "WithoutPlatformSection";
        G.MusicPlayer.StopMusic();

        // Loading the next scene
        Connect("LoadScene", new Callable(GetNode("../.."), "LoadScene"));
        EmitSignal("LoadScene", "res://Content/Scenes/Levels/FullParts/Level" + G.CurrentLevel + G.LevelAdditionalLink + ".tscn");

        // Issuing an achievement after switching the scene
        Achievements.GetAchievementAfter(0.3f, "Did you really fall for that");
    }
}
