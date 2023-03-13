using Godot;

public partial class G : Node
{
	// G is gameplay singleton, that having importal information which may be needed in various places of the game. They will not save by exiting the game
	public static bool PlayerDead, DoNewRecordReached;
	public static float PlayerDeathTimer, PlayerDeathTimerCoeff = 0, PlayerMoveCoeff = 1, ReversedPlayerDeathTimerCoeff = 1, ResetTimer, AfterPlayerDeadTimer, Scores = 0;
	public static int CurrentLevel;
	public static readonly int LevelsInGameTotal = 1, CrossesInGameTotal = 5, DificultiesInGameTotal = 3;
    public static readonly int[] _defaultCrossWeight = { 100, 40, 20, 10, 30 };
    public static Vector2[] RestlessCrossPoses =
	{

	};
	public static readonly Vector2[] LevelXYSizes =
	{
		//Level sizes start from element with index "1", element with index "0" is the default level size
		new Vector2(1280, 720),
		new Vector2(1280, 720),
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
