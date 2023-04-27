using Godot;

public partial class CameraMovingByLevelSimulation : PathFollow2D
{
	[Export] int _levelNumber = 0;
	

	private bool _doMotionReversed;
	private float _stopTimer;
	private int _stopCiclePhase = 0;
	public override void _Ready()
	{
		ProgressRatio = 0.001f;
	}

	public override void _Process(double delta)
	{
        if (_stopTimer <= 0)
		{
			Progress += !_doMotionReversed ? 1.5f: -1.5f;
			if (G.LevelXYSizes[_levelNumber] == new Vector2(2560, 1280))
			{
				float[] StopPositions = {0.3597f, 0.5f, 0.8597f, 0.999f};
				if (ProgressRatio >= StopPositions[_stopCiclePhase])
				{
					_stopTimer = 1.5f;
					if (_stopCiclePhase == 3)
					{
						_stopCiclePhase = 0;
						Progress = 0.001f;
					}
					else _stopCiclePhase++;
				}
			}
			else if (ProgressRatio == 0)
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
