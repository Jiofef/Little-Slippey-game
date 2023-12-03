using Godot;
using System;

public partial class RecycleBinMenu : Control
{
	private string _openedFile = "";
    public override void _Ready()
    {
        if (UnchangableMeta.LevelCompleteStatus[9] == 0)
        {
            string[] NodesToHideNames = { "GratitudeDiaryTxt", "GratitudeDiaryTxtLabel", "TheTestIsOverTxt", "TheTestIsOverTxtLabel" };
            for (int i = 0; i < NodesToHideNames.Length; i++)
                GetNode("Icons/" + NodesToHideNames[i]).QueueFree();
        }
    }
    public void OpenFile(string FileName)
	{
		if (_openedFile == "")
        {
            GetNode<AnimationPlayer>("FileBoxes/" + FileName + "/AnimationPlayer").Play("Opening");
            GetNode<Node2D>("FileBoxes/" + FileName).Visible = true;
            _openedFile = FileName;
		}
		else if (_openedFile == FileName)
		{
            CloseFile();
        }
		else
		{
            GetNode<AnimationPlayer>("FileBoxes/" + FileName + "/AnimationPlayer").Play("Opening");
			GetNode<Node2D>("FileBoxes/" + _openedFile).Visible = false;
            GetNode<Node2D>("FileBoxes/" + FileName).Visible = true;
            _openedFile = FileName;
        }
	}
	public void CloseFile()
	{
        GetNode<Node2D>("FileBoxes/" + _openedFile).Visible = false;
        _openedFile = "";
    }
	public void OpenLink(string link)
	{
		OS.ShellOpen(link);
	}
}
