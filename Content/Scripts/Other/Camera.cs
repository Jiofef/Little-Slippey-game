using Godot;
using System;

public partial class Camera : Camera2D
{
    [Signal] public delegate void ResetEventHandler();
    AnimatedSprite2D _restartNoise;
    Player _player;
    Label _scores;
    TextureProgressBar _standBar;
    public override void _Ready()
    {
        G.CameraLimits = new Vector4(0, G.LevelXYSizes[G.CurrentLevel].X, G.LevelXYSizes[G.CurrentLevel].Y, 0);
        LimitsChangingBy(true);
        ResetSmoothing();

        _restartNoise = GetNode<AnimatedSprite2D>("GUICanvas/GUI/RestartNoise");
        _player = GetNode<Player>("..");
        _scores = GetNode<Label>("GUICanvas/GUI/Scores");
        _standBar = GetNode<TextureProgressBar>("GUICanvas/GUI/StandBar");

        ApplyGUIOptions(true);
    }
    public void LimitsChangingBy(bool DoResetSmoothing = false, float plus1 = 0, float plus2 = 0, float plus3 = 0, float plus4 = 0)
    {
        float[] Defaultlimits = {G.CameraLimits.X - 30, G.CameraLimits.Y + 30, G.CameraLimits.Z + 100, G.CameraLimits.W - 30};
        LimitTop = (int)(Defaultlimits[0] + plus1);
        LimitRight = (int)(Defaultlimits[1] + plus2);
        LimitBottom = (int)(Defaultlimits[2] + plus3);
        LimitLeft = (int)(Defaultlimits[3] + plus4);

        if (DoResetSmoothing)
            ResetSmoothing();
    }
    public override void _PhysicsProcess(double delta)
    {
        if (_scores.Visible)
        {
            _scores.Text = ((int)G.Scores).ToString();
            if (Meta.Instance.Video.ScoresLabelLocationY == 0)
                _scores.Modulate = new Color(_scores.Modulate.R, _scores.Modulate.G, _scores.Modulate.B, _player.Position.Y > G.CameraLimits.X + 200 ? 1 : _player.Position.Y / (G.CameraLimits.X + 200));
        }

        Vector2 LimitsExpansion = Vector2.Zero;
        _standBar.Value = 1 - G.PlayerMoveCoeff;

        if (G.ResetTimer != 0) // When resetting 
        {
            if (!_restartNoise.IsPlaying())
                _restartNoise.Play();
            _restartNoise.Modulate = new Color(_restartNoise.Modulate.R, _restartNoise.Modulate.G, _restartNoise.Modulate.B, G.ResetTimer / 2);

            var restartNoiseSound = GetNode<AudioStreamPlayer>("GUICanvas/GUI/RestartNoise/Sound");
            if (!restartNoiseSound.Playing)
                restartNoiseSound.Play();
            restartNoiseSound.VolumeDb = -5 + G.ResetTimer * 10;

            Random random = new Random();
            LimitsExpansion = new Vector2(random.Next(-50, 50) * G.ResetTimer, random.Next(-50, 50) * G.ResetTimer);
            Position = new Vector2(LimitsExpansion.X, LimitsExpansion.Y);
            LimitsChangingBy(true, LimitsExpansion.Y, LimitsExpansion.X, LimitsExpansion.Y, LimitsExpansion.X);
        }
        else if (_restartNoise.IsPlaying()) // When reset interrupts
        {
            var restartNoise = GetNode<AnimatedSprite2D>("GUICanvas/GUI/RestartNoise");
            restartNoise.Stop();
            restartNoise.Modulate = new Color(restartNoise.Modulate.R, restartNoise.Modulate.G, restartNoise.Modulate.B, 0);

            GetNode<AudioStreamPlayer>("GUICanvas/GUI/RestartNoise/Sound").Stop();
            LimitsChangingBy();
            LimitsExpansion = Vector2.Zero;
        }

        if (G.IsPlayerDead) // When player dead
        {
            float zoom = G.PlayerCorpseFlightTimer < 4 ? Meta.Instance.Video.CameraZoom + G.PlayerCorpseFlightTimer * ((4.5f - Meta.Instance.Video.CameraZoom) / 4) : 4.5f;
            Zoom = new Vector2(zoom, zoom);
            float PlayerCorpseFlightTimerX50 = G.PlayerCorpseFlightTimer * 50;
            LimitsChangingBy(false, -PlayerCorpseFlightTimerX50 - LimitsExpansion.Y, PlayerCorpseFlightTimerX50 + LimitsExpansion.X, PlayerCorpseFlightTimerX50 + LimitsExpansion.Y, -PlayerCorpseFlightTimerX50 - LimitsExpansion.X);

            if (G.PlayerCorpseFlightTimer >= 4.5f)
            {
                var emergingElements = GetNode<Node2D>("GUI/EmergingElements");
                if (G.IsNewRecordReached)
                {
                    const string link = "GUI/EmergingElements/NewRecordScores";
                    var newRecordScores = GetNode<Label>(link);
                    newRecordScores.Text = Tr("New Record!\nScore: ") + (int)G.Scores;
                    newRecordScores.Visible = true;
                    GetNode<CpuParticles2D>(link + "/Shine1").Emitting = true;
                    GetNode<CpuParticles2D>(link + "/Shine2").Emitting = true;
                }
                else
                {
                    var emergingScores = GetNode<Label>("GUI/EmergingElements/Scores");
                    emergingScores.Visible = true;
                    emergingScores.Text = Convert.ToString(Tr("Score: ") + (int)G.Scores);
                }
                if (emergingElements.Modulate.A < 1)
                    emergingElements.Modulate = new Color(emergingElements.Modulate.R, emergingElements.Modulate.G, emergingElements.Modulate.B, emergingElements.Modulate.A + 0.005f);

                void SetHoldIMG(string ImageName)
                {
                    GetNode<RichTextLabel>("GUI/EmergingElements/Hold R").Text = "[center]Hold [img]res://Content/Sprites/Interface/ControllerButtons/" + ImageName + ".png[/img]";
                }
                switch (G.TypeOfUsedController)
                {
                    case "Keyboard":
                        SetHoldIMG("KeyboardButtonBigR");
                        break;
                    case "PS Gamepad":
                        SetHoldIMG("PSControllerBigTriangle");
                        break;
                    default:
                        SetHoldIMG("XControllerBigY");
                        break;
                }
            }
        }
    }
    public void CameraZoom()
    {
        PositionSmoothingEnabled = false;
        _scores.Visible = false;
    }

    public void ApplyGUIOptions(bool IsLevelJustStarted)
    {
        if (IsLevelJustStarted)
        {
            _scores.Visible = Meta.Instance.Video.ScoresShowingFormatIndex != 2 && (G.CurrentLevel != 1 || G.LevelAdditionalLink != "Tutorial");
            Zoom = new Vector2(Meta.Instance.Video.CameraZoom, Meta.Instance.Video.CameraZoom);
        }
        else
        {
            _scores.Visible = Meta.Instance.Video.ScoresShowingFormatIndex != 2 && !G.IsPlayerDead && (G.CurrentLevel != 1 || G.LevelAdditionalLink != "Tutorial");
            float zoom = G.PlayerCorpseFlightTimer < 4 ? Meta.Instance.Video.CameraZoom + G.PlayerCorpseFlightTimer * ((4.5f - Meta.Instance.Video.CameraZoom) / 4) : 4.5f;
            Zoom = new Vector2(zoom, zoom);

            _scores.Modulate = new Color(_scores.Modulate.R, _scores.Modulate.G, _scores.Modulate.B);
        }

        float ScoresScale = Meta.Instance.Video.ScoresShowingFormatIndex == 0 ? 1.5f : 1;
        Vector2 ScoresSize = Meta.Instance.Video.ScoresShowingFormatIndex == 0 ? new Vector2(211, 120) : new Vector2(315, 175);
        if (_scores.Visible)
        {
            _scores.Scale = new Vector2(ScoresScale, ScoresScale);
            _scores.Size = ScoresSize;
        }

        _scores.HorizontalAlignment = (HorizontalAlignment)Meta.Instance.Video.ScoresLabelLocationX;
        _scores.VerticalAlignment = (VerticalAlignment)Meta.Instance.Video.ScoresLabelLocationY;
    }
}
