using Godot;
using System;
using OtherExtension;
using static ModDataManager;

public partial class ModOptionsGroupEdit : MarginContainer
{
    [Signal] public delegate void MakeAnOptionButtonPressedEventHandler();

    public TreeNode<object> GroupTreeItem;
    public ModOptionGroup Group;

    /// <summary>
    /// Needed for correct work
    /// </summary>
    public void SetModOptionGroupTreeItem(TreeNode<object> groupTreeItem)
    {
        GroupTreeItem = groupTreeItem;
        Group = (ModOptionGroup)GroupTreeItem.Value;

        GetNode<LineEdit>("VBoxContainer/OptionNameEdit").Text = Group.Name;
    }

    private void OnMakeAnOptionButtonPressed()
    {
        EmitSignal("MakeAnOptionButtonPressed");
    }

    private void SetGroupName(string value)
    {
        Group.Name = value;
    }

    public ModOptionEdit CreateAnOption()
    {
        // Creating an empty option in the options tree
        var optionDataItem = GroupTreeItem.AddChild(new ModOptionData());

        var modOptionEdit = (ModOptionEdit)GD.Load<PackedScene>("res://Content/Scenes/Interface&Menu/ModOptionEdit.tscn").Instantiate();
        modOptionEdit.SetModOptionDataTreeItem(optionDataItem);

        GetNode("VBoxContainer/MarginC/OptionsContainer").AddChild(modOptionEdit);
        return modOptionEdit;
    }
    public ModOptionEdit CreateAnOption(TreeNode<object> node)
    {
        var modOptionEdit = (ModOptionEdit)GD.Load<PackedScene>("res://Content/Scenes/Interface&Menu/ModOptionEdit.tscn").Instantiate();
        modOptionEdit.SetModOptionDataTreeItem(node);


        GetNode("VBoxContainer/MarginC/OptionsContainer").AddChild(modOptionEdit);
        return modOptionEdit;
    }

    private void RemoveGroup()
    {
        GroupTreeItem.Free();
        QueueFree();
    }

    //public void CreateAChildGroup()
    //{

    //}
}
