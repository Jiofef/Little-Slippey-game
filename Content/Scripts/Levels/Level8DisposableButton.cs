using Godot;

public partial class Level8DisposableButton : Area2D
{
	[Signal] public delegate void ButtonPressedEventHandler();
	
	public void AreaEntered()
	{
		EmitSignal("ButtonPressed");
	}
}
