using Godot;
using System;
using OtherExtension;
using static OtherExtension.RandomTools;

public partial class BonusSpawner : Node2D
{
	// Export variables
	[Export] public string[] PossibleBonusesPathes { get; private set; }
	[Export] public float SpawnPercentChange = 100;
	[ExportGroup("Respawn")]
	[Export] public bool EnableRespawn = false;
	[Export] public double RespawnTime = 30d;

	private PackedScene[] _possibleBonusesPacked;

	public Timer RespawnTimer;

	public void SetPossibleBonusesPathes(string[] pathes)
	{
		PossibleBonusesPathes = pathes;
		_possibleBonusesPacked = new PackedScene[pathes.Length];

		for (int i = 0; i < PossibleBonusesPathes.Length; i ++)
			_possibleBonusesPacked[i] = GD.Load<PackedScene>(PossibleBonusesPathes[i]);
	}

	public override void _Ready()
	{
		SetPossibleBonusesPathes(PossibleBonusesPathes);

		RespawnTimer = GetNode<Timer>("RespawnTimer");

		RespawnTimer.WaitTime = RespawnTime;
		if (EnableRespawn)
			RespawnTimer.Start();

		TrySpawn();
	}

	public bool TrySpawn()
	{
		if (!TryRand(SpawnPercentChange)) return false;

		var bonus = _possibleBonusesPacked.GetRandom().Instantiate<Bonus>();
		bonus.Collected += OnBonusCollected;

        var tween = CreateTween();
		bonus.Scale = Vector2.Zero;
        tween.TweenProperty(bonus, property: "scale", Vector2.One, 0.5).SetEase(Tween.EaseType.Out);

        AddChild(bonus);

		return true;
	}

	private void OnRespawnTimerTimeout()
	{
		if (EnableRespawn)
		{
			if (!TrySpawn())
				OnBonusCollected();
        }
	}

	public void OnBonusCollected()
	{
		if (EnableRespawn)
		{
            RespawnTimer.WaitTime = RespawnTime;
            RespawnTimer.Start();
        }
	}
}
