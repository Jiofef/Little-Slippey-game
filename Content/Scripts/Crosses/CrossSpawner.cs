using Godot;
using System;
using System.Linq;
using static Crosses;

public partial class CrossSpawner : Node2D
{
    public enum CannonsModeEnum {Horizontal, Vertical, Both}
    [Export] public CannonsModeEnum CannonsMode = CannonsModeEnum.Horizontal;

    Random _random = new Random();

    private readonly float _floatDelta = 0.016667f;

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
                Cross.AddToGroup("Crosses");
            }
        }
    }

    // Removing crosses after resurrection so that there is no instant death
    public void OnPlayerResurrected()
    {
        foreach (Node cross in GetTree().GetNodesInGroup("Crosses"))
            cross.QueueFree();
    }
}
