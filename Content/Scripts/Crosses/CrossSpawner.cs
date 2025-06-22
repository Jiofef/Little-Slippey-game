using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
using static Crosses;

public partial class CrossSpawner : Node2D
{
    #region Export

    // These exports affect global state on start
    [Export] private bool _crossesSpawnEnabled = true;
    [Export] private float _crossesAmountMultiplier = 1f;
    [Export] private float _crossesProgressMultiplier = 1f;

    public bool CrossesSpawnEnabled { get => G.IsCrossesEnabled; set => G.IsCrossesEnabled = value; }
    public float CrossesAmountMultiplier { get => G.CrossSpawnMultiplier; set => G.CrossSpawnMultiplier = value; }
    public float CrossesProgressMultiplier { get => G.CrossesProgressCoeff; set => G.CrossesProgressCoeff = value; }

    #endregion

    Random _random = new Random();
    public Dictionary<string, object> EverythingImportant = new();

    public override void _Ready()
    {
        // Apply local exported values to global state
		if (_crossesSpawnEnabled != true)
        	CrossesSpawnEnabled = _crossesSpawnEnabled;
		if (_crossesAmountMultiplier != 1f)
        	CrossesAmountMultiplier = _crossesAmountMultiplier;
		if (_crossesProgressMultiplier != 1f)
        	CrossesProgressMultiplier = _crossesProgressMultiplier;

        if (G.Player != null)
            G.Player.PlayerResurrected += OnPlayerResurrected;
    }


	public override void _PhysicsProcess(double delta)
	{
        if (G.IsCrossesEnabled)
        {
            int RandomRange = 20 - Meta.Instance.Gameplay.Difficulty * 5;
            RandomRange = (int)((RandomRange - (RandomRange / 2 - G.PlayerMoveCoeff * RandomRange / 2)) / G.CrossSpawnMultiplier);

            if (_random.Next(RandomRange) == 0)
            {
                SpawnRandomCrossIn(this);
            }
        }
    }

    // Removing crosses after resurrection so that there is no instant death
    public void OnPlayerResurrected()
    {
        RemoveAllSpawnedCrosses();
    }
}
