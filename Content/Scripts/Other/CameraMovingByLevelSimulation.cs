using Godot;

public partial class CameraMovingByLevelSimulation : PathFollow2D
{
	private bool _doMotionReversed;
	private float _stopTimer;
	public override void _Ready()
	{
		ProgressRatio = 0.001f;
	}

	public override void _Process(double delta)
	{
        if (_stopTimer <= 0)
		{
			Progress += !_doMotionReversed ? 1.5f : -1.5f;
			if (ProgressRatio == 0)
			{
				_stopTimer = 1.5f;
				_doMotionReversed = false;
			}
			else if (ProgressRatio == 1)
			{
                _stopTimer = 1.5f;
                _doMotionReversed = true;
			}
		}
		else
			_stopTimer -= 0.016667f;

	}
}
