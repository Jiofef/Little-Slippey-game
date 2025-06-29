using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Godot;

public partial class ContentManager : Node
{
	public const string SAVE_DATA_PATH = "user://content_data.json";
	public static Dictionary<ContentTypeEnum, Dictionary<string, IContent>> ContentDic = new Dictionary<ContentTypeEnum, Dictionary<string, IContent>>
	{
		{
			ContentTypeEnum.Addition,

			new Dictionary<string, IContent> {
			{ AdditionEnum.Thunderstorm.ToString(),
			new Addition(AdditionType.Challenge, 1)},
			{AdditionEnum.OldFilm.ToString(),
			new Addition(AdditionType.Challenge, 1)},
			{AdditionEnum.JiofefsHead.ToString(),
			new Addition(AdditionType.Challenge, 2)},
			{AdditionEnum.EnhancedCrosses.ToString(),
			new Addition(AdditionType.Challenge, 1)},
			}
		},
	};
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
	public static void UnlockContent(ContentTypeEnum contentType, string contentName)
	{
		var content = ContentDic[contentType][contentName];

		if (content is IUnlockableContent uContent)
		{
			if (uContent.IsUnlocked) return;
			
			uContent.IsUnlocked = true;

			QueueSave();
		}
		else
		{
			GD.PrintErr($"Trying to unlock not an unlockable content \"{contentName}\"");
		}
	}
	public static C GetContent<C>(ContentTypeEnum contentType, string contentName) where C : IContent
	{
		C content = (C)ContentDic[contentType][contentName];

		if (content == null) GD.PrintErr("There is no such a content with name " + contentName);
		return content;
	}

	// Content info save-loading

	/// <summary>
	/// Call this every time you make changes to the ContentDic that need to be saved
	/// </summary>
	private static bool _isQueuedToSave = false;
	public static void QueueSave()
	{
		_isQueuedToSave = true;
	}

	/// <summary>
	/// forceSave if active, saves the file regardless of whether QueueSave was called after last saving
	/// </summary>
	public static void SaveToFile(bool forceSave = false)
	{
		if (_isQueuedToSave || forceSave)
		{
			try
			{
				_isQueuedToSave = false;
				FileSystemExtension.SaveInJson(ContentDic, SAVE_DATA_PATH);
			}
			catch (Exception e)
			{
				GD.PrintErr("Error during saving content data: " + e.Message);
			}
		}
	}
	public static void LoadData()
	{
		try
		{
			var model = FileSystemExtension.GetSystemJsonModel<ContentTypeEnum, Dictionary<string, IContent>>(SAVE_DATA_PATH);

			if (model == null) return;

			foreach (var (contentType, contentDict) in model)
			{
				foreach (var (contentName, contentValue) in contentDict)
				{
					if (!ContentDic.ContainsKey(contentType) || !ContentDic[contentType].ContainsKey(contentName)) continue;

					var content = contentValue;
					var defaultContent = ContentDic[contentType][contentName];

					// Some values like "Cost" should not be loaded
					if (content is IPurchasableContent)
						((IPurchasableContent)content).Cost = ((IPurchasableContent)defaultContent).Cost;
					if (content.GetType().GetInterface("ITypeHolder`1") != null)
					{
						dynamic dynamicHolder = (ITypeHolder)content;
						dynamic defaultHolder = defaultContent;

						dynamicHolder.Type = defaultHolder.Type;
					}
					
					ContentDic[contentType][contentName] = content;
				}
			}
		}
		catch (Exception e)
		{
			GD.PrintErr("Error during loading content data: " + e.Message);
		}
	}
}


public enum ContentTypeEnum { Addition }
[JsonDerivedType(typeof(Addition), "addition")] public interface IContent { }
public interface IUnlockableContent : IContent
{
	public bool IsUnlocked { get; set; }
}
public interface IPurchasableContent : IContent
{
	public bool IsBought { get; set; }
	[JsonIgnore] public int Cost { get; set; }
}

public interface IToggleableContent : IContent
{
	public bool IsActivated { get; set; }
}

public enum AdditionEnum { Thunderstorm, OldFilm, JiofefsHead, EnhancedCrosses };

public enum AdditionType { Neutral, Cheat, Challenge }
[Serializable]
public class Addition : IPurchasableContent, IToggleableContent, IUnlockableContent, ITypeHolder<AdditionType>
{
	[JsonIgnore] public AdditionType Type { get; set; }
	public bool IsUnlocked { get; set; }
	public bool IsBought { get; set; }
	[JsonIgnore] public int Cost { get; set; }
	public bool IsActivated { get; set; }
	public Addition(AdditionType type = AdditionType.Neutral, int cost = 0, bool isUnlocked = false, bool isBought = false, bool isActivated = false)
	{
		Type = type;
		Cost = cost;
		IsUnlocked = isUnlocked;
		IsBought = isBought;
		IsActivated = isActivated;
	}
}