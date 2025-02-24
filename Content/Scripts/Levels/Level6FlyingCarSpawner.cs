using Godot;
using System;
using static OtherExtension.RandomTools;
using static OtherExtension.FastInstanceCreator;
using static OtherExtension.MathTools;

public partial class Level6FlyingCarSpawner : Node2D
{
	public PackedScene Car;
	public PackedScene[] UnusualCars = new PackedScene[3];
	public float[] UnusualCarWeights = [15, 15, 10];
	public const float UNUSUAL_CAR_MAX_CHANCE = 50f;

	private Random _random = new Random();
	public override void _Ready()
	{
		// Initializing the nodes
		Car = LoadPackedResScene("Other/Level6DefaultCar.tscn");
		string[] carNames = {"JumpeyCar", "WindowlickerCar", "WindowlickerJumpeyCar"};
		for(int i = 0; i < UnusualCars.Length; i ++)
			UnusualCars[i] = LoadPackedResScene("Other/Level6" + carNames[i] + ".tscn");

	}

	public const int CARS_RARITY = 8;
	enum SpawnSides {Right, Left}
	public override void _PhysicsProcess(double delta)
	{
		if (_random.Next(CARS_RARITY) == 0)
		{
			SpawnACar();
        }
	}

	public void SpawnACar()
	{


        // Setting the spawn side
        SpawnSides side = FiftyFifty() ? SpawnSides.Right : SpawnSides.Left;


        float xPos = side == SpawnSides.Left ? 640 : 3200;
        float yPos = _random.Next(100, 2560);
        float yCoeff = ClosenessTo(yPos, 2560, 2460);

        // The closer the car is to the bottom of the map, the more likely it is to be unusual
        bool isCarUnusual = TryRand(UNUSUAL_CAR_MAX_CHANCE * yCoeff);

        Path2D flyingCar = !isCarUnusual ? (Path2D)Car.Instantiate() : (Path2D)PickRandomByWeight(UnusualCars, UnusualCarWeights).Instantiate();
        flyingCar.Scale = new Vector2(side == SpawnSides.Left ? 1 : -1, 1);
        flyingCar.Position = new Vector2(xPos, yPos);

        AddChild(flyingCar);
    }
}
