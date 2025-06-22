using Godot;
using System;
using System.Runtime;
using static OtherExtension.GeometryTools;

public partial class Camera : Camera2D
{
    // Needed nodes
    AnimatedSprite2D _restartNoise;
    InGameGui _gui;


    Random _random = new Random();

	[ExportGroup("Alternative Smoothing")]
	[Export] public bool AlternativeSmoothingEnabled = true, AlternativeLimitSmoothed = true;
	[Export] public Node2D CameraTarget;

	[Export] public Vector2 TargetOffset = Vector2.Zero;

	[ExportGroup("Camera limits additions")]
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

		SmoothedPosition = GlobalPosition;

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

	public Rect2 GetLimitsRect()
	{
    	return new Rect2(LimitLeft, LimitTop, LimitRight - LimitLeft, LimitBottom - LimitTop);
	}


    private void SetTheLimitsAddition(bool DoResetSmoothing = false, float plus1 = 0, float plus2 = 0, float plus3 = 0, float plus4 = 0)
    {
        _limitsAddition = new Rect2(plus4, plus1, plus3, plus2);

        Rect2 DefaultLimits = new Rect2();
        DefaultLimits.Position = G.CameraLimits.Position + ViewAngleAddition.Position;
        DefaultLimits.End = G.CameraLimits.End + ViewAngleAddition.Size;

        LimitLeft = (int)(DefaultLimits.Position.X + plus4);
        LimitTop = (int)(DefaultLimits.Position.Y + plus1);
        LimitBottom = (int)(DefaultLimits.End.Y + plus3);
        LimitRight = (int)(DefaultLimits.End.X + plus2);

        if (DoResetSmoothing)
            ResetSmoothing();
    }
    private void UpdateTheLimits(bool DoResetSmoothing = false)
    {
        SetTheLimitsAddition(false, 0, 0, 0, 0);
    }

	Vector2 _limitsExpansion, _targetPos;
	public Vector2 SmoothedPosition;
    public override void _PhysicsProcess(double delta)
    {
        _limitsExpansion = Vector2.Zero;
		
		// Alternative smoothing
		if (AlternativeSmoothingEnabled && CameraTarget != null)
		{
        	_targetPos = CameraTarget.GlobalPosition + TargetOffset;

			if (AlternativeLimitSmoothed)
				ApplyLimitSmoothing();

        	SmoothedPosition = SmoothedPosition.Lerp(_targetPos, (float)(delta * PositionSmoothingSpeed));

			GlobalPosition = SmoothedPosition;
        }

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


            _limitsExpansion = new Vector2(_random.Next(-50, 50) * G.ResetTimer, _random.Next(-50, 50) * G.ResetTimer);
            Offset = new Vector2(_limitsExpansion.X, _limitsExpansion.Y);
            SetTheLimitsAddition(true, _limitsExpansion.Y, _limitsExpansion.X, _limitsExpansion.Y, _limitsExpansion.X);
        }
        #endregion
        #region When reset interrupts
        else if (_restartNoise.IsPlaying())
        {
            _restartNoise.Stop();
            _restartNoise.Modulate = new Color(_restartNoise.Modulate.R, _restartNoise.Modulate.G, _restartNoise.Modulate.B, 0);

            _restartNoise.GetNode<AudioStreamPlayer>("Sound").Stop();
            SetTheLimitsAddition();
            _limitsExpansion = Vector2.Zero;
        }
        #endregion



        if (G.IsPlayerDead) // When player dead
        {
            float zoom = G.PlayerCorpseFlightTimer < 4 ? Meta.Instance.Video.CameraZoom + G.PlayerCorpseFlightTimer * ((4.5f - Meta.Instance.Video.CameraZoom) / 4) : 4.5f;
            Zoom = new Vector2(zoom, zoom);
            float PlayerCorpseFlightTimerX50 = G.PlayerCorpseFlightTimer * 50;
            SetTheLimitsAddition(false, -PlayerCorpseFlightTimerX50 - _limitsExpansion.Y, PlayerCorpseFlightTimerX50 + _limitsExpansion.X, PlayerCorpseFlightTimerX50 + _limitsExpansion.Y, -PlayerCorpseFlightTimerX50 - _limitsExpansion.X);
        }
    }

 	private void ApplyLimitSmoothing()
    {
		Vector2 screenSize = GetViewportRect().Size;

		Vector2 screenHalf = screenSize / 2 / Zoom;

		// If camera limits is too small
		if (LimitLeft + screenSize.X > LimitRight)
			screenHalf.X = (LimitRight - LimitLeft) / 2;
		if (LimitTop + screenSize.Y > LimitBottom)
			screenHalf.Y = (LimitBottom - LimitTop) / 2;

		_targetPos.X = Math.Clamp(_targetPos.X, LimitLeft + screenHalf.X, LimitRight - screenHalf.X);
		_targetPos.Y = Math.Clamp(_targetPos.Y, LimitTop + screenHalf.Y, LimitBottom - screenHalf.Y);
    }

	new public void ResetSmoothing()
	{
		base.ResetSmoothing();

		if (CameraTarget != null && AlternativeSmoothingEnabled)
		{
			GlobalPosition = CameraTarget.GlobalPosition;
			SmoothedPosition = GlobalPosition;
		}
	}
	private Vector2 ClampToLimit(Vector2 pos)
	{
		var limit = GetLimitsRect();
		var screenSize = GetViewportRect().Size * new Vector2(1 / Zoom.X, 1 / Zoom.Y);
		Vector2 screenOffset = (AnchorMode == AnchorModeEnum.DragCenter) ? (screenSize * 0.5f) : Vector2.Zero;

		float minX = limit.Position.X + screenOffset.X;
		float maxX = limit.End.X - screenOffset.X;
		float minY = limit.Position.Y + screenOffset.Y;
		float maxY = limit.End.Y - screenOffset.Y;

		return new Vector2(
			Mathf.Clamp(pos.X, minX, maxX),
			Mathf.Clamp(pos.Y, minY, maxY)
		);
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

        public bool AlternativeSmoothingEnabled, AlternativeLimitSmoothed;


        public void SaveParams()
        {
            HasSavedParams = true;

			// Params
            Zoom = camera.Zoom;

            AlternativeSmoothingEnabled = camera.AlternativeSmoothingEnabled;

			AlternativeLimitSmoothed = camera.AlternativeLimitSmoothed;

        }

        public void LoadParams()
        {
            if (!HasSavedParams) return;

            camera.Zoom = Zoom;

            camera.AlternativeSmoothingEnabled = AlternativeSmoothingEnabled;

			camera.AlternativeLimitSmoothed = AlternativeLimitSmoothed;
        }
    }
    PreDeathParams preDeathParams;

    public void OnPlayerDead()
    {
        preDeathParams.SaveParams();

		Position = Vector2.Zero;
        AlternativeSmoothingEnabled = false;
		AlternativeLimitSmoothed = false;

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
