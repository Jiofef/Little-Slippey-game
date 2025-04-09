using Godot;
using GodotSteam;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;

public partial class Achievements : Node
{
    //Achievements singleton. WARNING. BEING HERE CAN CAUSE HEAD ACHE, DIZZINESS, VOMITING, AND ALSO CAN PROVOKE AIDS AND ACUTE FORM OF PROSTATE CANCER. You have been warned.

    [Serializable]
    public class Data
    {
        public Data(bool isHidden) 
        {
            IsHidden = isHidden;
        }
        public Data()
        {
            IsHidden = false;
        }
        public Data(int goldenCrossesRewardAmount)
        {
            RewardAmount = goldenCrossesRewardAmount;
            IsHidden = false;
        }
        public Data(int goldenCrossesRewardAmount, bool isHidden)
        {
            RewardAmount = goldenCrossesRewardAmount;
            IsHidden = isHidden;
        }
        public Data(int goldenCrossesRewardAmount, string notificationKey, int notificationAmount = 1, bool isHidden = false)
        {
            RewardAmount = goldenCrossesRewardAmount;
            NotificationKey = notificationKey;
            NotificationAmount = notificationAmount;
            IsHidden = isHidden;
        }
        public Data(string notificationKey, int notificationAmount = 1, bool isHidden = false)
        {
            NotificationKey = notificationKey;
            NotificationAmount = notificationAmount;
            IsHidden = false;
        }
        public bool IsHidden { get; set; }
        public bool IsReceived { get; set; } = false;
        public int RewardAmount { get; set; } = 0;

        // If isn't null, adds NotificationAmount to the corresponding notifications in UnchangableMeta
        public string NotificationKey;
        public int NotificationAmount;
    }


    public static Dictionary<string, Data> AllTheAchievements = new Dictionary<string, Data>()
    {
        {"000-", new Data(20)}, //0
        {"-000-", new Data(20)}, //1
        {"_000", new Data(20)}, //2
        {"I am already the Slippey", new Data(15, "SkinsMenu")}, //3
        {"It's worth a shot", new Data(10)}, //4
        {"I'm no stranger to", new Data(10)}, //5
        {"Over and over and over and over and over and", new Data(50)}, //6
        {"You're getting somewhere", new Data(25)}, //7
        {"I Have No Eyes, and I Must Oversee", new Data(75)}, //8
        {"At least I got to hobnob with royalty", new Data(2)}, //9
        {"Level1Hard", new Data(50, "SkinsMenu")}, //10
        {"Level1Insane", new Data(50)}, //11
        {"Level1Inferno", new Data(50)}, //12
        {"Level2Hard", new Data(50, "SkinsMenu")}, //13
        {"Level2Insane", new Data(50)}, //14
        {"Level2Inferno", new Data(50)}, //15
        {"Level3Hard", new Data(50, "SkinsMenu")}, //16
        {"Level3Insane", new Data(50)}, //17
        {"Level3Inferno", new Data(50)}, //18
        {"Level4Hard", new Data(50, "SkinsMenu")}, //19
        {"Level4Insane", new Data(50)}, //20
        {"Level4Inferno", new Data(50)}, //21
        {"Level5Hard", new Data(50, "SkinsMenu")}, //22
        {"Level5Insane", new Data(50)}, //23
        {"Level5Inferno", new Data(50)}, //24
        {"Level6Hard", new Data(50, "SkinsMenu")}, //25
        {"Level6Insane", new Data(50)}, //26
        {"Level6Inferno", new Data(50)}, //27
        {"Level7Hard", new Data(50, "SkinsMenu")}, //28
        {"Level7Insane", new Data(50)}, //29
        {"Level7Inferno", new Data(50)}, //30
        {"Eureka", new Data(50)}, //31
        {"Marathoner", new Data(50)}, //32
        {"Level8Hard", new Data(50, "SkinsMenu")}, //33
        {"Level8Insane", new Data(50)}, //34
        {"Level8Inferno", new Data(50)}, //35
        {"Laziness is our everything", new Data(30)}, //36
        {"Did you really fall for that", new Data(true) }, //37
        {"Level9Hard", new Data(50, "SkinsMenu")}, //38
        {"Level9Insane", new Data(50)}, //39
        {"Level9Inferno", new Data(50)}, //40
        {"Level10Hard", new Data(50, "SkinsMenu")}, //41
        {"Level10Insane", new Data(50)}, //42
        {"Level10Inferno", new Data(50)}, //43
        {"Congratulations!0", new Data(25,true)}, //44
        {"Congratulations!1", new Data(50, true)}, //45
        {"Congratulations!2", new Data(100, true)}, //46
        {"Congratulations!3", new Data(200, true)}, //47
        {"you were deceived.", new Data(true)}, //48
        {"A criminal against humanity", new Data(30, true)}, //49
        {"You will regret it.", new Data("RecycleBin")}, // 50
        {"Thank you for everything, player", new Data(999, "SkinsMenu")}, //51
    };


    public static int AchievementsCount()
    {
        int AchievementsCount = 0;

        foreach (var achievement in AllTheAchievements)
            if (achievement.Value.IsReceived)
                AchievementsCount++;

        return AchievementsCount;
    }

    public static void GetLevelAchievements()
    {
        if (G.CurrentLevel != 0)
        {
            for (int i = 0; i <= Meta.Instance.Gameplay.Dificulty; i ++)
            {
                GetAchievement("Level" + G.CurrentLevel + Meta.Instance.Gameplay.DificultyNames[i]);
            }
        }
    }
    public static void UpdateLevelAchievements(int levelId)
    {
        int levelStatus = UnchangableMeta.LevelCompleteStatus[levelId];
        
        for (int i = 0; i <= G.DificultiesInGameTotal; i++)
        {
            if (i <  levelStatus)
            {
                GetAchievement("Level" + levelId + Meta.Instance.Gameplay.DificultyNames[i]);
            }
            else
            {
                RemoveAchievement("Level" + levelId + Meta.Instance.Gameplay.DificultyNames[i]);
            }
        }
    }
    public static int AchievementPopupTimerMultiplier = 0;
    public static void GetAchievement(string name) // Do not ruin someone else's experience and do not give away game achievements for nothing. If you are making a cheat map or mod, mark it in the title/preview
    {
        Steam.SetAchievement(name);

        Data achievement = AllTheAchievements[name];
        if (achievement.IsReceived) return;

        // Spawning achievement
        var achievementNode = (Achievement)ResourceLoader.Load<PackedScene>("res://Content/Scenes/Achievements/" + name + ".tscn").Instantiate();
        G.AdditionalGuiLayer.AddChild(achievementNode);

        achievementNode.IsPopupVersion = true;

        achievementNode.GetNode<Timer>("PopupVersionPart/PopupTimer").Start(0.05f + 0.3f * AchievementPopupTimerMultiplier);

        achievementNode.FocusMode = Control.FocusModeEnum.None;
        achievementNode.MouseFilter = Control.MouseFilterEnum.Ignore;

        achievement.IsReceived = true;
        SaveAchievementStatuses();
        AchievementPopupTimerMultiplier++;

        // Giving the reward
        UnchangableMeta.AddGoldenCrosses(achievement.RewardAmount);

        // Notificationing if necessary
        if (achievement.NotificationKey != null)
            UnchangableMeta.NotificationsAmount[achievement.NotificationKey] += achievement.NotificationAmount;

        // Final achievement check
        if (AchievementsCount() == (AllTheAchievements.Count - 1))
            GetAchievement("Thank you for everything, player");
    } 
    public async static void GetAchievementAfter(float timer, string name)
    {
        await Task.Delay((int)(timer * 1000));
        GetAchievement(name);
    }
    public async static void GetAchievementAfter(Task task, string name)
    {
        await task;
        GetAchievement(name);
    }

    public static void RemoveAchievement(string name)
    {
        Data achievement = AllTheAchievements[name];

        achievement.IsReceived = false;
        UnchangableMeta.GoldenCrossesAmount -= achievement.RewardAmount;
        
        if (achievement.NotificationKey != null && UnchangableMeta.NotificationsAmount[achievement.NotificationKey] > 0)
            UnchangableMeta.NotificationsAmount[achievement.NotificationKey]--;
    }

    public static void SaveAchievementStatuses()
    {
        try
        {
            var SaveData = JsonSerializer.Serialize(GetJSON());
            FileSystemExtension.SaveInJson(GetJSON(), "user://achievements.json");
        }
        catch { }
    }
    public static void LoadAchievementStatuses()
    {
        try
        {
            var model = FileSystemExtension.GetSystemJsonModel<bool>("user://achievements.json");

            if (model == null) return;

            foreach (var achievement in model)
            {
                if (AllTheAchievements.ContainsKey(achievement.Key))
                    AllTheAchievements[achievement.Key].IsReceived = achievement.Value;
            }
        }
        catch { }
    }

    public static Dictionary<string, bool> GetJSON()
    {
        Dictionary<string, bool> dic = new();

        foreach(var achievement in AllTheAchievements)
        {
            dic.Add(achievement.Key, achievement.Value.IsReceived);
        }
        return dic;
    }
}
