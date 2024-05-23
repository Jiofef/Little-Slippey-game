using Godot;
using System;
using System.Linq;

public partial class Level10ScientistScript : Node2D
{
    [Signal] public delegate void SetScoresEventHandler();
    [Signal] public delegate void SetResetDisabledEventHandler();
    AudioStreamPlayer2D _megaphone;
    AudioStreamPlayer _musicPlayer;
    CharacterBody2D _player;
    private float _megaphonePhraseTimer = 0;
    private float[] _scriptedPhrasesTimeCodes = {3, 50, 150, 250, 290, 300};
    //private float[,] _phrasesTimeCodes =
    //{
    //    {
            
    //    }
    //};
    //private string[,] _phrasesSubtitles =
    //{
    //    {

    //    }
    //};
    
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
        if (_scriptedPhrasesTimeCodes.Length > (int)G.TransitiveVariant[7] + 1 && G.Scores >= _scriptedPhrasesTimeCodes[(int)G.TransitiveVariant[7] + 1])
        {
            PlayMegaphonePhrase("Scripted" + ((int)G.TransitiveVariant[7] + 2));
            if ((int)G.TransitiveVariant[7] == 1)
                EmitSignal("SetResetDisabled", true);
            G.TransitiveVariant[7] = (int)G.TransitiveVariant[7] + 1;
        }
        if (!_megaphone.Playing && _megaphonePhraseTimer <= 0 && (int)G.TransitiveVariant[1] >= 5 && (bool)G.TransitiveVariant[6] != true && !G.IsPlayerDead)
        {
            G.TransitiveVariant[6] = true;
            PlayMegaphonePhrase("FiveDeaths");
        }
        //if (!_megaphone.Playing && _megaphonePhraseTimer <= 0 && _scriptedPhrasesTimeCodes[(int)G.TransitiveVariant[7] + 1] - G.Scores > 10 && !G.IsPlayerDead && G.Scores > 5)
        //{
        //    Random random = new Random();
        //    if (random.Next(1) == 0)
        //    {
        //        PlayMegaphonePhrase("Random" + random.Next(1, 5));
        //    }
        //}
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
            G.TransitiveVariant[3] = true;
            Random random = new Random();
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
        GD.Print(_megaphonePhraseTimer);
        _megaphone.Stream = null;
        _megaphonePhraseTimer = 10;
        GD.Print(_megaphonePhraseTimer);
    }
}
