using Godot;
using System;

public partial class Camera : Camera2D
{
    // Needed nodes
    AnimatedSprite2D _restartNoise;
    Player _player;
    Label _scores;
    TextureProgressBar _standBar;


    Random _random = new Random();

    // The number of pixels visible outside the exposed limits of the camera. X = top, Y = right, Z = bottom, W = left
    public Vector4 ViewAngleAddition = new Vector4(-30, 30, 100, -30);

    private bool _isScoreDisabled = false;

    public override void _Ready()
    {
        //Initializing nodes
        _restartNoise = GetNode<AnimatedSprite2D>("GUICanvas/RestartNoise");
        _player = GetNode<Player>("..");
        _scores = GetNode<Label>("GUICanvas/GUI/Scores");
        _standBar = GetNode<TextureProgressBar>("GUICanvas/GUI/StandBar");


        G.CameraLimits = new Vector4(0, G.LevelXYSizes[G.CurrentLevel].X, G.LevelXYSizes[G.CurrentLevel].Y, 0);
        SetTheLimitsAddition(true);

        _onCameraLimitsChangedHandler = OnCameraLimitsChanged;
        G.OnCameraLimitsChanged += _onCameraLimitsChangedHandler;



        ApplyGUIOptions(true);

        preDeathParams = new PreDeathParams(this);
    }
    private G.CameraLimitsChangedEventHandler _onCameraLimitsChangedHandler;
    public override void _ExitTree()
    {
        if (_onCameraLimitsChangedHandler != null)
            G.OnCameraLimitsChanged -= _onCameraLimitsChangedHandler;
    }



    private void SetTheLimitsAddition(bool DoResetSmoothing = false, float plus1 = 0, float plus2 = 0, float plus3 = 0, float plus4 = 0)
    {
        Vector4 Defaultlimits = G.CameraLimits + ViewAngleAddition;
        LimitTop = (int)(Defaultlimits[0] + plus1);
        LimitRight = (int)(Defaultlimits[1] + plus2);
        LimitBottom = (int)(Defaultlimits[2] + plus3);
        LimitLeft = (int)(Defaultlimits[3] + plus4);

        if (DoResetSmoothing)
            ResetSmoothing();
    }
    private void UpdateTheLimits(bool DoResetSmoothing = false)
    {
        Vector4 Defaultlimits = G.CameraLimits + ViewAngleAddition;
        LimitTop = (int)Defaultlimits[0];
        LimitRight = (int)Defaultlimits[1];
        LimitBottom = (int)Defaultlimits[2];
        LimitLeft = (int)Defaultlimits[3];

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

            if (_isScoreDisabled != G.IsProgressPaused)
            {
                _isScoreDisabled = G.IsProgressPaused;
                if (_isScoreDisabled)
                    _scores.Modulate = new Color(0.6f, 0.6f, 0.6f);
                else
                    _scores.Modulate = new Color(1, 1, 1);
            }
        }

        Vector2 LimitsExpansion = Vector2.Zero;


        #region When resetting 
        if (G.ResetTimer != 0)
        {
            if (!_restartNoise.IsPlaying())
                _restartNoise.Play();
            _restartNoise.Modulate = new Color(_restartNoise.Modulate.R, _restartNoise.Modulate.G, _restartNoise.Modulate.B, G.ResetTimer / 2);

            var restartNoiseSound = _restartNoise.GetNode<AudioStreamPlayer>("Sound");
            if (!restartNoiseSound.Playing)
                restartNoiseSound.Play();
            restartNoiseSound.VolumeDb = -5 + G.ResetTimer * 10;


            LimitsExpansion = new Vector2(_random.Next(-50, 50) * G.ResetTimer, _random.Next(-50, 50) * G.ResetTimer);
            Position = new Vector2(LimitsExpansion.X, LimitsExpansion.Y);
            SetTheLimitsAddition(true, LimitsExpansion.Y, LimitsExpansion.X, LimitsExpansion.Y, LimitsExpansion.X);
        }
        #endregion
        #region When reset interrupts
        else if (_restartNoise.IsPlaying())
        {
            _restartNoise.Stop();
            _restartNoise.Modulate = new Color(_restartNoise.Modulate.R, _restartNoise.Modulate.G, _restartNoise.Modulate.B, 0);

            _restartNoise.GetNode<AudioStreamPlayer>("Sound").Stop();
            SetTheLimitsAddition();
            LimitsExpansion = Vector2.Zero;
        }
        #endregion



        if (G.IsPlayerDead) // When player dead
        {
            float zoom = G.PlayerCorpseFlightTimer < 4 ? Meta.Instance.Video.CameraZoom + G.PlayerCorpseFlightTimer * ((4.5f - Meta.Instance.Video.CameraZoom) / 4) : 4.5f;
            Zoom = new Vector2(zoom, zoom);
            float PlayerCorpseFlightTimerX50 = G.PlayerCorpseFlightTimer * 50;
            SetTheLimitsAddition(false, -PlayerCorpseFlightTimerX50 - LimitsExpansion.Y, PlayerCorpseFlightTimerX50 + LimitsExpansion.X, PlayerCorpseFlightTimerX50 + LimitsExpansion.Y, -PlayerCorpseFlightTimerX50 - LimitsExpansion.X);

            if (G.PlayerCorpseFlightTimer >= 4.5f)
            {
                if (G.IsNewRecordReached)
                {
                    const string link = "GUICanvas/GUI/EmergingElements/NewRecordScores";
                    var newRecordScores = GetNode<Label>(link);
                    newRecordScores.Text = Tr("New Record!\nScore: ") + (int)G.Scores;
                    newRecordScores.Visible = true;
                    GetNode<CpuParticles2D>(link + "/Shine1").Emitting = true;
                    GetNode<CpuParticles2D>(link + "/Shine2").Emitting = true;
                }
                else
                {
                    var emergingScores = GetNode<Label>("GUICanvas/GUI/EmergingElements/Scores");
                    emergingScores.Visible = true;
                    emergingScores.Text = Convert.ToString(Tr("Score: ") + (int)G.Scores);
                }

                var emergingElements = GetNode<Node2D>("GUICanvas/GUI/EmergingElements");
                if (emergingElements.Modulate.A < 1)
                    emergingElements.Modulate = new Color(emergingElements.Modulate.R, emergingElements.Modulate.G, emergingElements.Modulate.B, emergingElements.Modulate.A + 0.005f);

                void SetHoldIMG(string ImageName)
                {
                    GetNode<RichTextLabel>("GUICanvas/GUI/EmergingElements/Hold R").Text = "[center]Hold [img]res://Content/Sprites/Interface/ControllerButtons/" + ImageName + ".png[/img]";
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

    private void OnCameraLimitsChanged(Vector4 limits)
    {
        bool DoResetSmoothing = GetScreenCenterPosition().DistanceTo(_player.GlobalPosition) > 1280;
        UpdateTheLimits(DoResetSmoothing);
    }
    #region Death
    private class PreDeathParams
    {
        Camera camera;
        public bool HasSavedParams = false;
        public PreDeathParams(Camera thisCamera)
        {
            camera = thisCamera;
        }

        public Vector2 Zoom;

        public Color EmergingElementsModulate;

        public bool PositionSmoothingEnabled;

        public bool NewRecordScoresVisible;
        public bool EmergingScoresVisible;

        public bool GoldenParticles1Emitting;
        public bool GoldenParticles2Emitting;


        public void SaveParams()
        {
            Zoom = camera.Zoom;

            EmergingElementsModulate = camera.GetNode<Node2D>("GUICanvas/GUI/EmergingElements").Modulate;

            PositionSmoothingEnabled = camera.PositionSmoothingEnabled;

            const string NEW_RECORD_SCORES_LINK = "GUICanvas/GUI/EmergingElements/NewRecordScores";
            NewRecordScoresVisible = camera.GetNode<Label>(NEW_RECORD_SCORES_LINK).Visible;
            EmergingScoresVisible = camera.GetNode<Label>("GUICanvas/GUI/EmergingElements/Scores").Visible;

            GoldenParticles1Emitting = camera.GetNode<CpuParticles2D>(NEW_RECORD_SCORES_LINK + "/Shine1").Emitting;
            GoldenParticles2Emitting = camera.GetNode<CpuParticles2D>(NEW_RECORD_SCORES_LINK + "/Shine2").Emitting;

            HasSavedParams = true;
        }

        public void LoadParams()
        {
            if (!HasSavedParams) return;

            camera.GetNode<Node2D>("GUICanvas/GUI/EmergingElements").Modulate = EmergingElementsModulate;

            camera.Zoom = Zoom;

            camera.PositionSmoothingEnabled = PositionSmoothingEnabled;

            const string NEW_RECORD_SCORES_LINK = "GUICanvas/GUI/EmergingElements/NewRecordScores";
            camera.GetNode<Label>(NEW_RECORD_SCORES_LINK).Visible = NewRecordScoresVisible;
            camera.GetNode<Label>("GUICanvas/GUI/EmergingElements/Scores").Visible = EmergingScoresVisible;

            camera.GetNode<CpuParticles2D>(NEW_RECORD_SCORES_LINK + "/Shine1").Emitting = GoldenParticles1Emitting;
            camera.GetNode<CpuParticles2D>(NEW_RECORD_SCORES_LINK + "/Shine2").Emitting = GoldenParticles2Emitting;
        }
    }
    PreDeathParams preDeathParams;

    public void OnPlayerDead()
    {
        preDeathParams.SaveParams();

        PositionSmoothingEnabled = false;
        _scores.Visible = false;
        GetNode<TextureProgressBar>("GUICanvas/GUI/StandBar").Visible = false;
    }

    public void OnPlayerResurrected()
    {
        SetTheLimitsAddition();
        ResetSmoothing();

        ApplyGUIOptions(false);

        preDeathParams.LoadParams();
    }
    //
    #endregion


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
