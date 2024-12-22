using Godot;
using System;
using System.Linq;

/// <summary>
/// G is gameplay singleton, that having importal information which may be needed in various places of the game. They will not save after exiting the game
/// </summary>
public partial class G : Node
{
    #region NOTE: Only in-game variables. Don't touch it if you're a modder please, or i will ban your map :)
    public static bool IsSystemInitiated, IsLevelVanilla = true;
    public static int CurrentLevel;
    public static Variant InGameTransitiveValue;
    public static Godot.Collections.Array<Node> NodeCopyBuffer = new Godot.Collections.Array<Node>();
    #endregion

    ////////////////////////////

    #region These are used in game and in very rare cases can be used for modders. You can twist them any way you want, they don't break anything important, but do it wisely.
    public static float PlayerMoveCoeff = 1, // The more the player moves, the greater the coefficient from 0 to 1
        ResetTimer, // It starts after pressing the R button and goes out quickly when released. By default level restarts at 1.5 seconds of it
        PlayerCorpseFlightTimer, // Starts after player dies
        AfterPlayerCorpseFlightTimer, // Starts when player death GUI starts appearing (Scores and "Press R")
        MusicStopTimeCode = 0; // It is necessary to put the music in the same position after restarting the level

    public static readonly string[] VanillaSkinNames = ["Slippey", "Samey", "Sanboy", "Strawman", "Pineplum", "Bondey", "Sleepy", "Daley", "Hostey", "CompressMass", "JioYobaFefski", "SlippeyChad", "MISSINGNULL", "Corey"];

    public static readonly Vector2[] LevelXYSizes =
[
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
    ]; // In game levels sizes

    public static float GetPlayerCorpseFlightTimerCoeff() // The same as PlayerCorpseFlightTimer() but from 0 to 1
    {
        return PlayerCorpseFlightTimer / 4.5f;
    } 
    public static float GetReversedPlayerCorpseFlightTimerCoeff() // The same as GetPlayerCorpseFlightTimerCoeff but from 1 to 0. Reversed in short.
    {
        return 1 - GetPlayerCorpseFlightTimerCoeff();
    } 
    public static readonly int LevelsInGameTotal = 10, CrossesInGameTotal = 5, DificultiesInGameTotal = 3;

    public static string LevelAdditionalLink, MusicName = "", ModMapPath, ModMapFolder, 
                        TypeOfUsedController = "Keyboard"; // Updates every _Input. Can be "Keyboard", "XInput Gamepad" or "PS Gamepad"

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey)
        {
            TypeOfUsedController = "Keyboard";
            if (Input.IsActionJustPressed("ToggleScreenMode"))
            {
                Meta.Instance.Video.IsFullScreen = !Meta.Instance.Video.IsFullScreen;
                Meta.Instance.ApplyOptions();
                Meta.Instance.SaveToFile();
            }
        }
        else if (@event is InputEventJoypadButton || @event is InputEventJoypadMotion)
        {
            string JoyName = Input.GetJoyName(0);
            if (JoyName == "") return;
            if (JoyName[0] == 'P' && JoyName[1] == 'S')
                TypeOfUsedController = "PS Gamepad";
            else if (JoyName == "XInput Gamepad")
                TypeOfUsedController = "XInput Gamepad";
            else TypeOfUsedController = "XInput Gamepad"; //Maybe I'll add more gamepads soon
        }
    }

    /// <summary>
    /// Call this at the moment the level starts, if you've somehow seriously changed the structure of the scene or Main script.
    /// </summary>
    public static void OnLevelStartedFunc(bool wasIntroShown)
    {
        OnLevelStarted.Invoke(wasIntroShown);
    }
    #endregion

    ////////////////////////////

    #region May be used to some if statements or something, but be careful if you change it. There are other, more correct ways to change them.
    public static bool IsPlayerDead, // To change correctly, call Death() or Ressurect() in player's script
                       IsNewRecordReached; // I don't know why you even might want to change it
    public static Vector4 CameraLimits; // Better use SetCameraLimits() from Player's script
    #endregion

    ////////////////////////////

    #region May be used however you want
    public static bool IsProgressPaused = false, // Enables or disables the earning of points and increasing the difficulty of crosses
                       IsCrossesEnabled = true, // Enables or disables the spawn of crosses
                       DidLevelIntroPassed, // If the intro is missing or changed in your level, you may want to set this value yourself
                       IsDebugEnabled = true; // If enabled, Alt+Z enables immortality, Alt+X disables player's physics, Alt+C disables the crosses. Also Alt + scrolling up your mouse wheel gives you +5 scores for every "scroll step" (Alt + scrolling down does the opposite)

    public static float Scores = 0, // Speaks for itself
                       CrossSpawnMultiplier = 1, // Too
                       CrossesProgressCoeff = 1, // Default crosses evolve every 30 seconds. If this equals 2, they will do it every 15 seconds. If it's 0.5 then 60 seconds. The evolve time can also change through CrossSpawner in the editor or code
                       MusicStartPosition = 0, // When music ends, if it can restart, it starts with this position. 1 = 1 second
                       LevelCompleteTime = 150; // When this second comes, the level is passed. Can be used for different things

    public static Variant[] TransitiveVariant = new Variant[64]; // You can store almost anything here for anything. The game deletes the data only after entering the menu. If you need to save some data after restarting a level or moving to another scene, this option is perfect for you
    public static object[] TransitiveObject = new object[64]; // Addition to Variant, if some required data types are not supported
    // P.s. we HIGHLY recommend commenting out these variables in your code to avoid confusion. Especially if you use a lot of them.

    public static string GetLanguagePrefix() // For localization maybe?
    {
        if (Meta.Instance.Video.language == Meta.VideoClass.Language.en)
            return "";
        var value = Meta.Instance.Video.language.ToString();
        value = char.ToUpper(value[0]) + value.Substring(1);
        return value;
    }

    #region Frequently used nodes

    public static Player Player
    { 
        get
        {
            return player;
        } 
        set
        {
            player = value;

            if (value != null)
                OnPlayerSetted?.Invoke();
        }
    }
    public delegate void PlayerSettedEventHandler();
    public static event PlayerSettedEventHandler OnPlayerSetted = delegate { };

    public static MainScript Main
    {
        get
        {
            return main;
        }
        set
        {
            main = value;

            if (value != null)
                OnMainSetted?.Invoke();
        }
    }
    public delegate void MainSettedEventHandler();
    public static event MainSettedEventHandler OnMainSetted = delegate { };


    public static LevelMusicPlayer MusicPlayer
    {
        get
        {
            return musicPlayer;
        }
        set
        {
            musicPlayer = value;
            if (value != null)
                OnMusicPlayerSetted?.Invoke();
        }
    }
    public delegate void MusicPlayerSettedEventHandler();
    public static event MusicPlayerSettedEventHandler OnMusicPlayerSetted = delegate { };


    private static Player player;
    private static MainScript main;
    private static LevelMusicPlayer musicPlayer;
    #endregion

    #region Useful events
    /// <summary>
    /// Be careful if you use this method multiple times to the same node. AI told me there might be problems with it :P
    /// <para>includeWasIntroShown determines whether the WasIntroShown argument is bound to the call. If so, it is placed before the arguments you insert or not.</para>
    /// </summary>
    public static void BindLevelStartEventToNodeSafely(Node node, string methodName, bool includeWasIntroShown = false, params Variant[] @args)
    {
        LevelStartedEventHandler @event;
        @event = (bool wasIntroShown) =>
        {
            Variant[] callArgs = includeWasIntroShown
            ? @args.Concat(new Variant[] { wasIntroShown }).ToArray()
            : @args;

            node.Call(methodName, callArgs);
        };
        OnLevelStarted += @event;

        node.TreeExiting += () =>
        {
            if (@event != null)
            {
                OnLevelStarted -= @event;
            }
        };
    }
    public delegate void LevelStartedEventHandler(bool wasIntroShown);
    /// <summary>
    /// Use this if you want to do something at the end of the level intro, or if the intro is skipped (e.g. it was already there). 
    /// 
    /// <para>wasIntroShown shows if the intro was shown this time. You can use this as a marker if the level was run for the first time (true if yes, false if not)</para>
    /// <para>Instead of using this event directly, you can use OnLevelStarted signal from "Main" or BindLevelStartEventToNodeSafely method for a simpler structure. </para>
    /// <para>___</para>
    /// <para>If you use it directly, here is one possible implementation to avoid NullReferenceException:</para>
    /// <para>
    /// <br>    private G.LevelStartedEventHandler _onLevelStartedHandler;</br>
    /// <br>    public override void _Ready()</br>
    /// <br>    {</br>
    /// <br>        _onLevelStartedHandler = (bool wasIntroShown) => OnLevelStarted();</br>
    /// <br>        G.OnLevelStarted += _onLevelStartedHandler;</br>
    /// <br>    }</br>
    /// <br>    private void OnLevelStarted()</br>
    /// <br>    }</br>
    /// <br>//your code</br>
    /// <br>    }</br>
    /// <br>    public override void _ExitTree()</br>
    /// <br>    {</br>
    /// <br>        if (_onLevelStartedHandler != null)</br>
    /// <br>            G.OnLevelStarted -= _onLevelStartedHandler;</br>
    /// <br>    {</br>
    /// </para>
    /// 
    /// </summary>
    public static event LevelStartedEventHandler OnLevelStarted = delegate { };

    #endregion

    #endregion

    ////////////////////////////

    // Other stuff

    public static void ResetValues() // Usually used during a level restart
    {
		IsNewRecordReached = false;
		IsPlayerDead = false;
		PlayerCorpseFlightTimer = 0;
		ResetTimer = 0;
		AfterPlayerCorpseFlightTimer = 0;
		Scores = 0;
    }
	public static void CompletelyResetValues() // Usually used during the exit from the level
    {
		ResetValues();
		LevelAdditionalLink = null;
		IsProgressPaused = false;
		CrossSpawnMultiplier = 1;
		IsCrossesEnabled = true;
		CrossesProgressCoeff = 1;
		CurrentLevel = 0;
        DidLevelIntroPassed = false;
        MusicStartPosition = 0;
        MusicStopTimeCode = 0;
        MusicName = "";
		LevelCompleteTime = 150;
        AudioServer.SetBusEffectEnabled(2, 0, false);
		AudioServer.SetBusEffectEnabled(6, 0, false);

        for (int i = 0; i < G.TransitiveVariant.Length; i++)
            G.TransitiveVariant[i] = "";

        IsLevelVanilla = true;
	}


    public class CrossSpawner  // Base for spawning crosses on custom levels. Use in scripts as you like. It is located at the end of the script just to avoid polluting its important parts, as the class is large
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
            TimeCodes = [0, 30, 60, 90, 120, 150];
            SpawnWeights = [600, 170, 80, 40, 110];
            ScenesPathes = ["res://Content/Scenes/Crosses/Cross1.tscn", "res://Content/Scenes/Crosses/Cross2.tscn", "res://Content/Scenes/Crosses/Cross3.tscn", "res://Content/Scenes/Crosses/Cross4.tscn", "res://Content/Scenes/Crosses/Cross5.tscn"];
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
        private float[] _crossWeight = new float[CrossesInGameTotal];
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