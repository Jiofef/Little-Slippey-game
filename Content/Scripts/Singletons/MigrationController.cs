using System;
using System.Collections.Generic;
using Godot;
using static Achievements;
using static ContentManager;
using static Additions;

public partial class MigrationController : Node
{
	public static MigrationController Inst { get; private set; }
	public MigrationController()
	{
		Inst = this;
		GameVersion = ProjectSettings.GetSetting("application/config/version").AsString();
		GD.Print(GameVersion);
	}
	public string GameVersion;
	public void MigrateIfNeeded(SaveBible saveBible)
	{
		string savedVersion = "";

		if (saveBible.SaveModel.ContainsKey("version"))
			savedVersion = saveBible.SaveModel["version"].ToString();

		switch (savedVersion)
		{
			case "":
				MigrateFromNowhere();
				break;
		}
	}

	#region All the migrations
	private void MigrateFromNowhere()
	{
		// In version 0.10.0.0 for the first time the game version was saved directly into json save file. In this version, the system of add-ons for levels has been fundamentally changed. In order for my 2 and a half player not to lose all the received add-ons, I will take care of their discovery through the appropriate achievements.
		if (HasAchievement("Level1Hard"))
		{
			UnlockContent(ContentTypeEnum.Addition, AdditionEnum.Fatty.ToString());
		}
		if (HasAchievement("Level5Hard"))
		{
			UnlockContent(ContentTypeEnum.Addition, AdditionEnum.Thunderstorm.ToString());
		}
		if (HasAchievement("Level6Hard"))
		{
			UnlockContent(ContentTypeEnum.Addition, AdditionEnum.TechAGrav.ToString());
		}
		if (HasAchievement("Level7Hard"))
		{
			UnlockContent(ContentTypeEnum.Addition, AdditionEnum.OldFilm.ToString());
		}
		if (HasAchievement("Level9Hard"))
		{
			UnlockContent(ContentTypeEnum.Addition, AdditionEnum.JiofefsHead.ToString());
		}
		
	}
	#endregion
}