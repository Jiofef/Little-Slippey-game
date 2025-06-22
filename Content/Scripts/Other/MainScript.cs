using Godot;
using System;
using static OtherExtension.ActionTools;
public partial class MainScript : Node2D
{
    [Signal] public delegate void RecalculateCrossWeightEventHandler();

    [Signal] public delegate void LevelResetEventHandler();

    [Signal] public delegate void LevelStartedEventHandler(bool wasIntroShown);
    [Signal] public delegate void LevelStartedNoArgEventHandler();
    public bool IsPauseDisabled = false, IsResetDisabled = false;

    [Export] public string LevelNodePath = "Level";
    [Export] public bool DoIgnoreIntro = false;

    public override void _EnterTree()
    {
        G.Main = this;
    }
    public override void _Ready()
    {
        TreeExited += () =>
        {
            if (G.Main == this)
                G.Main = null;
        };

        Action unpauseMusicPlayerAction = () => G.MusicPlayer.StreamPaused = false;
        BindEventSafelyTo(G.OnMusicPlayerSetted, unpauseMusicPlayerAction);


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
        EmitSignal(nameof(LevelStarted), wasIntroShown);
        EmitSignal(nameof(LevelStartedNoArg));
    }
    public void OnIntroFinished()
    {
        GetTree().Paused = false;
        CallDeferred(nameof(CallOnLevelStarted), true);
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
            GetTree().ChangeSceneToFile(ModManager.DefaultModsPath + G.ModMapPath);
        }

        if (IsInsideTree()) //In very rare cases (e.g. with a level 7 black screen), the node is not deleted correctly when the scene is changed. This fixes this bug.
            QueueFree();
    }
    public void Reset()
    {
        G.WasTheLevelRestarted = true;

        G.IsCrossesEnabled = true;
        G.IsProgressPaused = false;
        G.CrossSpawnMultiplier = 1;
        EmitSignal(nameof(LevelReset));

        G.ResetValues();

        if (G.IsLevelVanilla)
        {
            UnchangableMeta.SaveRecords();
            LoadScene("res://Content/Scenes/Levels/FullParts/Level" + G.CurrentLevel + G.LevelAdditionalLink + ".tscn");
        }
        else
        {
            LoadScene(G.ModMapPath);
        }
    }
    public void SaveTheGame()
    {
        UnchangableMeta.SaveToFile();
    }

	public void TeleportPlayerTo(Vector2 globalPos, bool disableGlitchEffect = false)
    {
        // Teleport visual effect

		if (!disableGlitchEffect)
        	_ = G.AdditionalGuiLayer.Glitch();

		Vector2 Offset = globalPos - G.Player.GlobalPosition;
        G.Player.GlobalPosition = globalPos;

		G.Player.Camera.SmoothedPosition += Offset;
    }
    public void TeleportPlayerTo(Node2D node, Vector2 pos)
    {
		TeleportPlayerTo(node.GlobalPosition + pos);
    }
	public void TeleportPlayerTo(Node2D node, Vector2 pos, bool disableGlitchEffect = false)
    {
		TeleportPlayerTo(node.GlobalPosition + pos, disableGlitchEffect);
    }
	public void TeleportPlayerTo(Vector2 globalPos)
	{
		TeleportPlayerTo(globalPos);
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

    public void DebugTransitiveObject(int index)
    {
        GD.Print(G.TransitiveObject[index]);
    }
    public void SetTransitiveObject(int index, Variant value)
    {
        G.TransitiveObject[index] = value;
    }
    public void ResetTransitiveObject(int index)
    {
        G.TransitiveObject[index] = "";
    }


    public void DebugTransitiveDValue(string key)
    {
        GD.Print(G.TransitiveVariantD[key]);
    }
    public void SetTransitiveDValue(string key, Variant value)
    {
        G.TransitiveVariantD[key] = value;
    }
    public void ResetTransitiveDValue(string key)
    {
        G.TransitiveVariantD[key] = "";
    }

    public void ResetAllTransitiveValues()
    {
        for (int i = 0; i < G.TransitiveVariant.Length; i++)
            G.TransitiveVariant[i] = "";

        for (int i = 0; i < G.TransitiveObject.Length; i++)
            G.TransitiveObject[i] = null;

        foreach (string v in G.TransitiveVariantD.Keys)
            G.TransitiveVariantD.Remove(v);
    }





    public void ResetAllTransitiveObject()
    {
        for (int i = 0; i < G.TransitiveObject.Length; i++)
            G.TransitiveObject[i] = "";
    }
}