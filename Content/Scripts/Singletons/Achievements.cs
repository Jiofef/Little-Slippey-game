using Godot;
using GodotSteam;
using System;
using System.Collections.Generic;
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
        public Data ()
        {
            IsHidden = false;
        }
        public Data(int goldenCrossesRewardAmount)
        {
            RewardAmount = goldenCrossesRewardAmount;
        }
        public Data(int goldenCrossesRewardAmount, bool isHidden)
        {
            RewardAmount = goldenCrossesRewardAmount;
            IsHidden = isHidden;
        }
        public bool IsHidden { get; set; }
        public bool IsReceived { get; set; } = false;
        public int RewardAmount { get; set; } = 0;

    }


    public static Dictionary<string, Data> AllTheAchievements = new Dictionary<string, Data>()
    {
        {"000-", new Data()}, //0
        {"-000-", new Data()}, //1
        {"_000", new Data()}, //2
        {"I am already the Slippey", new Data()}, //3
        {"It's worth a shot", new Data()}, //4
        {"I'm no stranger to", new Data()}, //5
        {"Over and over and over and over and over and", new Data()}, //6
        {"You're getting somewhere", new Data()}, //7
        {"I Have No Eyes, and I Must Oversee", new Data()}, //8
        {"At least I got to hobnob with royalty", new Data()}, //9
        {"Level1Hard", new Data()}, //10
        {"Level1Insane", new Data()}, //11
        {"Level1Inferno", new Data()}, //12
        {"Level2Hard", new Data()}, //13
        {"Level2Insane", new Data()}, //14
        {"Level2Inferno", new Data()}, //15
        {"Level3Hard", new Data()}, //16
        {"Level3Insane", new Data()}, //17
        {"Level3Inferno", new Data()}, //18
        {"Level4Hard", new Data()}, //19
        {"Level4Insane", new Data()}, //20
        {"Level4Inferno", new Data()}, //21
        {"Level5Hard", new Data()}, //22
        {"Level5Insane", new Data()}, //23
        {"Level5Inferno", new Data()}, //24
        {"Level6Hard", new Data()}, //25
        {"Level6Insane", new Data()}, //26
        {"Level6Inferno", new Data()}, //27
        {"Level7Hard", new Data()}, //28
        {"Level7Insane", new Data()}, //29
        {"Level7Inferno", new Data()}, //30
        {"Eureka", new Data()}, //31
        {"Marathoner", new Data()}, //32
        {"Level8Hard", new Data()}, //33
        {"Level8Insane", new Data()}, //34
        {"Level8Inferno", new Data()}, //35
        {"Laziness is our everything", new Data()}, //36
        {"Did you really fall for that", new Data(true) }, //37
        {"Level9Hard", new Data()}, //38
        {"Level9Insane", new Data()}, //39
        {"Level9Inferno", new Data()}, //40
        {"Level10Hard", new Data()}, //41
        {"Level10Insane", new Data()}, //42
        {"Level10Inferno", new Data()}, //43
        {"Congratulations!0", new Data(true)}, //44
        {"Congratulations!1", new Data(true)}, //45
        {"Congratulations!2", new Data(true)}, //46
        {"Congratulations!3", new Data(true)}, //47
        {"you were deceived.", new Data(true)}, //48
        {"A criminal against humanity", new Data(true)}, //49
        {"You will regret it.", new Data()}, // 50
        {"Thank you for everything, player", new Data()}, //51
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
    public static int AchievementPopupTimerMultiplier = 0;
    public static void GetAchievement(string name) // Do not ruin someone else's experience and do not give away game achievements for nothing. If you are making a cheat map or mod, mark it in the title/preview
    {
        Steam.SetAchievement(name);
        if (AllTheAchievements[name].IsReceived) return;
        var achievement = (Achievement)ResourceLoader.Load<PackedScene>("res://Content/Scenes/Achievements/" + name + ".tscn").Instantiate();
        G.AdditionalGuiLayer.AddChild(achievement);

        achievement.IsPopupVersion = true;

        achievement.GetNode<Timer>("PopupVersionPart/PopupTimer").Start(0.05f + 0.3f * AchievementPopupTimerMultiplier);

        achievement.FocusMode = Control.FocusModeEnum.None;
        achievement.MouseFilter = Control.MouseFilterEnum.Ignore;

        AllTheAchievements[name].IsReceived = true;
        SaveAchievementStatuses();
        AchievementPopupTimerMultiplier++;

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
            var model = FileSystemExtension.GetSystemJsonModel("user://achievements.json");

            foreach(var achievement in model)
            {
                AllTheAchievements[achievement.Key].IsReceived = (bool)achievement.Value;
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
