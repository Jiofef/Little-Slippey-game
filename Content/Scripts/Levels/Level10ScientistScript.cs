using Godot;
using System;
using System.Linq;

public partial class Level10ScientistScript : Node2D
{
    [Signal] public delegate void SetScoresEventHandler();
    [Signal] public delegate void SetResetDisabledEventHandler();
    [Signal] public delegate void ShowTextQueueEventHandler();
    [Signal] public delegate void ClearTextEventHandler();
    AudioStreamPlayer2D _megaphone;
    AudioStreamPlayer _musicPlayer;
    CharacterBody2D _player;
    private float _megaphonePhraseTimer = 0;
    private float[] _scriptedPhrasesTimeCodes = {3, 50, 150, 250, 290, 300};
    Random random = new Random();
    private float[][] _phrasesTimeCodes =
    {
        new float[]
        {
            0, 4.4f, 9.15f, 11.5f, 15, 18.3f, 20.75f,24.7f, 25.55f, 27
        },
        new float[]
        {
            0,3.6f, 7.1f, 11.7f, 13.4f
        },
        new float[]
        {
            0, 3.7f, 7
        },
        new float[]
        {
            0, 3
        },
        new float[]
        {
            0, 2
        },
        new float[]
        {
            0, 2.3f, 4.3f, 6.35f, 8.1f, 11, 13.5f, 15.8f, 20
        },
        new float[]
        {
            0, 4.4f
        },
        new float[]
        {
            0, 2
        },
        new float[]
        {
            0, 2
        },
        new float[]
        {
            0, 2
        },
        new float[]
        {
            0, 2
        },
        new float[]
        {
            0, 2
        },
        new float[]
        {
            0, 2
        },
        new float[]
        {
            0, 2.5f
        },
    };
    private float[][] _phrasesTimeCodesRu =
    {
        new float[]
        {
            0.25f, 4.5f, 9.3f, 11.4f, 15.1f, 17.8f, 19.85f,23.65f, 24.15f, 26
        },
        new float[]
        {
            0, 2.9f, 6.25f, 9.9f, 12
        },
        new float[]
        {
            0, 3.9f, 7
        },
        new float[]
        {
            0, 2.5f
        },
        new float[]
        {
            0, 2
        },
        new float[]
        {
            0, 2.4f, 3.9f, 6.25f, 8.15f, 9.7f, 10.8f, 13.2f, 17
        },
        new float[]
        {
            0, 4
        },
        new float[]
        {
            0, 2
        },
        new float[]
        {
            0, 2
        },
        new float[]
        {
            0, 2
        },
        new float[]
        {
            0, 1.5f
        },
        new float[]
        {
            0, 2.3f
        },
        new float[]
        {
            0, 2
        },
        new float[]
        {
            0, 2.5f
        },
    };
    private string[][] _phrasesSubtitles =
    {
        new string[]
        {
            "Hey! Listen. I pulled you out of the previous test.", "The bad guys who are keeping you here wanted to make you go through a handful more test chambers,",
            "but I can see that you've suffered enough.", "Hold on, I'll try to break this test and get you free.", "I will be able to do this only after three hundred seconds,",
            "that's how long it takes to pass this chamber.",
            "It's a lot, but don't worry, I'll figure out how to help you.",
            "As for now...",
            "JUST LIVE!"
        },
        new string[]
        {
            "Listen, I think there's an opportunity to break the timer here.",
            "This will stop it from zeroing out every time a bomb hits you.",
            "He's got some kind of twisted security system here, but I'll figure something out.",
            "Just live for now!"
        },
        new string[]
        {
            "YES! I DID IT! LIVE, SLIPPEY, LIVE!",
            "YOU HAVE EVERY CHANCE TO LIVE UP TO THREE HUNDRED SECONDS!"
        },
        new string[]
        {
            "JUST A LITTLE BIT! DOO IT!"
        },
        new string[]
        {
            "10 SECONDS!!!"
        },
        new string[]
        {
            "YEAH! YOU DID IT!",
            "Congratulations!",
            "Well, I'm gonna...",
            "he... hey, wha...",
            "the timer was... fixed?",
            "I... I don't understand...",
            "I ca... I can't... I can't contro...*laugh*",
            "I can't control the crosses, hey, what's with them ;)"
        },
        new string[]
        {
            "Come on, I'll push the crosses out from you so it won't be so hard!"
        },
        new string[]
        {
            "Come on, hold on!"
        },
        new string[]
        {
            "I'm rooting for you!"
        },
        new string[]
        {
            "Don't give up!"
        },
        new string[]
        {
            "You'll slip away!"
        },
        new string[]
        {
            "I'm with you, my friend!"
        },
        new string[]
        {
            "I believe in you!"
        },
        new string[]
        {
            "Hold on, it will be over soon!"
        }
    };

    public override void _Ready()
    {
        Connect("ShowTextQueue", new Callable(GetNode("CanvasLayer/Subtitles"), "ShowTextQueue"));
        Connect("ClearText", new Callable(GetNode("CanvasLayer/Subtitles"), "ClearText"));
        Connect("SetResetDisabled", new Callable(GetNode("../../"), "SetResetDisabled"));
        if ((bool)G.TransitiveVariant[3])
            EmitSignal("SetResetDisabled", true);
        _megaphone = GetNode<AudioStreamPlayer2D>("Megaphone");
        _musicPlayer = GetNode<AudioStreamPlayer>("../../LevelMusicPlayer");
        _player = GetNode<CharacterBody2D>("../Player");
        if ((bool)G.TransitiveVariant[0] == true)
            G.TransitiveVariant[0] = "";
        else
            GetNode<ColorRect>("../CanvasLayer/ColorRect").QueueFree();

        if (!G.DidLevelIntroPassed)
        {
            G.TransitiveVariant[7] = -1;
            _megaphonePhraseTimer = 3;
        }
        else
        {
            _megaphone.Stream = (AudioStream)G.TransitiveVariant[8];
            _megaphone.Play((float)G.TransitiveVariant[11]);
            _megaphonePhraseTimer = (float)G.TransitiveVariant[9];
            _musicPlayer.VolumeDb = (float)G.TransitiveVariant[10];
        }

        if ((bool)G.TransitiveVariant[3])
        {
            Connect("SetScores", new Callable(GetNode("../.."), "SetScores"));
            CallDeferred("emit_signal", "SetScores", G.TransitiveVariant[4]);
            var whiteNoiseGlitch = GetNode<AnimatedSprite2D>("../CanvasLayer/WhiteNoiseGlitch");
            whiteNoiseGlitch.Visible = true;
            whiteNoiseGlitch.Play();
            GetNode<AudioStreamPlayer>("../CanvasLayer/WhiteNoiseGlitch/AudioStreamPlayer").Play();
        }
        if (G.DidLevelIntroPassed)
        {
            LoadSubtitlesSavedState();
        }
    }
    public override void _PhysicsProcess(double delta)
    {
        if ((bool)G.TransitiveVariant[14])
        {
            G.CrossSpawnMultiplier *= 1.01f;
        }
        if (G.Scores >= 300 && (bool)G.TransitiveVariant[12] != true)
        {
            G.TransitiveVariant[12] = true;
            G.IsCrossesEnabled = false;
            var AllCrossesOnScreen = GetTree().GetNodesInGroup("Crosses");
            for (int i = 0; AllCrossesOnScreen.Count > i; i++)
                AllCrossesOnScreen[i].QueueFree();
        }
        _megaphonePhraseTimer -= 0.01667f;
        if (_megaphone.Playing && _musicPlayer.VolumeDb > 4)
            _musicPlayer.VolumeDb -= 0.2f;
        else if (!_megaphone.Playing && _musicPlayer.VolumeDb < 10)
            _musicPlayer.VolumeDb += 0.2f;
        if ((int)G.TransitiveVariant[1] >= 5)
        {
            var AllCrossesOnScreen = GetTree().GetNodesInGroup("Crosses");
            if (AllCrossesOnScreen.Count() > 0)
            {
                var FirstCross = (Node2D)AllCrossesOnScreen.First();
                FirstCross.Position -= FirstCross.GlobalPosition.DirectionTo(_player.GlobalPosition) * 5;
                var LastCross = (Node2D)AllCrossesOnScreen.Last();
                LastCross.Position -= LastCross.GlobalPosition.DirectionTo(_player.GlobalPosition) * 5;
            }
        }
        bool MegaphoneDefaultCondition = !_megaphone.Playing && _megaphonePhraseTimer <= 0 && _scriptedPhrasesTimeCodes.Length > (int)G.TransitiveVariant[7] + 1 && !G.IsPlayerDead;
        if (MegaphoneDefaultCondition && G.Scores >= _scriptedPhrasesTimeCodes[(int)G.TransitiveVariant[7] + 1])
        {
            PlayMegaphonePhrase("Scripted" + ((int)G.TransitiveVariant[7] + 2));
            EmitSignal("ShowTextQueue", _phrasesSubtitles[(int)G.TransitiveVariant[7] + 1], Meta.Instance.language == Meta.Language.en ? _phrasesTimeCodes[(int)G.TransitiveVariant[7] + 1] : _phrasesTimeCodesRu[(int)G.TransitiveVariant[7] + 1], 1);
            if ((int)G.TransitiveVariant[7] == 1)
                EmitSignal("SetResetDisabled", true);
            G.TransitiveVariant[7] = (int)G.TransitiveVariant[7] + 1;
        }
        else if (MegaphoneDefaultCondition && (int)G.TransitiveVariant[1] >= 5 && (bool)G.TransitiveVariant[6] != true)
        {
            G.TransitiveVariant[6] = true;
            PlayMegaphonePhrase("FiveDeaths");
            EmitSignal("ShowTextQueue", _phrasesSubtitles[6], Meta.Instance.language == Meta.Language.en ? _phrasesTimeCodes[6] : _phrasesTimeCodesRu[6], 1);
        }
        else if (MegaphoneDefaultCondition && _scriptedPhrasesTimeCodes[(int)G.TransitiveVariant[7] + 1] - G.Scores > 10 && !G.IsPlayerDead && G.Scores > 5)
        {
            if (random.Next(2000) == 0)
            {
                int i = random.Next(1, 8);
                while (i == (int)G.TransitiveVariant[13])
                    i = random.Next(1, 8);
                PlayMegaphonePhrase("Random" + i);
                EmitSignal("ShowTextQueue", _phrasesSubtitles[i + 6], Meta.Instance.language == Meta.Language.en ? _phrasesTimeCodes[i + 6] : _phrasesTimeCodesRu[i + 6], 1);
                G.TransitiveVariant[13] = i;
            }
        }
    }
    public void PlayerDied()
    {
        if (G.Scores > 300)
        {
            GetNode<Node2D>("../Player/Camera2D/GUI/EmergingElements").Visible = false;
            GetNode<Node2D>("../../").SetPhysicsProcess(false);
        }
        else if (G.Scores > 150 || (bool)G.TransitiveVariant[3])
        {
            OnLevelReset();
            G.TransitiveVariant[3] = true;
            G.TransitiveVariant[4] = G.Scores - random.Next(5, 15);
            if ((float)G.TransitiveVariant[4] < 0)
                G.TransitiveVariant[4] = 0;

            SaveSubtitlesState();

            GetTree().ReloadCurrentScene();
        }

    }
    public void PlayMegaphonePhrase(string value)
    {
        _megaphone.Stream = ResourceLoader.Load<AudioStream>("res://Content/Sounds/Levels/Level10Scientist" + value + G.GetLanguagePrefix() + ".mp3");
        _megaphone.Play();
    }

    public void OnLevelReset()
    {
        G.TransitiveVariant[1] = (int)G.TransitiveVariant[1] + 1;
        G.TransitiveVariant[8] = _megaphone.Stream;
        G.TransitiveVariant[9] = _megaphonePhraseTimer;
        G.TransitiveVariant[10] = _musicPlayer.VolumeDb;
        G.TransitiveVariant[11] = _megaphone.GetPlaybackPosition();

        SaveSubtitlesState();

        G.MusicStopTimeCode = _musicPlayer.GetPlaybackPosition();
    }
    public void PhraseFinished()
    {
        _megaphone.Stream = null;
        _megaphonePhraseTimer = 10;
        EmitSignal("ClearText");
    }
    public void SaveSubtitlesState()
    {
        var subtitles = GetNode<Subtitles>("CanvasLayer/Subtitles");
        G.TransitiveVariant[16] = subtitles._textToDraw;
        G.TransitiveVariant[17] = subtitles._timeToDraw;
        G.TransitiveVariant[18] = subtitles._timer;

        G.TransitiveVariant[19] = subtitles._textsQueue;
        G.TransitiveVariant[20] = subtitles._textTimeCodes;
        G.TransitiveVariant[21] = subtitles._isTextQueued;
        G.TransitiveVariant[22] = subtitles._textSavingTime;
        G.TransitiveVariant[23] = subtitles._currentQueueNumber;

        G.TransitiveVariant[24] = GetNode<ColorRect>("CanvasLayer/ColorRect").Modulate;
        G.TransitiveVariant[25] = GetNode<AnimationPlayer>("CanvasLayer/ColorRect/AnimationPlayer").CurrentAnimation;
        G.TransitiveVariant[26] = subtitles.IsPhysicsProcessing();
        G.TransitiveVariant[27] = subtitles.Text;
        G.TransitiveVariant[28] = subtitles.VisibleRatio;
    }
    public void LoadSubtitlesSavedState()
    {
        var subtitles = GetNode<Subtitles>("CanvasLayer/Subtitles");
        subtitles._textToDraw = (string)G.TransitiveVariant[16];
        subtitles._timeToDraw = (float)G.TransitiveVariant[17];
        subtitles._timer = (float)G.TransitiveVariant[18];

        subtitles._textsQueue = (string[])G.TransitiveVariant[19];
        subtitles._textTimeCodes = (float[])G.TransitiveVariant[20];
        subtitles._isTextQueued = (bool)G.TransitiveVariant[21];
        subtitles._textSavingTime = (float)G.TransitiveVariant[22];
        subtitles._currentQueueNumber = (int)G.TransitiveVariant[23];
        GetNode<ColorRect>("CanvasLayer/ColorRect").Modulate = (Color)G.TransitiveVariant[24];
        if ((string)G.TransitiveVariant[25] != "")
            GetNode<AnimationPlayer>("CanvasLayer/ColorRect/AnimationPlayer").Play((string)G.TransitiveVariant[25]);
        subtitles.SetPhysicsProcess((bool)G.TransitiveVariant[26]);
        subtitles.Text = (string)G.TransitiveVariant[27];
        subtitles.VisibleRatio = (float)G.TransitiveVariant[28];
    }
}
