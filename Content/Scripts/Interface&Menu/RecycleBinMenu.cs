using Godot;
using System;

public partial class RecycleBinMenu : Control
{
	private string _openedFile = "";
    public override void _Ready()
    {
        GetNode<Sprite2D>("FileBoxes/GratitudeDiaryTxt/ILoveYouAllGuys<3").Position = new Vector2(Convert.ToInt32(Tr("GratitudeDiaryHeartXPos")), 48);
        if (UnchangableMeta.AchievementStatuses[48] == 0)
        {
            string[] NodesToHideNames = { "TheTestIsOverTxt", "GratitudeDiaryTxt", "Dem01", "Dem02", "Dem03", "Dem04", "Dem05", "Memory1", "Memory2", "Memory3", "Memory4", "Eternity"};
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

    public void GiveRoyaltyAchievement()
    {
        G.GetAchievement(9);
    }
}
