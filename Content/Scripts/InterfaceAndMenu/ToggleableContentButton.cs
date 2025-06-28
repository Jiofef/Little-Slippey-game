using Godot;
using System;

public partial class ToggleableContentButton : ContentButton
{
	public override void HandleActivate()
	{
		base.HandleActivate();

		if (_content is IToggleableContent tContent)
		{
			tContent.IsActivated = ButtonPressed;
			ContentManager.QueueSave();
		}
	}

}
