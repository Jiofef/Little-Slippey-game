using Godot;
using System;
using OtherExtension;

/// <summary>
/// This class is designed so that when changing some aspects of the balance you do not have to run through the whole project and change identical copy-pasted formulas.
/// </summary>
public partial class BalanceCenter : Node
{
	public static float FullCrossSpawnMultiplier => G.CrossSpawnMultiplier * Meta.Instance.Gameplay.DifficultyEffects.CrossesSpawnMultiplier;
	public static float FullGoldenCrossesMultiplier => Additions.GameplayEffects.GoldenCrossesMultiplier;
	public static int GetRandomGoldenCrossesAmount(int defaultMin, int defaultMax)
	{
		float multiplier = FullGoldenCrossesMultiplier;
		return RandomTools.RandomIn((int)(defaultMin * multiplier), (int)(defaultMax * multiplier));
	}
}
