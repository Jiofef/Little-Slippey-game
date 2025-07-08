using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Godot;
using static OtherExtension.FastInstanceCreator;
using static OtherExtension.ActionTools;
using static Additions;

public partial class ContentManager : Node
{
	public static ContentManager Inst { get; private set; }
	[Signal] public delegate void AdditionsGameplayEffectsUpdatedEventHandler();
	[Signal] public delegate void SavingEventHandler();
	[Signal] public delegate void SaveQueuedEventHandler();
	public const string SAVE_DATA_PATH = "user://content_data.json";
	public static Dictionary<ContentTypeEnum, Dictionary<string, IContent>> ContentDic = new Dictionary<ContentTypeEnum, Dictionary<string, IContent>>{};
	public override void _Ready()
	{
		Inst = this;
		Additions.Init();
	}

	
	public static void UnlockContent(ContentTypeEnum contentType, string contentName)
	{
		var content = ContentDic[contentType][contentName];

		if (content is IUnlockableContent uContent)
		{
			if (uContent.IsUnlocked) return;

			uContent.Unlock();

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
		Inst.EmitSignal(nameof(Inst.SaveQueued));
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
				Inst.EmitSignal(nameof(Inst.Saving));
			}
			catch (Exception e)
			{
				GD.PrintErr("Error during saving content data: " + e.Message);
			}
		}
	}
	public static void LoadData(SaveBible saveBible = null)
	{
		try
		{
			var model = FileSystemExtension.GetSystemJsonModel<ContentTypeEnum, Dictionary<string, IContent>>(SAVE_DATA_PATH);

			if (saveBible != null) saveBible.ContentModel = model;

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
					if (content is IPackedSceneHolder)
					{
						((IPackedSceneHolder)content)._PackedScene = ((IPackedSceneHolder)defaultContent)._PackedScene;
					}
					if (content is ChallengeAddition)
					{
						((ChallengeAddition)content).RewardsMultiplier = ((ChallengeAddition)defaultContent).RewardsMultiplier;
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

[JsonDerivedType(typeof(Addition), "addition")]
[JsonDerivedType(typeof(ChallengeAddition), "challenge_addition")]
 public interface IContent { }
public interface IUnlockableContent : IContent
{
	public bool IsUnlocked { get; set; }
	public void Unlock()
	{
		if (IsUnlocked) return;

		IsUnlocked = true;
	}
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

