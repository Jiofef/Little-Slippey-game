using Godot;
using Godot.Collections;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static OtherExtension.ActionTools;

/// <summary>
/// G is gameplay singleton, that having important information or methods which may be needed in various places of the game. They will not save after exiting the game
/// </summary>
public partial class G : Node
{
    private static G _instance;
    private static SceneTree _sceneTree;
    private static Node _root;

    /// <summary>
    /// G singleton instance
    /// </summary>
    public static G Inst { get => _instance; }
    public static SceneTree SceneTree { get => _sceneTree; }
    public static Node Root { get => _root; }

    public override void _Ready()
    {
        _instance = this;
        _sceneTree = _instance.GetTree();
        _root = _sceneTree.Root;
    }

    #region NOTE: Only in-game variables. Don't touch it if you're a modder please, or i will ban your mod :)
    public static bool IsSystemInitiated, IsLevelVanilla = true;
    public static int CurrentLevel; // The variable is most often used to understand whether the player is in a level or in the menu. If the player is on a level from the mod, the value is -1 
    public static Variant VanillaTransitiveValue;
    public static Array<Node> NodeCopyBuffer = new Array<Node>(); // Copied nodes from level editor
    #endregion

    ////////////////////////////

    #region These are used in game and in very rare cases can be used for modders. You can twist them any way you want, they don't break anything important, but do it wisely.
    public static float PlayerMoveCoeff = 1, // The more the player moves, the greater the coefficient from 0 to 1
        ResetTimer, // It starts after pressing the R button and goes out quickly when released. By default level restarts at 1.5 seconds of it
        PlayerCorpseFlightTimer, // Starts after player dies
        AfterPlayerCorpseFlightTimer, // Starts when player death GUI starts appearing (Scores and "Press R")
        MusicStopTimeCode = 0; // It is necessary to put the music in the same position after restarting the level

    public static int ResurrectionsInARow = 0; // To increase the price of resurrections after each resurrection
    public static byte ShowMouseDuringGameplay // Not a bool but a byte, because several objects (such as optional parts of the interface) may want the mouse to be visible at once. To avoid this problem, the mouse is only invisible when the variable is 0. To make it visible, add 1 to the variable and subtract that 1 when your object no longer needs a visible mouse.
    { get => _showMouseDuringGameplay; set { _showMouseDuringGameplay = value; UpdateMouseVisible(); } }
    private static byte _showMouseDuringGameplay = 0;
    public static void UpdateMouseVisible()
    {
        bool hideMouse = IsOnLevel && _showMouseDuringGameplay == 0;
        Input.MouseMode = hideMouse ? Input.MouseModeEnum.Hidden : Input.MouseModeEnum.Visible;
    }

    public static bool BlockSavingSomeValues = false; // Due to bugs in the engine, if you first remove a scene and then add it to the tree again, it starts to behave strangely when deleting it. One of the cases is that music player saves timecode after all main values in G are reset. This variable is designed for such moments. It is disabled in the menu. If you use it in your own way on a level, you may need to disable it yourself.

    public static readonly Vector2[] LevelXYSizes =
[
        new Vector2(1280, 640),
        new Vector2(1280, 640),
        new Vector2(2560, 640),
        new Vector2(2560, 1280),
        new Vector2(2560, 1280),
        new Vector2(2560, 640),
        new Vector2(3840, 2560),
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

    public static int GetResurrectionCost()
    {
        return MinResurrectionCost + (ResurrectionsInARow * MinResurrectionCost / 3);
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
        OnLevelStarted.Handler?.Invoke(wasIntroShown);
    }
    #endregion

    ////////////////////////////

    #region May be used to some if statements or something, but be careful if you change it. There are other, more correct ways to change them.
    public static bool IsPlayerDead, // To change correctly, call Death() or Resurrect() in player's script
                       IsNewRecordReached, // I don't know why you even might want to change it
                       HasLevelBeenCompleted;
    #endregion

    ////////////////////////////

    #region May be used however you want
    public static bool IsProgressPaused = false, // Enables or disables the earning of points and increasing the difficulty of crosses
                       IsCrossesEnabled = true, // Enables or disables the spawn of crosses
                       IsDebugEnabled = false, // If enabled, Alt+Z enables immortality, Alt+X disables player's physics, Alt+C disables the crosses. Also Alt + scrolling up your mouse wheel gives you +5 scores for every "scroll step" (Alt + scrolling down does the opposite)
                       
                       DidLevelIntroPassed, // If the intro is missing or changed in your level, you may want to set this value yourself
                       WasTheLevelRestarted; // Essentially a continuation of the previous variable. But this one obviously has the difference that it becomes true only when the level is reloaded


    public static float Scores = 0, // Speaks for itself
                       CrossSpawnMultiplier = 1, // Too
                       CrossesProgressCoeff = 1, // Default crosses evolve every 30 seconds. If this equals 2, they will do it every 15 seconds. If it's 0.5 then 60 seconds. The evolve time can also change through Crosses singleton or the editor
                       MusicStartPosition = 0, // When music ends, if it can restart, it starts with this position. 1 = 1 second
                       LevelCompleteTime = 150; // When this second comes, the level is passed. Can be used for different things

    public static bool IsOnLevel => CurrentLevel != 0;
	public static float FullCrossSpawnMultiplier => CrossSpawnMultiplier * Meta.Instance.Gameplay.DifficultyEffects.CrossesSpawnMultiplier;

    public const int DEFAULT_RESURRECTION_COST = 75;
    public static int MinResurrectionCost = DEFAULT_RESURRECTION_COST;


    private static Rect2 _cameraLimits;
    public static Rect2 CameraLimits 
    {
        get { return _cameraLimits; }
        set 
        {
            _cameraLimits = value;
            OnCameraLimitsChanged?.Invoke(value);
        }
    }
    public delegate void CameraLimitsChangedEventHandler(Rect2 limits);
    public static event CameraLimitsChangedEventHandler OnCameraLimitsChanged = delegate { };

    public static Variant[] TransitiveVariant = new Variant[64]; // You can store almost anything here for anything. The game deletes the data only after entering the menu. If you need to save some data after restarting a level or moving to another scene, this option is perfect for you
    public static object[] TransitiveObject = new object[64]; // Addition to Variant, if some required data types are not supported
    // P.s. we HIGHLY recommend commenting out these variables in your code to avoid confusion. Especially if you use a lot of them.

    public const float FLOAT_DELTA = 0.016667f; // Default physics delta for 60 fps

    public static Dictionary<string, Variant> TransitiveVariantD = new Dictionary<string, Variant>(); // TransitiveVariant, but for those who don't want to get confused by unnamed array elements and don't need to comment out the elements.

    public static Variant GetModOption(string modName, string optionKey)
    {
        Dictionary modDictionary = (Dictionary)ModDataManager.ModSettings[modName];
        return modDictionary[optionKey];
    }

    public static Variant GetCurrentModMapOption(string optionKey)
    {
        Dictionary modDictionary = (Dictionary)ModDataManager.ModSettings[Path.GetFileName(ModMapPath)];
        return modDictionary[optionKey];
    }




    public static Variant TakeAndRemoveFromTrVaD(string key)
    {
        Variant value = TransitiveVariantD[key];
        TransitiveVariantD.Remove(key);
        return value;
    }

    public static string GetLanguagePrefix() // For localization maybe?
    {
        if (Meta.Instance.Video.language == Meta.VideoClass.Language.en)
            return "";
        var value = Meta.Instance.Video.language.ToString();
        value = char.ToUpper(value[0]) + value.Substring(1);
        return value;
    }

    /// <summary>
    /// Plays the sound once at the given path.
    /// <para>The default path for loading sounds is "res://Content/Sounds/". Remove this part from the path you send. Example soundPath: : "Interface&Menu/UI.mp3"</para>
    /// </summary>
    public static AudioStreamPlayer PlayOneshotSound(string soundPath, Node parent, string busName = "Master", float volumeDb = 0)
    {
        const string PATH_START = "res://Content/Sounds/";
        AudioStream stream = GD.Load<AudioStream>(PATH_START + soundPath);

        return PlayOneshotSound(stream, parent, busName, volumeDb);
    }
    public static AudioStreamPlayer PlayOneshotSound(AudioStream stream, Node parent, string busName = "Master", float volumeDb = 0)
    {
        AudioStreamPlayer player = new AudioStreamPlayer();

        player.Stream = stream;
        player.Finished += player.QueueFree;
        player.Bus = busName;
        player.VolumeDb = volumeDb;

        parent.AddChild(player);

        player.Play();

        return player;
    }

	/// <summary>
	/// Using example: await G.ToScore(30f, () => _isDisposed);
	/// <para>you can update bool _isDisposed with _EnterTree() and _ExitTree()</para>
	/// </summary>
    public static async Task ToScore(float scoreTarget, Func<bool> isNodeDisposed, int checkDelay = 100)
    {
        while (Scores < scoreTarget && !isNodeDisposed())
        {
            await Task.Delay(checkDelay);
        }
    }

    public static async Task WaitFor(float timeSec, bool ignorePause = true)
    {
        var timer = SceneTree.CreateTimer(timeSec, ignorePause);

        await timer.ToSignal(timer, "timeout");
    }

    public static async Task WaitForFrame()
    {
        await _sceneTree.ToSignal(_sceneTree, "process_frame");
    }

    /// <summary>
    /// Requires a CanvasItem to get the associated Viewport. Most often you can just insert 'this'
    /// </summary>
    public static Rect2 GetCameraRect(CanvasItem anyCanvasItem)
    {
        var CanvasTransfrom = anyCanvasItem.GetCanvasTransform();

        return new Rect2(-CanvasTransfrom.Origin, anyCanvasItem.GetViewportRect().Size / CanvasTransfrom.Scale);
    }

    public static void ResetMusicVariables()
    {
        MusicName = "";
        MusicStartPosition = 0;
        MusicStopTimeCode = 0;
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
            if (value == player) return;

            player = value;

            if (value != null)
                OnPlayerSetted.Handler?.Invoke();
        }
    }
    public static EventWrapper OnPlayerSetted = new();

    public static MainScript Main
    {
        get
        {
            return main;
        }
        set
        {
            if (value == main) return;

            main = value;

            if (value != null)
                OnMainSetted.Handler?.Invoke();
        }
    }
    public static EventWrapper OnMainSetted = new();


    public static LevelMusicPlayer MusicPlayer
    {
        get
        {
            return musicPlayer;
        }
        set
        {
            if (value == musicPlayer) return;

            musicPlayer = value;

            if (value != null)
                OnMusicPlayerSetted.Handler?.Invoke();
        }
    }
    public static EventWrapper OnMusicPlayerSetted = new();

    public static AdditionalGuiLayer AdditionalGuiLayer = null;


    private static Player player;
    private static MainScript main;
    private static LevelMusicPlayer musicPlayer;
    #endregion

    #region Useful events
    /// <summary>
    /// Use this if you want to do something at the end of the level intro, or if the intro is skipped (e.g. it was already there). 
    /// 
    /// <para>wasIntroShown shows if the intro was shown this time. You can use this as a marker if the level was run for the first time (true if yes, false if not)</para>
    /// <para>Instead of using this event directly, you can use OnLevelStarted signal from "Main" or BindLevelStartEventToNodeSafely method for a simpler structure. </para>
    /// </summary>
    public static EventWrapper1A<bool> OnLevelStarted = new();

    #endregion

    #endregion

    ////////////////////////////

    // Other stuff

    public static void ResetValues() // Usually used during a level restart
    {
        //
		IsNewRecordReached = false;
        HasLevelBeenCompleted = false;

        // Consequences after death
        IsPlayerDead = false;
        PlayerCorpseFlightTimer = 0;
		AfterPlayerCorpseFlightTimer = 0;
        ResurrectionsInARow = 0;

        ShowMouseDuringGameplay = 0;

        // Numbers? Idk
        ResetTimer = 0;
        Scores = 0;

        // Crosses
        Crosses.ResetLocalValues();
    }
	public static void CompletelyResetValues() // Usually used during the exit from the level
    {
		ResetValues();

        //Level info
        CurrentLevel = 0;
        LevelAdditionalLink = null;

        UpdateMouseVisible();

        DidLevelIntroPassed = false;
        WasTheLevelRestarted = false;

        IsProgressPaused = false;

        MinResurrectionCost = DEFAULT_RESURRECTION_COST;

        //Crosses
		IsCrossesEnabled = true;
		CrossesProgressCoeff = 1;
        CrossSpawnMultiplier = 1;
		
		Crosses.SetDefaultCrossesPack();



        //Music
		MusicStartPosition = 0;
        MusicStopTimeCode = 0;
        MusicName = "";

        //Sounds (I don't remember why this code is here, I'm afraid to remove it, just shhhh.)
        AudioServer.SetBusEffectEnabled(2, 0, false);
		AudioServer.SetBusEffectEnabled(6, 0, false);
        
                for (int i = 0; i < TransitiveVariant.Length; i++)
            TransitiveVariant[i] = "";

        for (int i = 0; i < TransitiveObject.Length; i++)
            TransitiveObject[i] = null;

        foreach (string v in TransitiveVariantD.Keys)
            TransitiveVariantD.Remove(v);

        //For mods
        IsLevelVanilla = true;
        ModManager.CurrentModMapFolderName = "";
    }

    public override void _PhysicsProcess(double delta) // This for debugging.
    {
        //GD.Print();
    }
}