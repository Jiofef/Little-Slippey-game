using Godot;
using System;
using System.Linq;

public partial class Level10ScientistScript : Node2D
{
    // To save between restarts
    public class LevelState
    {
        public bool IsFakeLevelEntry;
        public int DeathCount;
        public bool IsTimerBroken;
        public float SavedScores;
        public bool HasFiveDeathsTriggered;
        public bool IsFakeLevel10Handled;


        public int CurrentPhraseIndex;
        public float MegaphoneTimer;
        public float MusicPlayerVolume;
        public int CurrentMusicPart;
        public float MegaphonePlaybackPosition;


        public bool IsLevelComplete;
        public bool IsCrossesConfiguredAfterLevelCompleted;
        public int LastRandomPhrase;
        public bool ShowIntro;
    }
    public LevelState _level = new LevelState();

    public class SubtitlesState
    {
        public string TextToDraw;
        public float TimeToDraw;
        public float Timer;
        public string[] TextsQueue;
        public float[] TextTimeCodes;
        public bool IsTextQueued;
        public float TextSavingTime;
        public int CurrentQueueNumber;
        public Color ColorModulate;
        public string CurrentAnimation;
        public bool IsPhysicsProcessing;
        public string Text;
        public float VisibleRatio;
    }
    public SubtitlesState _subtitles = new SubtitlesState();

    // Signals
    [Signal] public delegate void SetScoresEventHandler();
    [Signal] public delegate void SetResetDisabledEventHandler();
    [Signal] public delegate void ShowTextQueueEventHandler();
    [Signal] public delegate void ClearTextEventHandler();
    [Signal] public delegate void SetCrossesProgressCoeffEventHandler();
    [Signal] public delegate void RecalculateCrossWeightEventHandler();

    // Nodes
    AudioStreamPlayer2D _megaphone;
    Level10MusicPlayer _musicPlayer;
    Player _player;
    MainScript _mainScript;

    // Phrases
    private float _megaphonePhraseTimer = 0;
    private readonly float[] _scriptedPhrasesTimeCodes = {3, 50, 150, 250, 290, 300};
    private readonly float[][] _phrasesTimeCodes =
    {
        [0, 4.4f, 9.15f, 11.5f, 15, 18.3f, 20.75f,24.7f, 25.55f, 27],
        [0,3.6f, 7.1f, 11.7f, 13.4f],
        [0, 3.7f, 7],
        [0, 3],
        [0, 2],
        [0, 2.3f, 4.3f, 6.35f, 8.1f, 11, 13.5f, 15.8f, 20],
        [0, 4.4f],
        [0, 2],
        [0, 2],
        [0, 2],
        [0, 2],
        [0, 2],
        [0, 2],
        [0, 2.5f],
    };
    private readonly float[][] _phrasesTimeCodesRu =
    {
        [0.25f, 4.5f, 9.3f, 11.4f, 15.1f, 17.8f, 19.85f,23.65f, 24.15f, 26],
        [0, 2.9f, 6.25f, 9.9f, 12],
        [0, 3.9f, 7],
        [0, 2.5f],
        [0, 2],
        [0, 2.4f, 3.9f, 6.25f, 8.15f, 9.7f, 10.8f, 13.2f, 17],
        [0, 4],
        [0, 2],
        [0, 2],
        [0, 2],
        [0, 1.5f],
        [0, 2.3f],
        [0, 2],
        [0, 2.5f],
    };
    private readonly string[][] _phrasesSubtitles =
    {
        ["Hey! Listen. I pulled you out of the previous test.", "The bad guys who are keeping you here wanted to make you go through a handful more test chambers,",
            "but I can see that you've suffered enough.", "Hold on, I'll try to break this test and get you free.", "I will be able to do this only after three hundred seconds,",
            "that's how long it takes to pass this chamber.",
            "It's a lot, but don't worry, I'll figure out how to help you.",
            "As for now...",
            "JUST LIVE!"],

        ["Listen, I think there's an opportunity to break the timer here.",
            "This will stop it from zeroing out every time a bomb hits you.",
            "He's got some kind of twisted security system here, but I'll figure something out.",
            "Just live for now!"],

        ["YES! I DID IT! LIVE, SLIPPEY, LIVE!",
            "YOU HAVE EVERY CHANCE TO LIVE UP TO THREE HUNDRED SECONDS!"],
        ["JUST A LITTLE BIT! DOO IT!"],

        ["10 SECONDS!!!"],

        ["YEAH! YOU DID IT!",
            "Congratulations!",
            "Well, I'm gonna...",
            "he... hey, wha...",
            "the timer was... fixed?",
            "I... I don't understand...",
            "I ca... I can't... I can't contro...*laugh*",
            "I can't control the crosses, hey, what's with them ;)"],

        ["Come on, I'll push the crosses out from you so it won't be so hard!"],

        ["Come on, hold on!"],

        ["I'm rooting for you!"],

        ["Don't give up!"],

        ["You'll slip away!"],

        ["I'm with you, my friend!"],

        ["I believe in you!"],

        ["Hold on, it will be over soon!"]
    };

    // Other vars
    Random random = new Random();
    const int TIMER_BREAK_TIME = 150;

    public override void _Ready()
    {
        // Initialize constants for the level
        G.LevelCompleteTime = 300;
        G.CrossesProgressCoeff = 0.5f;

        // Initialize nodes
        _megaphone = GetNode<AudioStreamPlayer2D>("Megaphone");
        _musicPlayer = GetNode<Level10MusicPlayer>("../../LevelMusicPlayer");
        _player = GetNode<Player>("../Player");
        _mainScript = GetNode<MainScript>("../..");

        // Connect signals to their respective methods
        Connect("ShowTextQueue", new Callable(GetNode("CanvasLayer/Subtitles"), "ShowTextQueue"));
        Connect("ClearText", new Callable(GetNode("CanvasLayer/Subtitles"), "ClearText"));
        Connect("SetResetDisabled", new Callable(_mainScript, "SetResetDisabled"));
        _player.Connect("PlayerDied", new Callable (this, "PlayerDied"));
        _mainScript.Connect("OnLevelResetting", new Callable(this, "OnLevelReset"));



        // Handle fake level 10 entry scenario
        if (G.TransitiveVariantD.ContainsKey("ImFromTheFakeLevel10"))
        {
            G.IsProgressPaused = false;

            G.TransitiveVariantD.Remove("ImFromTheFakeLevel10"); // Reset flag
            GetNode<AnimationPlayer>("../CanvasLayer/ColorRect/AnimationPlayer").Play("Blumxd");
            GetNode<AudioStreamPlayer>("../CanvasLayer/ColorRect/AudioStreamPlayer").Play();

            _mainScript.SetCrossesEnabled(true);
            _mainScript.SetProgressPaused(false);
            LoadMegaphoneInitialState();

            _level.IsFakeLevel10Handled = true; // Mark that the scenario was handled
        }

        // Set up megaphone or subtitles based on intro state
        if (!G.DidLevelIntroPassed && !_level.IsFakeLevel10Handled || G.TransitiveVariantD.ContainsKey("ImFromLevel000000000"))
        {
            G.TransitiveVariantD.Remove("ImFromLevel000000000");

            LoadMegaphoneInitialState();
        }
        else if (!_level.IsFakeLevel10Handled)
        {
            LoadSavedStatesAfterRestart();
        }

        if (_level.IsTimerBroken) // After surviving 150 seconds once
        {
            EmitSignal("SetResetDisabled", true);
            Connect("SetScores", new Callable(GetNode("../.."), "SetScores"));
            CallDeferred("emit_signal", "SetScores", _level.SavedScores);

            var whiteNoiseGlitch = GetNode<AnimatedSprite2D>("../CanvasLayer/WhiteNoiseGlitch");
            whiteNoiseGlitch.Visible = true;
            whiteNoiseGlitch.Play();
            GetNode<AudioStreamPlayer>("../CanvasLayer/WhiteNoiseGlitch/AudioStreamPlayer").Play();
        }

        // Load saved subtitles state if intro has passed
        if (G.DidLevelIntroPassed)
        {
            LoadSubtitlesSavedState();
        }
    }

    public void LoadMegaphoneInitialState()
    {
        _level.CurrentPhraseIndex = -1; // Initialize phrase index
        _megaphonePhraseTimer = 3;  // Start timer for the first phrase
    }

    public void LoadSavedStatesAfterRestart()
    {
        _level = (LevelState)G.TransitiveObject[0];
        _subtitles = (SubtitlesState)G.TransitiveObject[1];

        _megaphone.Stream = (AudioStream)G.TransitiveVariant[0]; // Loading the megaphone stream
        _megaphone.Play(_level.MegaphonePlaybackPosition);
        _megaphonePhraseTimer = _level.MegaphoneTimer;
        _musicPlayer.VolumeDb = _level.MusicPlayerVolume;
    }

    ////////

    public override void _PhysicsProcess(double delta)
    {
        _megaphonePhraseTimer -= 0.01667f;
        //Music fades out when megaphone speaks
        if (_megaphone.Playing && _musicPlayer.VolumeDb > 4)
            _musicPlayer.VolumeDb -= 0.2f;
        //The music comes back when the megaphone fades out
        else if (!_megaphone.Playing && _musicPlayer.VolumeDb < 10)
            _musicPlayer.VolumeDb += 0.2f;


        // When level finished
        if (G.Scores >= G.LevelCompleteTime && _level.IsLevelComplete != true)
        {
            OnLevelCompleted();
        }
        else if (G.Scores >= G.LevelCompleteTime + 15)
        {
            if (!G.IsPlayerDead)
            {
                if (!_level.IsCrossesConfiguredAfterLevelCompleted)
                {
                    _level.IsCrossesConfiguredAfterLevelCompleted = true;

                    G.IsCrossesEnabled = true;
                    G.CrossesProgressCoeff = 0.05f;
                    Crosses.UpdateCrossesWeight();
                    Crosses.EnableGoldenCrossSpawning = false;
                    Crosses.CurrentPackName = "LightweightTNT2.0 Modified";
                }
                G.CrossSpawnMultiplier *= 1.01f;
            }
        }

        // Pushing the crosses away from the player
        if (_level.DeathCount >= 5) 
        {
            // It should not push away the golden and enhanced blum crosses
            var AllCrossesOnScreen = GetTree().GetNodesInGroup("Crosses").Where(child => !(child is BlueElementalCrossPart) && !(child is EnhancedBlumCross));
            const float PUSH_SPEED = 5f;
            if (AllCrossesOnScreen.Count() > 0)
            {
                var FirstCross = (Node2D)AllCrossesOnScreen.First();
                FirstCross.Position -= FirstCross.GlobalPosition.DirectionTo(_player.GlobalPosition) * PUSH_SPEED;
                var LastCross = (Node2D)AllCrossesOnScreen.Last();
                LastCross.Position -= LastCross.GlobalPosition.DirectionTo(_player.GlobalPosition) * PUSH_SPEED;
            }
        }



        bool MegaphoneDefaultCondition = !_megaphone.Playing && _megaphonePhraseTimer <= 0 && _scriptedPhrasesTimeCodes.Length > _level.CurrentPhraseIndex + 1 && !G.IsPlayerDead;
        // Playing current phrases
        if (MegaphoneDefaultCondition && G.Scores >= _scriptedPhrasesTimeCodes[_level.CurrentPhraseIndex + 1])
        {
            PlayNumeralPhrase(_level.CurrentPhraseIndex);
            _level.CurrentPhraseIndex++;
        }

        // Playing crosses help phrase
        else if (MegaphoneDefaultCondition && _level.DeathCount >= 5 && _level.HasFiveDeathsTriggered != true)
        {
            _level.HasFiveDeathsTriggered  = true;
            PlayPhrase("FiveDeaths");
            EmitSignal("ShowTextQueue", _phrasesSubtitles[6], Meta.Instance.Video.language == Meta.VideoClass.Language.en ? _phrasesTimeCodes[6] : _phrasesTimeCodesRu[6], 1);
        }

        // Playing random phrases
        else if (MegaphoneDefaultCondition && _scriptedPhrasesTimeCodes[_level.CurrentPhraseIndex + 1] - G.Scores > 10 && !G.IsPlayerDead && G.Scores > 5)
        {
            if (random.Next(2000) == 0)
            {
                PlayRandomPhrase();
            }
        }
    }

    ////////

    public void PlayerDied()
    {
        _level.ShowIntro = false;
        if (G.Scores < G.LevelCompleteTime && (G.Scores > TIMER_BREAK_TIME || _level.IsTimerBroken))
        {

            _level.IsTimerBroken = true;
            G.Scores -= random.Next(5, 15);
            _level.SavedScores = G.Scores;
            Crosses.UpdateCrossesWeight();

            // Switch to secret level
            if (_level.SavedScores < 0 && ModManager.IsStandartTimerLibLoaded && !ModManager.IsModsDisabled)
            {
                G.TransitiveVariantD.Add("SavedScores", G.Scores);
                G.TransitiveVariantD.Add("PlayerSavedPos", _player.Position);
                GetTree().ChangeSceneToFile("res://Content/Scenes/Levels/FullParts/Level000000000.tscn");
                return;
            }

            SaveSubtitlesState();

            _player.CallDeferred("Resurrect");
            _musicPlayer.SaveTimeCode();
            _musicPlayer.StartPlaying();
        }



        if (_level.IsLevelComplete)
        {
            G.IsCrossesEnabled = false;
            SetPhysicsProcess(false);
        }
    }


    public void OnLevelReset()
    {
        _level.DeathCount++;
        G.TransitiveVariant[0] = _megaphone.Stream; // Saving the megaphone stream
        _level.MegaphoneTimer = _megaphonePhraseTimer;
        _level.MusicPlayerVolume = _musicPlayer.VolumeDb;
        _level.MegaphonePlaybackPosition = _megaphone.GetPlaybackPosition();
        _level.ShowIntro = false;

        G.TransitiveObject[0] = _level;
        G.TransitiveObject[1] = _subtitles;

        SaveSubtitlesState();

        G.MusicStopTimeCode = _musicPlayer.GetPlaybackPosition();
    }

    public void OnLevelCompleted()
    {
        _level.IsLevelComplete = true;
        G.IsCrossesEnabled = false;
        G.Main.IsPauseDisabled = true;
        G.Main.IsResetDisabled = true;
        G.CrossSpawnMultiplier = 0.25f;

        G.Player.SetGUIVisible(false);
        G.Player.DisableAfterDeathGui = true;
        G.Player.UpdateGUIOptions();

        _megaphonePhraseTimer = 0;
        var AllCrossesOnScreen = GetTree().GetNodesInGroup("Crosses");
        for (int i = 0; AllCrossesOnScreen.Count > i; i++)
            AllCrossesOnScreen[i].QueueFree();
        GetNode<AnimationPlayer>("../CanvasLayer/ColorRect/AnimationPlayer").Play("Blumxd");
        GetNode<AudioStreamPlayer>("../CanvasLayer/ColorRect/AudioStreamPlayer").Play();

        Achievements.GetLevelAchievements();
        Achievements.GetAchievement("Congratulations!0");
        Achievements.GetAchievement("Congratulations!1");
        Achievements.GetAchievement("Congratulations!2");
        Achievements.GetAchievement("Congratulations!3");

        Connect("SetCrossesProgressCoeff", new Callable(GetNode("../.."), "SetCrossesProgressCoeff"));
        EmitSignal("SetCrossesProgressCoeff", 0.01f);
        Connect("RecalculateCrossWeight", new Callable(GetNode(".."), "RecalculateCrossWeight"));
        EmitSignal("RecalculateCrossWeight");
    }






    //Megaphone methods
    public void PlayPhrase(string value)
    {
        _megaphone.Stream = ResourceLoader.Load<AudioStream>("res://Content/Sounds/Levels/Level10Scientist" + value + G.GetLanguagePrefix() + ".mp3");
        _megaphone.Play();
    }

    public void PlayNumeralPhrase(int PhraseNumber)
    {
        int NextPhraseNumber = PhraseNumber + 1;
        PlayPhrase("Scripted" + (NextPhraseNumber + 1));

        EmitSignal("ShowTextQueue", _phrasesSubtitles[NextPhraseNumber], Meta.Instance.Video.language == Meta.VideoClass.Language.en ? _phrasesTimeCodes[NextPhraseNumber] : _phrasesTimeCodesRu[NextPhraseNumber], 1);

        // Breaking the timer
        if (PhraseNumber == 1)
        {
            EmitSignal("SetResetDisabled", true);

            GetNode<AnimationPlayer>("../CanvasLayer/ColorRect/AnimationPlayer").Play("Blumxd");
            GetNode<AudioStreamPlayer>("../CanvasLayer/TimerBroken").Play();

            G.Player.DisableAfterDeathGui = true;
            G.Player.UpdateGUIOptions();
        }
        else if (PhraseNumber == 3)
            _megaphonePhraseTimer = 0;
    }

    public void PlayRandomPhrase()
    {
        const int RANDOM_PHRASES_COUNT = 8;

        int i = random.Next(1, RANDOM_PHRASES_COUNT);
        while (i == _level.LastRandomPhrase)
            i = random.Next(1, RANDOM_PHRASES_COUNT);

        PlayPhrase("Random" + i);
        EmitSignal("ShowTextQueue", _phrasesSubtitles[i + 6], Meta.Instance.Video.language == Meta.VideoClass.Language.en ? _phrasesTimeCodes[i + 6] : _phrasesTimeCodesRu[i + 6], 1);
        _level.LastRandomPhrase = i;
    }

    public void PhraseFinished()
    {
        _megaphone.Stream = null;
        _megaphonePhraseTimer = 10;
        EmitSignal("ClearText");
    }




    //Saving / loading subtitles values
    public void SaveSubtitlesState()
    {
        var subtitles = GetNode<Subtitles>("CanvasLayer/Subtitles");
        _subtitles.TextToDraw = subtitles.TextToDraw;
        _subtitles.TimeToDraw = subtitles.TimeToDraw;
        _subtitles.Timer = subtitles.Timer;

        _subtitles.TextsQueue = subtitles.TextsQueue;
        _subtitles.TextTimeCodes = subtitles.TextTimeCodes;
        _subtitles.IsTextQueued = subtitles.IsTextQueued;
        _subtitles.TextSavingTime = subtitles.TextSavingTime;
        _subtitles.CurrentQueueNumber = subtitles.CurrentQueueNumber;

        _subtitles.ColorModulate = GetNode<ColorRect>("CanvasLayer/SubtitlesRect").Modulate;
        _subtitles.CurrentAnimation = GetNode<AnimationPlayer>("CanvasLayer/SubtitlesRect/AnimationPlayer").CurrentAnimation;
        _subtitles.IsPhysicsProcessing = subtitles.IsPhysicsProcessing();
        _subtitles.Text = subtitles.Text;
        _subtitles.VisibleRatio = subtitles.VisibleRatio;
    }

    public void LoadSubtitlesSavedState()
    {
        var subtitles = GetNode<Subtitles>("CanvasLayer/Subtitles");
        subtitles.TextToDraw = _subtitles.TextToDraw;
        subtitles.TimeToDraw = _subtitles.TimeToDraw;
        subtitles.Timer = _subtitles.Timer;

        subtitles.TextsQueue = _subtitles.TextsQueue;
        subtitles.TextTimeCodes = _subtitles.TextTimeCodes;
        subtitles.IsTextQueued = _subtitles.IsTextQueued;
        subtitles.TextSavingTime = _subtitles.TextSavingTime;
        subtitles.CurrentQueueNumber = _subtitles.CurrentQueueNumber;

        GetNode<ColorRect>("CanvasLayer/SubtitlesRect").Modulate = _subtitles.ColorModulate;
        if (!string.IsNullOrEmpty(_subtitles.CurrentAnimation))
            GetNode<AnimationPlayer>("CanvasLayer/SubtitlesRect/AnimationPlayer").Play(_subtitles.CurrentAnimation);
        subtitles.SetPhysicsProcess(_subtitles.IsPhysicsProcessing);
        subtitles.Text = _subtitles.Text;
        subtitles.VisibleRatio = _subtitles.VisibleRatio;
    }
}
