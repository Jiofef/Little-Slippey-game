using Godot;
using System;

public class G : Node
{
    // G is gameplay singleton, that having importal information which may be needed in various places of the game. They will not save by exiting the game
    public static bool _playerdead;
    public static float _playerdeathtimer, _pdcoeff1, _movecoeffplayer = 1, _pdcoeff2 = 1, _resettimer, afterdeadtimer, _scores = 0;
    public static int _currentlvl;
    public static readonly int _levelstotal = 1, _crossestotal = 3, _dificultiescount = 2;

    public static readonly Vector2[] LevelSizes = 
    { 
        new Vector2(456, 250),
    };
    public override void _Process(float delta)
    {
        _pdcoeff1 = _playerdeathtimer / 4.5f;
        _pdcoeff2 = 1 - _pdcoeff1;
    }
    public static void FitToDefaultValues()
    {
        _playerdead = false;
        _playerdeathtimer = 0;
        _resettimer = 0;
        afterdeadtimer = 0;
        _scores = 0;
        UnchangableMeta.SaveToFile();
    }
    public static void SaveRecords()
    {
        if (_scores > UnchangableMeta._levelrecords[Meta.Instance._dificulty][_currentlvl - 1])
            UnchangableMeta._levelrecords[Meta.Instance._dificulty][_currentlvl - 1] = (int)_scores;
    }
}
