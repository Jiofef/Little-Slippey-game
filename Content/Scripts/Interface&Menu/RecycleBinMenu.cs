using Godot;
using System;

[Tool]
public partial class RecycleBinMenu : DraggableWindow
{
	private string _openedFile = "";

    public override void _Ready()
    {
        UnchangableMeta.IsThereNewContentInRecycleBin = false;
        UnchangableMeta.SaveToFile();
        GetNode<Sprite2D>("FileBoxes/GratitudeDiaryTxt/ILoveYouAllGuys<3").Position = new Vector2(Convert.ToInt32(Tr("GratitudeDiaryHeartXPos")), Convert.ToInt32(Tr("GratitudeDiaryHeartYPos")));
        var icons = GetNode("MarginContainer/VBoxContainer/Icons").GetChildren();
        foreach ( RecycleBinIcon icon in icons )
        {
            if (icon.iconType == RecycleBinIcon.IconType.File)
                icon.Pressed += () => OpenFile(icon.Link);
            else if (icon.iconType == RecycleBinIcon.IconType.Link)
            {
                icon.Pressed += () =>
                {
                    OpenLink(icon.Link);
                    GiveRoyaltyAchievement();
                };
            }
        }
        if (!Achievements.AllTheAchievements["you were deceived."].IsReceived)
        {
            string[] NodesToHideNames = { "TheTestIsOverTxt", "GratitudeDiaryTxt", "Dem01", "Dem02", "Dem03", "Dem04", "Dem05", "Memory1", "Memory2", "Memory3", "Memory4", "Eternity"};
            for (int i = 0; i < NodesToHideNames.Length; i++)
                GetNode("MarginContainer/VBoxContainer/Icons/" + NodesToHideNames[i]).QueueFree();
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
        Achievements.GetAchievement("At least I got to hobnob with royalty");
    }
}
