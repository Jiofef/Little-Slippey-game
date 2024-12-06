using Godot;
public partial class MainScript : Node2D
{
    [Signal] public delegate void RecalculateCrossWeightEventHandler();
    [Signal] public delegate void LevelResetingEventHandler();
    public bool IsPauseDisabled = false, IsResetDisabled = false;
    


    public override void _Ready()
    {
        G.Main = this;
        TreeExited += () =>
        {
            if (G.Main == this)
                G.Main = null;
        };

        G.OnMusicPlayerSetted += () => 
        { 
            G.MusicPlayer.StreamPaused = false; 
        };


        GetTree().Paused = true;

        AudioServer.SetBusEffectEnabled(2, 0, false);
        AudioServer.SetBusEffectEnabled(6, 0, false);

        Achievements.CurrentPopupAchievementsLayer = GetNode<CanvasLayer>("PopupAchievementsLayer");

        Connect("RecalculateCrossWeight", new Callable(GetNode("Level"), "RecalculateCrossWeight"));

        if (G.DidLevelIntroPassed)
        {
            GetNode<CanvasLayer>("EpicIntro").QueueFree();
            GetTree().Paused = false;
            CallDeferred("CallOnLevelStarted", false);
        }
        if (G.IsLevelVanilla && UnchangableMeta.LevelPlayedStatus[G.CurrentLevel - 1] != 1)
        {
            UnchangableMeta.LevelPlayedStatus[G.CurrentLevel - 1] = 1;
            UnchangableMeta.SaveToFile();
        }
    }
    public override void _PhysicsProcess(double delta)
    {

    }
    private void CallOnLevelStarted(bool wasIntroShown)
    {
        G.OnLevelStartedFunc(wasIntroShown);
    }
    private void OnIntroFinished()
    {
        GetTree().Paused = false;
        CallDeferred("CallOnLevelStarted", true);
    }


    private void MusicAnimationFinished(string animation)
    {
        if (animation == "MusicStopping")
        {
            G.MusicPlayer.Stream = null;
            G.MusicPlayer.Stop();
        }
    }

    private void GiveAchievement(int index)
    {
        Achievements.GetAchievement(index);
    }

    //Methods from above are actively used in game scripts and their sloppy use can break some processes, use at your own risk.
    //The methods below are made specifically for modding, use them however you want.

    public void LoadScene(string ScenePath)
    {
        if (G.IsLevelVanilla)
            GetTree().ChangeSceneToFile(ScenePath);
        else
        {
            G.ModMapPath = ScenePath;
            GetTree().ChangeSceneToFile(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + @"\Godot\app_userdata\Little Slippey\mods\" + G.ModMapPath);
        }
    }
    public void SetCrossesEnabled(bool value)
    {
        G.IsCrossesEnabled = value;
    }
    public void SetCrossesSpawnMultiplier(float value)
    {
        G.CrossSpawnMultiplier = value;
    }
    public void SetCrossesProgressCoeff(float value)
    {
        G.CrossesProgressCoeff = value;
    }
    public void SetProgressPaused(bool value)
    {
        G.IsProgressPaused = value;
    }
    public void DebugTransitiveValue(int index)
    {
        GD.Print(G.TransitiveVariant[index]);
    }
    public void SetTransitiveValue(int index, Variant value)
    {
        G.TransitiveVariant[index] = value;
    }
    public void ResetTransitiveValue(int index)
    {
        G.TransitiveVariant[index] = "";
    }
    public void ResetAllTransitiveValues()
    {
        for (int i = 0; i < G.TransitiveVariant.Length; i++)
            G.TransitiveVariant[i] = "";
    }
    public void SetScores(float value)
    {
        G.Scores = value;
        EmitSignal("RecalculateCrossWeight");
    }
    public void SetPauseDisabled(bool value)
    {
        IsPauseDisabled = value;
    }
    public void SetResetDisabled(bool value)
    {
        IsResetDisabled = value;
    }
    public void SetLevelCompleteTime(float value)
    {
        G.LevelCompleteTime = value;
    }
    public void SetDebugEnabled(bool value)
    {
        G.IsDebugEnabled = value;
    }
    public void SetMapScenePath(string value)
    {
        //The path starts from "mods/" folder. For example, value can be "MyMod/MainScene.tscn" or "SomeMod/Directory/Scene1.tscn"
        G.ModMapPath = value;
    }
}