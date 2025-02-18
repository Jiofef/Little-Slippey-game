using Godot;
using System;

public partial class Camera : Camera2D
{
    // Needed nodes
    AnimatedSprite2D _restartNoise;
    InGameGui _gui;


    Random _random = new Random();

    // The number of pixels visible outside the exposed limits of the camera. X = top, Y = right, Z = bottom, W = left
    [Export] public Rect2 ViewAngleAddition = new Rect2(-30, -30, 30, 100);

    private Rect2 _limitsAddition = new Rect2(0, 0, 0, 0);
    [Export] public Rect2 LimitsAddition 
    {
        get => _limitsAddition;
        set => SetTheLimitsAddition(false, value.Position.Y, value.End.X, value.End.Y, value.Position.X);
    }

    public override void _Ready()
    {
        //Initializing nodes
        _restartNoise = GetNode<AnimatedSprite2D>("GUICanvas/RestartNoise");
        _gui = GetNode<InGameGui>("GUICanvas/GUI");


        G.CameraLimits = new Rect2(0, 0, G.LevelXYSizes[G.CurrentLevel].X, G.LevelXYSizes[G.CurrentLevel].Y);
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
        _limitsAddition = new Rect2(plus4, plus1, plus3, plus2);

        Rect2 Defaultlimits = new Rect2();
        Defaultlimits.Position = G.CameraLimits.Position + ViewAngleAddition.Position;
        Defaultlimits.End = G.CameraLimits.End + ViewAngleAddition.Size;

        LimitLeft = (int)(Defaultlimits.Position.X + plus4);
        LimitTop = (int)(Defaultlimits.Position.Y + plus1);
        LimitBottom = (int)(Defaultlimits.End.Y + plus3);
        LimitRight = (int)(Defaultlimits.End.X + plus2);

        if (DoResetSmoothing)
            ResetSmoothing();
    }
    private void UpdateTheLimits(bool DoResetSmoothing = false)
    {
        SetTheLimitsAddition(false, 0, 0, 0, 0);
    }
    public override void _PhysicsProcess(double delta)
    {
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
        }
    }

    private void OnCameraLimitsChanged(Rect2 limits)
    {
        bool DoResetSmoothing = GetScreenCenterPosition().DistanceTo(G.Player.GlobalPosition) > 1280;
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

        public bool PositionSmoothingEnabled;


        public void SaveParams()
        {
            Zoom = camera.Zoom;

            PositionSmoothingEnabled = camera.PositionSmoothingEnabled;

            HasSavedParams = true;
        }

        public void LoadParams()
        {
            if (!HasSavedParams) return;

            camera.Zoom = Zoom;

            camera.PositionSmoothingEnabled = PositionSmoothingEnabled;
        }
    }
    PreDeathParams preDeathParams;

    public void OnPlayerDead()
    {
        preDeathParams.SaveParams();

        PositionSmoothingEnabled = false;
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
            Zoom = new Vector2(Meta.Instance.Video.CameraZoom, Meta.Instance.Video.CameraZoom);
        }
        else
        {
            float zoom = G.PlayerCorpseFlightTimer < 4 ? Meta.Instance.Video.CameraZoom + G.PlayerCorpseFlightTimer * ((4.5f - Meta.Instance.Video.CameraZoom) / 4) : 4.5f;
            Zoom = new Vector2(zoom, zoom);
        }

        _gui.UpdateAllTheOptions();
    }
}
