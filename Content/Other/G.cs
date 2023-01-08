using Godot;
using System;

public class G : Node
{
    // G is gameplay singleton, that having importal information which may be needed in various places of the game. They will not save by exiting the game
    public static bool PlayerDead;
    public static float PlayerDeathTimer, PlayerDeathTimerCoeff = 0, PlayerMoveCoeff = 1, ReversedPlayerDeathTimerCoeff = 1, ResetTimer, AfterPlayerDeadTimer, Scores = 0;
    public static int CurrentLevel;
    public static readonly int LevelsInGameTotal = 1, CrossesInGameTotal = 3, DificultiesInGameTotal = 2;

    public static readonly Vector2[] LevelXYSizes = 
    { 
        new Vector2(456, 250),
    };
    public override void _Process(float delta)
    {
        PlayerDeathTimerCoeff = PlayerDeathTimer / 4.5f;
        ReversedPlayerDeathTimerCoeff = 1 - PlayerDeathTimerCoeff;
    }
    public static void FitToDefaultValues()
    {
        PlayerDead = false;
        PlayerDeathTimer = 0;
        ResetTimer = 0;
        AfterPlayerDeadTimer = 0;
        Scores = 0;
        UnchangableMeta.SaveToFile();
    }
    public static void SaveRecords()
    {
        if (Scores > UnchangableMeta.LevelRecords[Meta.Instance.Dificulty][CurrentLevel - 1])
            UnchangableMeta.LevelRecords[Meta.Instance.Dificulty][CurrentLevel - 1] = (int)Scores;
    }
}
