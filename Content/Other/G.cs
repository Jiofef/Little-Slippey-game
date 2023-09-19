using Godot;

public partial class G : Node
{
	// G is gameplay singleton, that having importal information which may be needed in various places of the game. They will not save by exiting the game
	public static bool IsPlayerDead, IsNewRecordReached, IsProgressPaused = false, IsCrossesEnabled = true;
	public static float PlayerMoveCoeff = 1, Scores = 0, ResetTimer, PlayerCorpseFlightTimer, AfterPlayerCorpseFlightTimer, CrossSpawnMultiplier = 1;
	public static int CurrentLevel;
	public static string LevelAdditionalLink;
	public static Vector4 CameraLimits;
	public static readonly int LevelsInGameTotal = 10, CrossesInGameTotal = 5, DificultiesInGameTotal = 3;
    public static readonly int[] DefaultCrossWeight = { 100, 40, 20, 10, 30 };
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
		UnchangableMeta.SaveRecords();
		UnchangableMeta.SaveToFile();
	}
}
