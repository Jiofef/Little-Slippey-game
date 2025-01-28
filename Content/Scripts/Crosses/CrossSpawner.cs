using Godot;
using System;
using System.Linq;
using static Crosses;

public partial class CrossSpawner : Node2D
{
    public enum CannonsModeEnum {Horizontal, Vertical, Both}
    [Export] public CannonsModeEnum CannonsMode = CannonsModeEnum.Horizontal;

    Random _random = new Random();

    public Crosses.CrossSpawner Spawner = new Crosses.CrossSpawner();

    PackedScene[] _crosses = new PackedScene[G.CrossesInGameTotal];


    private int[] _crossDefaultWeight = { 600, 170, 80, 40, 110 };
    private float[] _crossWeight = new float[G.CrossesInGameTotal];
    private int _lastAviableCrossNumber = 0;
    private float _weightMultiplierExtenderToCurrentCross = 0;
    private bool _doAllCrossWeigthsSetted, _isCrossesEnhanced;
    private readonly float _floatDelta = 0.016667f;

    public override void _Ready()
	{

	}


	public override void _PhysicsProcess(double delta)
	{
        if (!G.IsProgressPaused)
            _weightMultiplierExtenderToCurrentCross += (_floatDelta * _crossDefaultWeight[_lastAviableCrossNumber]) / 30 * G.CrossesProgressCoeff;


        if (G.IsCrossesEnabled)
        {
            int RandomRange = 20 - Meta.Instance.Gameplay.Dificulty * 5;
            RandomRange = (int)((RandomRange - (RandomRange / 2 - G.PlayerMoveCoeff * RandomRange / 2)) / G.CrossSpawnMultiplier);

            if (_random.Next(RandomRange) == 0)
            {
                CanvasItem Cross = Crosses.CrossSpawner.SpawnRandomCrossIn(this);
            }
        }
    }
}
