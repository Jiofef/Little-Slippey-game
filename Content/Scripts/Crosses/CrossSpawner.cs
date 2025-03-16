using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
using static Crosses;

public partial class CrossSpawner : Node2D
{
    Random _random = new Random();

    public Dictionary<string, object> EverythingImportant = new();

    public override void _Ready()
	{
        if (G.Player != null)
            G.Player.PlayerResurrected += OnPlayerResurrected;
	}


	public override void _PhysicsProcess(double delta)
	{
        if (G.IsCrossesEnabled)
        {
            int RandomRange = 20 - Meta.Instance.Gameplay.Dificulty * 5;
            RandomRange = (int)((RandomRange - (RandomRange / 2 - G.PlayerMoveCoeff * RandomRange / 2)) / G.CrossSpawnMultiplier);

            if (_random.Next(RandomRange) == 0)
            {
                CanvasItem Cross = SpawnRandomCrossIn(this);
                if (Cross is not EnhancedCannonCross and not CannonCross) Cross.QueueFree();
            }
        }
    }

    // Removing crosses after resurrection so that there is no instant death
    public void OnPlayerResurrected()
    {
        foreach (UnusualCrossNode cross in GetTree().GetNodesInGroup("Crosses"))
            cross.OnFinished();
    }
}
