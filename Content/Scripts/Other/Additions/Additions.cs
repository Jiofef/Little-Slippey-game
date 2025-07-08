using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Godot;
using static OtherExtension.FastInstanceCreator;
using static ContentManager;
using System.Threading.Channels;

public static class Additions
{
	static Additions()
	{
		ContentDic.Add(ContentTypeEnum.Addition, new Dictionary<string, IContent>
			{
			{ AdditionEnum.TechAGrav.ToString(),
			new Addition(GetPackedFrom(new TechAGravAddition()), AdditionType.Neutral, 40)},

			{ AdditionEnum.Fatty.ToString(),
			new Addition(GetPackedFrom(new FattyAddition()), AdditionType.Neutral, 20)},

			{ AdditionEnum.Thunderstorm.ToString(),
			new ChallengeAddition(GetPackedFrom(new ThunderStormAddition()), AdditionType.Challenge, 1)},

			{ AdditionEnum.OldFilm.ToString(),
			new ChallengeAddition(GetPackedFrom(new OldFilmAddition()), AdditionType.Challenge, 1)},

			{ AdditionEnum.JiofefsHead.ToString(),
			new ChallengeAddition(GetPackedFrom(new JiofefsHeadAddition()), AdditionType.Challenge, 2)},

			{ AdditionEnum.EnhancedCrosses.ToString(),
			new ChallengeAddition(GetPackedFrom(new EnhancedCrossesAddition()), AdditionType.Challenge, 1)},
			});

		ContentManager.Inst.SaveQueued += GameplayEffects.UpdateValues;
	}
	public static void Init() { }
	public enum AdditionEnum { Fatty, TechAGrav, Thunderstorm, OldFilm, JiofefsHead, EnhancedCrosses };

	public enum AdditionType { Neutral, Cheat, Challenge }
	[Serializable]
	public class Addition : IPurchasableContent, IToggleableContent, IUnlockableContent, ITypeHolder<AdditionType>, IPackedSceneHolder
	{
		[JsonIgnore] InGameNotifierComponent NotifierComponent = new ("Additions");
		[JsonIgnore] public AdditionType Type { get; set; }
		[JsonIgnore] public PackedScene _PackedScene { get; set; }
		public bool IsUnlocked { get; set; }
		public bool IsBought { get; set; }
		[JsonIgnore] public int Cost { get; set; }
		public bool IsActivated { get; set; }
		public Addition(PackedScene _packedScene = null, AdditionType type = AdditionType.Neutral, int cost = 0)
		{
			_PackedScene = _packedScene;
			Type = type;
			Cost = cost;
		}

		public void Unlock()
		{
			if (IsUnlocked) return;

			IsUnlocked = true;
			NotifierComponent.CallNotify();
		}
	}

	public class ChallengeAddition : Addition
	{
		[JsonIgnore] public float RewardsMultiplier { get; set; }

		public ChallengeAddition(PackedScene _packedScene = null, AdditionType type = AdditionType.Neutral, int cost = 0, float rewardsMultiplier = 1.25f) : base(_packedScene, type, cost)
		{
			RewardsMultiplier = rewardsMultiplier;
		}
	}

	public static Dictionary<string, Addition> GetAdditions(AdditionType? type = null)
	{
		var additions = ContentDic[ContentTypeEnum.Addition].ToDictionary(kpv => kpv.Key, kvp => (Addition)kvp.Value);
		if (type == null) return additions;

		var filteredDic = additions.Where(kvp => kvp.Value is ITypeHolder<AdditionType> taValue && taValue.Type == type).ToDictionary();

		return filteredDic;
	}

	public static int GetAdditionsAmount(AdditionType? type = null)
	{
		return GetAdditions(type).Count();
	}
	public static int GetAvailableAdditionsAmount(AdditionType? type = null)
	{
		var availableAdditions = GetAdditions(type).Where(kvp => kvp.Value.IsUnlocked && kvp.Value.IsBought);
		int amount = availableAdditions.Count();
		return amount;
	}

	public static bool IsAdditionActive(AdditionEnum addition)
	{
		return ((Addition)ContentDic[ContentTypeEnum.Addition][addition.ToString()]).IsActivated;
	}

	public static class GameplayEffects
	{
		public static bool AchievementsDisabled { get; private set; } = false;
		public static float GoldenCrossesMultiplier { get; private set; } = 1f;

		public static void UpdateValues()
		{
			// Achievements are disabled if activated cheats additions is more than 0
			AchievementsDisabled = GetAdditions(AdditionType.Cheat).Where(v => v.Value.IsActivated).Count() > 0;

			float newMultiplierValue = 1f;
			var activatedChallenges = GetAdditions(AdditionType.Challenge).Where(v => v.Value is ChallengeAddition && v.Value.IsActivated).ToDictionary(kvp => kvp.Key, kvp => (ChallengeAddition)kvp.Value);

			foreach (var addition in activatedChallenges.Values)
			{
				newMultiplierValue *= addition.RewardsMultiplier;
			}

			GoldenCrossesMultiplier = newMultiplierValue;

			ContentManager.Inst?.EmitSignal(nameof(ContentManager.Inst.AdditionsGameplayEffectsUpdated));
		}
	}
}