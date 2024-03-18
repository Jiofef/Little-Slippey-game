using Godot;

public partial class G : Node
{
	// G is gameplay singleton, that having importal information which may be needed in various places of the game. They will not save by exiting the game
	public static bool IsSystemInitiated, IsPlayerDead, IsNewRecordReached, IsProgressPaused = false, IsCrossesEnabled = true, _isLevel10Finaling = false;
	public static float PlayerMoveCoeff = 1, Scores = 0, ResetTimer, PlayerCorpseFlightTimer, AfterPlayerCorpseFlightTimer, CrossSpawnMultiplier = 1;
	public static int CurrentLevel;
	public static string LevelAdditionalLink;
	public static Vector4 CameraLimits;
	public static readonly int LevelsInGameTotal = 10, CrossesInGameTotal = 5, DificultiesInGameTotal = 3;
    public static readonly Vector2[] LevelXYSizes =
	{
		//Level sizes starts from Vector2 with index "1", Vector2 with index "0" is the minimal level size
		new Vector2(1280, 640),
		new Vector2(1280, 640),
		new Vector2(2560, 640),
		new Vector2(2560, 1280),
		new Vector2(2560, 1280),
		new Vector2(2560, 640),
		new Vector2(2560, 1280),
		new Vector2(1280, 640),
		new Vector2(999999999, 640),
		new Vector2(2560, 1280),
		new Vector2(2560, 1280)
	};
	public static float GetPlayerCorpseFlightTimerCoeff()
	{
		return PlayerCorpseFlightTimer / 4.5f;
	}
	public static float GetReversedPlayerCorpseFlightTimerCoeff()
	{
		return 1 - GetPlayerCorpseFlightTimerCoeff();
	}
    public static void ResetValues()
	{
		IsNewRecordReached = false;
		IsPlayerDead = false;
		PlayerCorpseFlightTimer = 0;
		ResetTimer = 0;
		AfterPlayerCorpseFlightTimer = 0;
		Scores = 0;
	}
	public static void CompletelyResetValues()
	{
		ResetValues();
		LevelAdditionalLink = null;
		IsProgressPaused = false;
		CrossSpawnMultiplier = 1;
		IsCrossesEnabled = true;
		CurrentLevel = 0;
		AudioServer.SetBusEffectEnabled(2, 0, false);
		AudioServer.SetBusEffectEnabled(6, 0, false);
	}

	//Achievements segment. WARNING. BEING HERE CAN CAUSE HEAD ACHE, DIZZINESS, VOMITING, AND ALSO CAN PROVOKE AIDS AND ACUTE FORM OF PROSTATE CANCER. You have been warned.
	public static readonly int[][] LevelCompletionAchievementNumbers =
	{
		new int[] {10, 13, 16, 19, 22, 25, 28, 33, 38, 41},
        new int[] {11, 14, 17, 20, 23, 26, 29, 34, 39, 42},
        new int[] {12, 15, 18, 21, 24, 27, 30, 35, 40, 43}
    };
	public static readonly bool[] IsAchievementHiden = { false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, true, false, false, false, false, false, false, false, true, true, true, true, true, false, false};
	public static CanvasLayer CurrentPopupAchievementsLayer;
	public static int AchievementPopupTimerMultiplier = 0;
	public static void GetAchievement(int index)
	{
		if (UnchangableMeta.AchievementStatuses[index] == 1) return;
		var achievement = (Control)ResourceLoader.Load<PackedScene>("res://Content/Scenes/Achievements/Achievement" + (index + 1) + ".tscn").Instantiate();
        CurrentPopupAchievementsLayer.AddChild(achievement);
        achievement.GetNode<Timer>("PopupVersionPart/PopupTimer").Start(0.05f + 0.3f * AchievementPopupTimerMultiplier);
		achievement.FocusMode = Control.FocusModeEnum.None;
		achievement.MouseFilter = Control.MouseFilterEnum.Ignore;
        UnchangableMeta.AchievementStatuses[index] = 1;
		UnchangableMeta.SaveToFile();
        AchievementPopupTimerMultiplier++;

		if (UnchangableMeta.AchievementsCount() == (UnchangableMeta.AchievementStatuses.Length - 1))
			GetAchievement(50);
	}
}
