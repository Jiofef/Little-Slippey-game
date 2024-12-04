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
    
    private string _description = "Are you sure?";

    private void UpdateDescrtiption()
    {
        GetNode<RichTextLabel>("MarginContainer/VBoxContainer/Description").Text = _description;
    }

    private void Accept()
    {
        EmitSignal("Result", true);
        EmitSignal("Accepted");
        QueueFree();
    }
    private void Decline()
    {
        EmitSignal("Result", false);
        EmitSignal("Declined");
        QueueFree();
    }
}
