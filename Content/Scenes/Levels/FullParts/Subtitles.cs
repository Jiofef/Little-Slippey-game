using Godot;
using System;
using System.Linq;

public partial class Subtitles : RichTextLabel
{
	[Signal] public delegate void TextAppearingEventHandler();
	[Signal] public delegate void TextClearingEventHandler();

    //I have made these fields public so that their values can be saved after the level is reloaded.
    public string _textToDraw = "";
    public float _timeToDraw = 0;
    public float _timer = 0;

    public string[] _textsQueue;
    public float[] _textTimeCodes;
    public bool _isTextQueued = false;
    public float _textSavingTime = 0;
    public int _currentQueueNumber = -1;
	//

    public override void _Ready()
	{
		SetPhysicsProcess(false);
	}

	public override void _PhysicsProcess(double delta)
	{
		_timer += 0.01667f;
		if (_isTextQueued)
		{
			if (_currentQueueNumber < _textsQueue.Length && _currentQueueNumber < _textTimeCodes.Length)
			{
                if (_timer > _textTimeCodes[_currentQueueNumber + 1])
                {
                    _currentQueueNumber++;
                    if (_currentQueueNumber < _textsQueue.Length)
                        Text = _textsQueue[_currentQueueNumber];
                    VisibleRatio = 0;
                }

                if (VisibleRatio < 1 && _currentQueueNumber < _textsQueue.Length)
				{
					if (_textTimeCodes[_currentQueueNumber + 1] - _textTimeCodes[_currentQueueNumber] - _textSavingTime > 0)
						VisibleRatio = (_timer - _textTimeCodes[_currentQueueNumber]) / (_textTimeCodes[_currentQueueNumber + 1] - _textTimeCodes[_currentQueueNumber] - _textSavingTime);
					else VisibleRatio = 1;
				}
            }
			else if (_timer < _textTimeCodes.Last() - _textSavingTime)
				ClearText();
		}
		else
		{
            if (_timeToDraw > 0 && VisibleRatio < 1)
                VisibleRatio = _timer / _timeToDraw;
            if (_timer > _timeToDraw + _textSavingTime)
                ClearText();
        }
	}
	public void ShowText(string TextToDraw, float TimeToDraw = 1)
	{
		_timer = 0;
        _timeToDraw = TimeToDraw;
		_textToDraw = TextToDraw;
        Text = TextToDraw;
		EmitSignal("TextAppearing");

        SetPhysicsProcess(true);
    }
	public void ClearText()
	{
		_textToDraw = "";
		_timeToDraw = 0;
		Text = "";
		_timer = 0;
		_textsQueue = null;
		_textTimeCodes = null;
		_textSavingTime = 0;
		_isTextQueued = false;
		_currentQueueNumber = -1;
		EmitSignal("TextClearing");

        SetPhysicsProcess(false);
	}
	public void ShowTextQueue(string[] TextsQueue, float[] TextTimeCodes, float TextSavingTime = 0)
	{
        _timer = 0;
		_textsQueue = TextsQueue;
		_textTimeCodes = TextTimeCodes;
		_textSavingTime = TextSavingTime;
		_isTextQueued = true;
        _currentQueueNumber = -1;
        EmitSignal("TextAppearing");

        SetPhysicsProcess(true);
    }
}
