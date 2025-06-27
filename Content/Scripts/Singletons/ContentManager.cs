using System;
using System.Collections.Generic;
using Godot;

public partial class ContentManager : Node
{
	public static Dictionary<ContentTypeEnum, Dictionary<string, IContent>> ContentDic = new Dictionary<ContentTypeEnum, Dictionary<string, IContent>>
	{
		{
			ContentTypeEnum.Addition,

			new Dictionary<string, IContent> {
			{ AdditionEnum.Thunderstorm.ToString(),
			new Addition(1)},
			{AdditionEnum.OldFilm.ToString(),
			new Addition(1)},
			{AdditionEnum.JiofefsHead.ToString(),
			new Addition(2)},
			{AdditionEnum.EnhancedCrosses.ToString(),
			new Addition(1)},
			}
		},
	};
	public static bool IsAdditionActive(AdditionEnum addition)
	{
		return ((Addition)ContentDic[ContentTypeEnum.Addition][addition.ToString()]).IsActivated;
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
	public static void SaveData(bool forceSave = false)
	{
		if (_isQueuedToSave || forceSave)
		{
			_isQueuedToSave = false;
			FileSystemExtension.SaveInJson(ContentDic, "user://content_data.json");
		}
	}
	public static void LoadData()
	{
		var model = FileSystemExtension.GetSystemJsonModel<ContentTypeEnum, Dictionary<string, IContent>>("user://content_data.json");

		if (model == null) return;

		ContentDic = model;
	}
}

public enum ContentTypeEnum { Addition }
public enum AdditionEnum { Thunderstorm, OldFilm, JiofefsHead, EnhancedCrosses };
public class Addition : IPurchasableContent, IToggleableContent, IUnlockableContent
{
	public bool IsUnlocked { get; set; }
	public bool IsBought { get; set; }
	public int Cost { get; set; }
	public bool IsActivated { get; set; }
	public Addition(int cost = 0, bool isUnlocked = false, bool isBought = false, bool isActivated = false)
	{
		Cost = cost;
		IsUnlocked = isUnlocked;
		IsBought = isBought;
		IsActivated = isActivated;
	}
}

public interface IContent { }
public interface IUnlockableContent : IContent
{
	public bool IsUnlocked { get; set; }
}
public interface IPurchasableContent
{
	public bool IsBought { get; set; }
	public int Cost { get; set; }
}

public interface IToggleableContent : IContent
{
	public bool IsActivated { get; set; }
}