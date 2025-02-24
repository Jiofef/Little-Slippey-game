using Godot;
using System;
using System.Runtime.CompilerServices;

public partial class SwooshObject : Path2D
{
	// Signals
	[Signal] public delegate void ObjectAppearedEventHandler();
	[Signal] public delegate void ObjectDisappearingEventHandler();
	[Signal] public delegate void ObjectDisappearedEventHandler();

	// Properties
	private bool _oneShot = true;
	[Export] public bool OneShot 
	{
		get => _oneShot;
		set
		{
			_oneShot = value;
			CallDeferred(nameof(UpdateOneShot));
		}

	}
	private void UpdateOneShot()
	{
		if (_oneShot)
		{
			GetNode<PathFollow2D>("PathFollow2D").Loop = !_oneShot;
        }
	}

	[ExportGroup("Moving")]
	[Export] public float Speed = 1f;
	[Export] public bool IsSpeedRatio = false;
    [Export] public Curve SpeedCurve;

    [ExportGroup("DisAppearing")]
    [Export] public float ZDisappearingScaleCoeff = 2f;

	[Export] public Curve DisAppearingCurve;

	[Export] bool IsFromForeground = false;
	[Export] float DisAppearingDirectionRandomness = 0f;


    // Nodes
	public PathFollow2D PathFollow;

	// Other variables
	public float AppearedCoeff => DisAppearingCurve.SampleBaked(PathFollow.ProgressRatio);
	private bool _isInTheForeground = false;
	public bool IsInTheForeground { get => _isInTheForeground; }

	public override void _Ready()
	{
        PathFollow = GetNode<PathFollow2D>("PathFollow2D");
		PathFollowScale = PathFollow.Scale;

		RandomizeDisAppearingDirection();

		UpdateZPos();
    }

	public void RandomizeDisAppearingDirection()
	{
        if (OtherExtension.RandomTools.TryRand(DisAppearingDirectionRandomness / 2f, 1f))
            IsFromForeground = !IsFromForeground;
    }


    public override void _PhysicsProcess(double delta)
    {
		MoveProcess();
    }

	public bool IsStopped = false;
	protected void MoveProcess()
	{
		if (IsStopped) return;

		float currentSpeed = SpeedCurve != null 
			? SpeedCurve.Sample(PathFollow.ProgressRatio) * Speed 
			: Speed;

		if (IsSpeedRatio)
			PathFollow.ProgressRatio += currentSpeed;
		else
			PathFollow.Progress += currentSpeed;

		UpdateZPos();

		if (PathFollow.ProgressRatio >= 1f)
		{
            EmitSignal(nameof(ObjectDisappeared));
            CallDeferred(nameof(OnObjectDisappeared));

            if (OneShot)
				Finish();
        }
	}

	private Vector2 PathFollowScale;
	public void UpdateZPos()
	{
		float coeff = AppearedCoeff;

		const float ONE_THRESHOLD = 0.999f;
        if (coeff >= ONE_THRESHOLD && !IsInTheForeground)
        {
            _isInTheForeground = true;
            EmitSignal(nameof(ObjectAppeared));
            CallDeferred(nameof(OnObjectAppeared));

            RandomizeDisAppearingDirection();
        }
        else if (coeff < ONE_THRESHOLD && IsInTheForeground)
        {
            _isInTheForeground = false;
            EmitSignal(nameof(ObjectDisappearing));

			CallDeferred(nameof(OnObjectDisappearing));

            RandomizeDisAppearingDirection();
        }

        PathFollow.Modulate = new Color(PathFollow.Modulate.R, PathFollow.Modulate.G, PathFollow.Modulate.B, coeff);

		if (!IsFromForeground)
			PathFollow.Scale = Vector2.One / ZDisappearingScaleCoeff + (PathFollowScale * coeff * (ZDisappearingScaleCoeff - 1) / ZDisappearingScaleCoeff);
		else
		{
			Vector2 disVec = new Vector2(ZDisappearingScaleCoeff, ZDisappearingScaleCoeff);
            PathFollow.Scale = disVec - PathFollowScale * coeff * (ZDisappearingScaleCoeff - 1);
        }

    }

	public void Finish()
	{
		QueueFree();
	}

	public virtual void OnObjectAppeared() { }

	public virtual void OnObjectDisappearing() { }

	public virtual void OnObjectDisappeared() { }
}
