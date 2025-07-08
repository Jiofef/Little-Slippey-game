using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class SaveBible
{
	public Dictionary<string, object> SaveModel;
	public Dictionary<string, object> SettingsModel;

	public Dictionary<string, bool> AchievementsModel;
	public Dictionary<ContentTypeEnum, Dictionary<string, IContent>> ContentModel;
}