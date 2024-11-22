using Godot;
using System;
using System.Linq;

public partial class G : Node
{
	// G is gameplay singleton, that having importal information which may be needed in various places of the game. They will not save after exiting the game
	public static bool IsSystemInitiated, IsPlayerDead, IsNewRecordReached, IsProgressPaused = false, IsCrossesEnabled = true, DidLevelIntroPassed, IsLevelVanilla = true, IsDebugEnabled = true;
	public static float PlayerMoveCoeff = 1, Scores = 0, ResetTimer, PlayerCorpseFlightTimer, AfterPlayerCorpseFlightTimer, CrossSpawnMultiplier = 1, CrossesProgressCoeff = 1, MusicStopTimeCode = 0, MusicRestartPosition = 0, LevelCompleteTime = 150;
	public static int CurrentLevel;
	public static string LevelAdditionalLink, MusicName = "", ModMapPath, ModMapFolder;
	public static Vector4 CameraLimits;
	public static readonly int LevelsInGameTotal = 10, CrossesInGameTotal = 5, DificultiesInGameTotal = 3;
	public static string GetLanguagePrefix()
	{
		if (Meta.Instance.language == Meta.Language.en)
			return "";
        var value = Meta.Instance.language.ToString();
        value = char.ToUpper(value[0]) + value.Substring(1);
        return value;
    }
	public static string TypeOfUsedController = "Keyboard";
    // These variables are designed to expand the capabilities in level scripting, including for modders. It is primarily created to store data remaining after restarting a level, or after changing the scene.
	public static Variant[] TransitiveVariant = new Variant[64];
    public static readonly Vector2[] LevelXYSizes =
	{
		//Level sizes starts from Vector2 with index "1", Vector2 with index "0" is the minimal level size
		new Vector2(1280, 640),
		new Vector2(1280, 640),
		new Vector2(2560, 640),
		new Vector2(2560, 1280),
		new Vector2(2560, 1280),
		new Vector2(2560, 640),
		new Vector2(2560, 1280),
		new Vector2(1280, 640),
		new Vector2(999999999, 640),
		new Vector2(2560, 1280),
		new Vector2(2560, 1280)
	};
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey)
        {
			TypeOfUsedController = "Keyboard";
			if (Input.IsActionJustPressed("ToggleScreenMode"))
			{
				Meta.Instance.IsFullScreen = !Meta.Instance.IsFullScreen;
				Meta.Instance.ApplyOptions();
				Meta.Instance.SaveToFile();
			}
        }
        else if (@event is InputEventJoypadButton || @event is InputEventJoypadMotion)
        {
			string JoyName = Input.GetJoyName(0);
			GD.Print(JoyName);
			if (JoyName == "") return;
			if (JoyName[0] == 'P' && JoyName[1] == 'S')
				TypeOfUsedController = "PS Gamepad";
			else if (JoyName == "XInput Gamepad")
				TypeOfUsedController = "XInput Gamepad";
			else TypeOfUsedController = "XInput Gamepad"; //Maybe I'll add more gamepads soon
        }
    }
    public static float GetPlayerCorpseFlightTimerCoeff()
	{
		return PlayerCorpseFlightTimer / 4.5f;
	}
	public static float GetReversedPlayerCorpseFlightTimerCoeff()
	{
		return 1 - GetPlayerCorpseFlightTimerCoeff();
	}
    public static void ResetValues()
	{
		IsNewRecordReached = false;
		IsPlayerDead = false;
		PlayerCorpseFlightTimer = 0;
		ResetTimer = 0;
		AfterPlayerCorpseFlightTimer = 0;
		Scores = 0;
    }
	public static void CompletelyResetValues()
	{
		ResetValues();
		LevelAdditionalLink = null;
		IsProgressPaused = false;
		CrossSpawnMultiplier = 1;
		IsCrossesEnabled = true;
		CrossesProgressCoeff = 1;
		CurrentLevel = 0;
        DidLevelIntroPassed = false;
        MusicRestartPosition = 0;
        MusicStopTimeCode = 0;
        MusicName = "";
		LevelCompleteTime = 150;
        AudioServer.SetBusEffectEnabled(2, 0, false);
		AudioServer.SetBusEffectEnabled(6, 0, false);
	}

	//Achievements segment. WARNING. BEING HERE CAN CAUSE HEAD ACHE, DIZZINESS, VOMITING, AND ALSO CAN PROVOKE AIDS AND ACUTE FORM OF PROSTATE CANCER. You have been warned.
	public static readonly int[][] LevelCompletionAchievementNumbers =
	{
		new int[] {10, 13, 16, 19, 22, 25, 28, 33, 38, 41},
        new int[] {11, 14, 17, 20, 23, 26, 29, 34, 39, 42},
        new int[] {12, 15, 18, 21, 24, 27, 30, 35, 40, 43}
    };
	public static readonly bool[] IsAchievementHiden = { false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, true, false, false, false, false, false, false, false, true, true, true, true, true, true, false, false};
	public static CanvasLayer CurrentPopupAchievementsLayer;
	public static int AchievementPopupTimerMultiplier = 0;
	public static void GetAchievement(int index)
	{
		if (UnchangableMeta.AchievementStatuses[index] == 1) return;
		var achievement = (Control)ResourceLoader.Load<PackedScene>("res://Content/Scenes/Achievements/Achievement" + (index + 1) + ".tscn").Instantiate();
        CurrentPopupAchievementsLayer.AddChild(achievement);
        achievement.GetNode<Timer>("PopupVersionPart/PopupTimer").Start(0.05f + 0.3f * AchievementPopupTimerMultiplier);
		achievement.FocusMode = Control.FocusModeEnum.None;
		achievement.MouseFilter = Control.MouseFilterEnum.Ignore;
        UnchangableMeta.AchievementStatuses[index] = 1;
		UnchangableMeta.SaveToFile();
        AchievementPopupTimerMultiplier++;

		if (UnchangableMeta.AchievementsCount() == (UnchangableMeta.AchievementStatuses.Length - 1))
			GetAchievement(51);
	}

    // This variable is used when switching between some scenes in the game menu. Don't touch it if you don't want to break anything.
    public static Variant InGameTransitiveValue;
    // This array used in level editor. Don't touch it either
    public static Godot.Collections.Array<Node> NodeCopyBuffer = new Godot.Collections.Array<Node>();

	public class CrossSpawner
	{
		public float[] TimeCodes;
		public float[] SpawnWeights;

		private string[] _scenesPathes;
		public string[] ScenesPathes
		{
			get { return _scenesPathes; }

			set {
                _scenesPathes = value;
				_scenes = new PackedScene[ScenesPathes.Length];
				 for (int i = 0; i < _scenes.Length; i++)
				 {
					 _scenes[i] = GD.Load<PackedScene>(ScenesPathes[i]);
				 }
			}
		}
        private PackedScene[] _scenes = { };
        public int TypesCount => _scenes.Length;

        public CrossSpawner()
        {
            TimeCodes = new float[] { 0, 30, 60, 90, 120, 150 };
            SpawnWeights = new float[] { 600, 170, 80, 40, 110 };
            ScenesPathes = new string[] { "res://Content/Scenes/Crosses/Cross1.tscn", "res://Content/Scenes/Crosses/Cross2.tscn", "res://Content/Scenes/Crosses/Cross3.tscn", "res://Content/Scenes/Crosses/Cross4.tscn", "res://Content/Scenes/Crosses/Cross5.tscn" };
        }
		public CrossSpawner(float[] timeCodes, float[] spawnWeights, string[] scenesPathes)
		{
			TimeCodes = timeCodes;
			SpawnWeights = spawnWeights;
			ScenesPathes = scenesPathes;
		}


		public Node GetCross(int index)
		{
			return _scenes[index].Instantiate();
		}


		Random _random = new Random();
        private int[] _defaultSpawnWeight = { 600, 170, 80, 40, 110 };
        private float[] _crossWeight = new float[G.CrossesInGameTotal];
        private int _lastAviableCrossNumber = 0;
        private float _lastCheckedScoresValue = 0;

        private bool _didAllCrossWeigthsSetted;
        public Node GetRandomCross()
		{
            if (!_didAllCrossWeigthsSetted)
            {
                if (_crossWeight[_lastAviableCrossNumber] + Scores - _lastCheckedScoresValue < _defaultSpawnWeight[_lastAviableCrossNumber])
                {
                    _crossWeight[_lastAviableCrossNumber] += Scores - _lastCheckedScoresValue;
                    _lastCheckedScoresValue = Scores;
                }
                else
                {
                    _crossWeight[_lastAviableCrossNumber] = _defaultSpawnWeight[_lastAviableCrossNumber];
                    _lastAviableCrossNumber++;
                    _lastCheckedScoresValue = 0;
                }
                if (_lastAviableCrossNumber >= _scenes.Length)
                {
                    _lastAviableCrossNumber = _scenes.Length - 1;
                    _didAllCrossWeigthsSetted = true;
                }
            }

            int SelectedCrossNumber;
            int RandomNumber = _random.Next((int)_crossWeight.Sum());
            for (int i = 0; ; i++)
            {
                if (RandomNumber < _defaultSpawnWeight[i])
                {
                    SelectedCrossNumber = i;
                    break;
                }
                else RandomNumber -= _defaultSpawnWeight[i];
            }
			return GetCross(SelectedCrossNumber);
        }

        public void RecalculateCrossWeight()
        {
            float WeightMultiplierExtender = Scores / 30 * CrossesProgressCoeff * _defaultSpawnWeight[_lastAviableCrossNumber];
            _crossWeight = new float[CrossesInGameTotal];
            _lastAviableCrossNumber = 0;
            for (int i = 0; WeightMultiplierExtender > 0; i++)
            {
                if (i >= 5)
                {
                    _didAllCrossWeigthsSetted = true;
                    _lastAviableCrossNumber = 4;
                    break;
                }
                if (_crossWeight[_lastAviableCrossNumber] + WeightMultiplierExtender * _defaultSpawnWeight[_lastAviableCrossNumber] < _defaultSpawnWeight[_lastAviableCrossNumber])
                {
                    _crossWeight[_lastAviableCrossNumber] += WeightMultiplierExtender * _defaultSpawnWeight[_lastAviableCrossNumber];
                    WeightMultiplierExtender = 0;
                }
                else
                {
                    _crossWeight[_lastAviableCrossNumber] = _defaultSpawnWeight[_lastAviableCrossNumber];
                    _lastAviableCrossNumber++;
                    WeightMultiplierExtender -= 1;
                }
            }
        }

    }
}