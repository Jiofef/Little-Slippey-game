using Godot;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Channels;
using System.Threading;

public partial class Level10ScientistScript : Node2D
{
    [Signal] public delegate void SetScoresEventHandler();
    [Signal] public delegate void SetResetDisabledEventHandler();
    AudioStreamPlayer2D _megaphone;
    AudioStreamPlayer _musicPlayer;
    CharacterBody2D _player;
    private float _megaphonePhraseTimer = 0;
    private float[] _scriptedPhrasesTimeCodes = {3, 50, 150, 250, 290, 300};
    Random random = new Random();
    private float[,] _phrasesTimeCodes =
    {
        {

        }
    };
    private string[][] _phrasesEngSubtitles =
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
            "Hey, wha...",
            "the timer was... fixed?",
            "I don't understand...",
            "I-I can't co...",
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
    }
    public override void _PhysicsProcess(double delta)
    {
        if ((bool)G.TransitiveVariant[13])
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
            if ((int)G.TransitiveVariant[7] == 1)
                EmitSignal("SetResetDisabled", true);
            G.TransitiveVariant[7] = (int)G.TransitiveVariant[7] + 1;
        }
        else if (MegaphoneDefaultCondition && (int)G.TransitiveVariant[1] >= 5 && (bool)G.TransitiveVariant[6] != true)
        {
            G.TransitiveVariant[6] = true;
            PlayMegaphonePhrase("FiveDeaths");
        }
        else if (MegaphoneDefaultCondition && _scriptedPhrasesTimeCodes[(int)G.TransitiveVariant[7] + 1] - G.Scores > 10 && !G.IsPlayerDead && G.Scores > 5)
        {
            if (random.Next(2500) == 0)
            {
                int i = random.Next(1, 8);
                while (i == (int)G.TransitiveVariant[13])
                    i = random.Next(1, 8);
                PlayMegaphonePhrase("Random" + i);
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
        G.MusicStopTimeCode = _musicPlayer.GetPlaybackPosition();
    }
    public void PhraseFinished()
    {
        _megaphone.Stream = null;
        _megaphonePhraseTimer = 10;
    }
}
