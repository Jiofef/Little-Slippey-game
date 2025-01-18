using Godot;
using System;
using static AppearingText;

public partial class Level000000000Revelation : Monologue
{
    AnimationPlayer _monologueAnimations;
    private string _playerName;
	public override void _Ready()
	{
        // Initialising the nodes
        _the = GetNode<AnimatedSprite2D>("The");
        _monologueAnimations = GetNode<AnimationPlayer>("MonologueAnimations");

        AppearingText = GetNode<AppearingText>("ThisIs");

        // For gamepads and keyboard
        GetNode<TextureButton>("EnterHandler").GrabFocus();

        //Initialising the monologue phrases

        _playerName = OS.HasEnvironment("USERNAME") ? OS.GetEnvironment("USERNAME") : Tr("Player");

        monologueProperties = [
        PP("Yeah, I agree with you.", 10), //0
        PP("It's a total piece of crap, not a level.", 10), //1
        PP("It's a mindless mess.", 10), //2
        PP("I'm not even gonna argue.", 10), //3
        PP("In our postmodern era, it's common to justify complete shit by saying \"I did it bad on purpose\", isn't it?"), //4
        PP("*Hahaha*"), //5
        PP("Programs don't break like that."), //6
        PP("And a lot of things are not really done the way they are shown to us in games and movies."), //7
        PP("Somehow we've gotten used to these conventions."), //8
        PP("It's one thing when they're woven in intelligently."), //9
        PP("But, uh. You're not blind, are you?"), //10
        PP("You have no idea how many white threads I had to weave into this \"monster\" to keep it from falling apart."), //11
        PP("Have you ever wondered what you're running the levels through?"), //12
        PP("Why the hell is there a program called \"Levels\" in the interface, simulating a working computer, when in fact it is a folder with camera recordings?"), //13
        PP("How come there are settings on that computer that change the course of those recordings?"), //14
        PP("And how did deleting some mod manage to change, again, the recordings already made?"), //15
        PP("Why is there... A story... Here?.."), //16
        PP("Well, we both know who it's for anyway."), //17
        PP("They'll just make it up as they go along and write me a story 3 games ahead of time."), //18
        PP("Although, to be honest, I didn't intend it that way..."), //19
        PP("When I started making this game, the \"creepy things in a cute wrapper\" setting wasn't that hackneyed yet."), //20
        PP("I wasn't that old."), //21
        PP("And... Not that far behind."), //22
        PP("I used to have eyes light up, too"), //23
        PP("AND THEY BURNED."), //24
        PP("..."), //25
        PP("Here's what you \"came\" here for."), //26
        PP("It wasn't worth it, right?"), //27
        PP("You probably don't care, though."), //28
        PP("I'm not happy with the work I've done either."), //29
        PP("In this life, we are given so little time to do anything."), //30
        PP("There were so many things I wanted to be..."), //31
        PP("All right."), //32
        PP(Tr("Goodbye, I'll see you again, ") + _playerName + Tr("Goodbye, I'll see you again2")), //33
        PP("Maybe."), //34
        PP(""), //35
        ];
        // gde-to zdes nado vstavit playername

        NextPhrase();
    }


    #region Jiofef animation
    AnimatedSprite2D _the;
    string _currentTheAnimation = "LookingForward";
    private readonly string[] _allTheAnimations = ["LookingForward", "Laughing", "LookingUp", "LookingUp", "SmallLaugh", "AfterLaugh"];

    Random _random = new Random();
    private const int AVERAGE_ADDITIONAL_FRAME_APPEAR_PERIOD = 100;
    public override void _PhysicsProcess(double delta)
    {
        /* The sprite has several animations. In each of the animations except for the laughter animation, 
         * the frames should not have a clear order, so we have to choose a random one.
         * The 4th frame should be rare, and the others with equal chance   */
        if (_the.Animation != "Laughing" && _the.Animation != "SmallLaugh")
        {
            _the.Frame = _random.Next(_the.SpriteFrames.GetFrameCount(_currentTheAnimation) - 1);

            if (_random.Next(AVERAGE_ADDITIONAL_FRAME_APPEAR_PERIOD) == 0)
            {
                _the.Frame = _the.SpriteFrames.GetFrameCount(_currentTheAnimation);
            }
        }
    }

    public void SetTheAnimation(string name)
    {
        _the.Animation = name;
        _currentTheAnimation = name;

        if (name == "Laughing")
        {
            _the.Play();
            GetNode<AudioStreamPlayer>("The/LaughX3n").Play();
        }
        else if (name == "SmallLaugh")
        {
            _the.Play();
            GetNode<AudioStreamPlayer>("The/Laugh").Play();
        }
        else if (name != "AfterLaugh")
            _the.Stop();
    }

    public void OnLaughFinished()
    {
        SetTheAnimation("AfterLaugh");
    }
    #endregion

    #region Monologue
    public static PhraseProperties PP(string text, float charsPerSecond = DEFAULT_CHARS_PER_SEC) // Just an abbreviation for PhraseProperties, I'm lazy
    {
        return new PhraseProperties(text, charsPerSecond);
    }

    public override void NextPhrase()
    {
        base.NextPhrase();
        switch (PhraseIndex)
        {
            case 5:
                _monologueAnimations.Play("HeadAppearing");
                PreventEnter = true;
                    break;
            case 6:
                // The music doesn't play before that
                GetNode<AudioStreamPlayer>("Music").Play();
                PreventEnter = false;
                break;
            case 8: SetTheAnimation("LookingDown");
                break;
            case 10: SetTheAnimation("LookingForward");
                break;
            case 15: SetTheAnimation("LookingUp");
                break;
            case 16: SetTheAnimation("LookingForward");
                break;
            case 17: SetTheAnimation("LookingUp");
                break;
            case 18: SetTheAnimation("LookingForward");
                break;
            case 19: SetTheAnimation("LookingDown");
                break;
            case 21: SetTheAnimation("SmallLaugh");
                break;
            case 22: SetTheAnimation("LookingForward");
                break;
            case 24: SetTheAnimation("Laughing");
                break;
            case 25: SetTheAnimation("LookingDown");
                break;
            case 26: SetTheAnimation("LookingForward");
                break;
            case 28: SetTheAnimation("LookingUp");
                break;
            case 29: SetTheAnimation("LookingForward");
                break;
            case 30: SetTheAnimation("LookingDown");
                break;
            case 32: SetTheAnimation("LookingUp");
                break;
            case 33: SetTheAnimation("LookingForward");
                break;
            case 35:
                PreventEnter = true;
                _monologueAnimations.Play("HeadDisappearing");
                break;
        }
    }
    #endregion

    public void ReturnToGOS()
    {
        G.CompletelyResetValues();

        if (!ModManager.IsStandartTimerLibExists()) // Returning the StandartTimerLib
            ModManager.CreateStandartTimerLib();

        // !Need to enable StandartTimerLib if it is disabled!

        GetTree().ChangeSceneToFile("res://Content/Scenes/Interface&Menu/WelcomeToGOS.tscn");
    }
}
