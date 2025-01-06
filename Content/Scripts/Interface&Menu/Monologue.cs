using Godot;
using System;
using static AppearingText;

public partial class Monologue : Node2D
{
    public enum SkipPhraseMode { SpeedUpTheAppearence, InstantAppearance }
    [Export] public SkipPhraseMode _skipPhraseMode = SkipPhraseMode.SpeedUpTheAppearence;
    [Export] public float SpeedUpTheAppearenceMultiplier = 4;
    [Export] public bool PreventEnter = false;


   
    
    public AppearingText AppearingText;

    public int PhraseIndex = -1;
    public PhraseProperties[] monologueProperties;
    public virtual void Enter()
    {
        if (PreventEnter) return;


        if (AppearingText.Appearing)
            SkipThePhrase();
        else
            NextPhrase();
    }

    private bool _phraseSkipped = false;
    public virtual void SkipThePhrase()
    {
        switch (_skipPhraseMode)
        {
            case SkipPhraseMode.SpeedUpTheAppearence:
                if (_phraseSkipped) break;
                _phraseSkipped = true;

                AppearingText.CharactersPerSecond *= SpeedUpTheAppearenceMultiplier;
                break;
            case SkipPhraseMode.InstantAppearance:
                AppearingText.SkipAppearing();
                break;
        }
    }

    public virtual void NextPhrase()
    {
        PhraseIndex++;
        AppearingText.SetText(monologueProperties[PhraseIndex]);


        if (_skipPhraseMode == SkipPhraseMode.SpeedUpTheAppearence && _phraseSkipped)
        {
            _phraseSkipped = false;
        }
    }
}
