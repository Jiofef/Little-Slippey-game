using Godot;
public partial class MainScript : Node2D
{
    [Signal] public delegate void RecalculateCrossWeightEventHandler();

    [Signal] public delegate void LevelResetingEventHandler();

    [Signal] public delegate void OnLevelStartedEventHandler(bool wasIntroShown);
    [Signal] public delegate void OnLevelStartedNoBoolEventHandler();
    public bool IsPauseDisabled = false, IsResetDisabled = false;

    [Export] public string LevelNodePath = "Level";
    [Export] public bool DoIgnoreIntro = false, DoIgnoreAchievementLayer = false;

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

        if (!DoIgnoreAchievementLayer)
            Achievements.CurrentPopupAchievementsLayer = GetNode<CanvasLayer>("PopupAchievementsLayer");

        Connect("RecalculateCrossWeight", new Callable(GetNode(LevelNodePath), "RecalculateCrossWeight"));

        if (G.DidLevelIntroPassed || DoIgnoreIntro)
        {
            GetTree().Paused = false;
            GetNode<CanvasLayer>("EpicIntro")?.QueueFree();
            CallDeferred("CallOnLevelStarted", false);
        }
        else
            GetTree().Paused = true;


        if (G.IsLevelVanilla && UnchangableMeta.LevelPlayedStatus[G.CurrentLevel - 1] != 1)
        {
            UnchangableMeta.LevelPlayedStatus[G.CurrentLevel - 1] = 1;
            UnchangableMeta.SaveToFile();
        }
    }
    private void CallOnLevelStarted(bool wasIntroShown)
    {
        G.OnLevelStartedFunc(wasIntroShown);
        EmitSignal("OnLevelStarted", wasIntroShown);
        EmitSignal("OnLevelStartedNoBool");
    }
    public void OnIntroFinished()
    {
        GetTree().Paused = false;
        CallDeferred("CallOnLevelStarted", true);
    }


    private void GiveAchievement(string name)
    {
        Achievements.GetAchievement(name);
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
}