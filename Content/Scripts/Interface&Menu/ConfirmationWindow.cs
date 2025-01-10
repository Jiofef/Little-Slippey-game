using Godot;
using System;

[Tool]
public partial class ConfirmationWindow : DraggableWindow
{
    [Signal] public delegate void ResultEventHandler(bool yesOrNot);
    [Signal] public delegate void AcceptedEventHandler();
    [Signal] public delegate void DeclinedEventHandler();


    [Export] public string Description
    {
        get => _description;
        set
        {
            _description = value;
            CallDeferred("UpdateDescrtiption");
        }
    }
    [Export] public bool ReturnTheFocusToPreviousOwnerWhenAccepted = true;
    
    private string _description = "Are you sure?";

    public override void _Ready()
    {
        _previousFocusOwner = GetViewport().GuiGetFocusOwner();
        GetNode<TextureButton>("MarginContainer/VBoxContainer/HBoxContainer/AcceptButton").GrabFocus();
    }

    private void UpdateDescrtiption()
    {
        GetNode<RichTextLabel>("MarginContainer/VBoxContainer/Description").Text = _description;
    }

    public virtual void Accept()
    {
        EmitSignal("Result", true);
        EmitSignal("Accepted");
        QueueFree();

        if (ReturnTheFocusToPreviousOwnerWhenAccepted)
            _previousFocusOwner.GrabFocus();
    }
    public void Decline()
    {
        EmitSignal("Result", false);
        EmitSignal("Declined");
        QueueFree();

        _previousFocusOwner.GrabFocus();
    }
}
