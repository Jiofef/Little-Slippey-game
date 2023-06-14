using Godot;

public partial class G : Node
{
	// G is gameplay singleton, that having importal information which may be needed in various places of the game. They will not save by exiting the game
	public static bool PlayerDead, DoNewRecordReached;
	public static float PlayerDeathTimer, PlayerDeathTimerCoeff = 0, PlayerMoveCoeff = 1, ReversedPlayerDeathTimerCoeff = 1, ResetTimer, AfterPlayerDeadTimer, Scores = 0;
	public static int CurrentLevel;
	public static readonly int LevelsInGameTotal = 7, CrossesInGameTotal = 5, DificultiesInGameTotal = 3;
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
    };
	public override void _Process(double delta)
	{
		PlayerDeathTimerCoeff = PlayerDeathTimer / 4.5f;
		ReversedPlayerDeathTimerCoeff = 1 - PlayerDeathTimerCoeff;
	}
	public static void ResetValues()
	{
		DoNewRecordReached = false;
		PlayerDead = false;
		PlayerDeathTimer = 0;
		ResetTimer = 0;
		AfterPlayerDeadTimer = 0;
		Scores = 0;
		SaveRecords();
		UnchangableMeta.SaveToFile();
	}
	public static void SaveRecords()
	{
		if (Scores > UnchangableMeta.LevelRecords[Meta.Instance.Dificulty][CurrentLevel - 1])
		{
            UnchangableMeta.LevelRecords[Meta.Instance.Dificulty][CurrentLevel - 1] = (int)Scores;
			DoNewRecordReached = true;
        }
	}
}
