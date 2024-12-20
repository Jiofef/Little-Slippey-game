using Godot;
using System;
using System.Linq;

public partial class Subtitles : RichTextLabel
{
	[Signal] public delegate void TextAppearingEventHandler();
	[Signal] public delegate void TextClearingEventHandler();

    //I have made these fields public so that their values can be saved after the level is reloaded.
    public string TextToDraw = "";
    public float TimeToDraw = 0;
    public float Timer = 0;

    public string[] TextsQueue;
    public float[] TextTimeCodes;
    public bool IsTextQueued = false;
    public float TextSavingTime = 0;
    public int CurrentQueueNumber = -1;
	//

    public override void _Ready()
	{
		SetPhysicsProcess(false);
	}

	public override void _PhysicsProcess(double delta)
	{
		Timer += 0.01667f;
		if (IsTextQueued)
		{
			if (CurrentQueueNumber < TextsQueue.Length && CurrentQueueNumber < TextTimeCodes.Length)
			{
                if (Timer > TextTimeCodes[CurrentQueueNumber + 1])
                {
                    CurrentQueueNumber++;
                    if (CurrentQueueNumber < TextsQueue.Length)
                        Text = TextsQueue[CurrentQueueNumber];
                    VisibleRatio = 0;
                }

                if (VisibleRatio < 1 && CurrentQueueNumber < TextsQueue.Length)
				{
					if (CurrentQueueNumber >= 0 && TextTimeCodes[CurrentQueueNumber + 1] - TextTimeCodes[CurrentQueueNumber] - TextSavingTime > 0)
						VisibleRatio = (Timer - TextTimeCodes[CurrentQueueNumber]) / (TextTimeCodes[CurrentQueueNumber + 1] - TextTimeCodes[CurrentQueueNumber] - TextSavingTime);
					else VisibleRatio = 1;
				}
            }
			else if (Timer < TextTimeCodes.Last() - TextSavingTime)
				ClearText();
		}
		else
		{
            if (TimeToDraw > 0 && VisibleRatio < 1)
                VisibleRatio = Timer / TimeToDraw;
            if (Timer > TimeToDraw + TextSavingTime)
                ClearText();
        }
	}
	public void ShowText(string TextToDraw, float TimeToDraw = 1)
	{
		Timer = 0;
        this.TimeToDraw = TimeToDraw;
		this.TextToDraw = TextToDraw;
        Text = TextToDraw;
		EmitSignal("TextAppearing");

        SetPhysicsProcess(true);
    }
	public void ClearText()
	{
		TextToDraw = "";
		TimeToDraw = 0;
		Text = "";
		Timer = 0;
		TextsQueue = null;
		TextTimeCodes = null;
		TextSavingTime = 0;
		IsTextQueued = false;
		CurrentQueueNumber = -1;
		EmitSignal("TextClearing");

        SetPhysicsProcess(false);
	}
	public void ShowTextQueue(string[] TextsQueue, float[] TextTimeCodes, float TextSavingTime = 0)
	{
        Timer = 0;
		this.TextsQueue = TextsQueue;
		this.TextTimeCodes = TextTimeCodes;
		this.TextSavingTime = TextSavingTime;
		IsTextQueued = true;
        CurrentQueueNumber = -1;
        EmitSignal("TextAppearing");

        SetPhysicsProcess(true);
    }
}
