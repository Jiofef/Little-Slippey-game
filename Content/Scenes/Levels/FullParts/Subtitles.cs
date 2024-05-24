using Godot;
using System;

public partial class Subtitles : RichTextLabel
{
    private string TextToDraw = "";
    private float TimeToDraw = 0;
	private float Timer = 0;
	public override void _Ready()
	{
		SetPhysicsProcess(false);
	}

	public override void _PhysicsProcess(double delta)
	{
		Timer += 0.01667f;
		if (TimeToDraw > 0)
			VisibleRatio = Timer / TimeToDraw;
		if (Timer > TimeToDraw)
			SetPhysicsProcess(false);
	}
	public void ShowText(string TextToDraw, float TimeToDraw = 1)
	{
		Timer = 0;
        this.TimeToDraw = TimeToDraw;
		this.TextToDraw = TextToDraw;
        Text = TextToDraw;
        SetPhysicsProcess(true);
    }
	public void ClearText()
	{
		TextToDraw = "";
		TimeToDraw = 0;
		Text = "";
		Timer = 0;
		SetPhysicsProcess(false);
	}
}
