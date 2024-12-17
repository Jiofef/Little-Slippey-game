using Godot;
using System;

public partial class AppearingText : RichTextLabel
{
	[Export] public float CharactersPerSecond = 15;
	[Export] public bool AutoStart = false, PopSound = true, AppearSound = true;
	private bool _appearing = false;
	[Export] public bool Appearing 
	{
		get => _appearing;
		set { SetAppearing(value); }
	}
	private double _charactersToShow = 0;

    public override void _Ready()
    {
		VisibleCharacters = 0;
		if (AutoStart)
			SetAppearing(true);
    }

    public override void _Process(double delta)
	{
		if (!_appearing) return;

		_charactersToShow += delta * CharactersPerSecond;
		
		if (_charactersToShow > 1)
		{
            int IntLettersToShow = (int)(_charactersToShow - _charactersToShow % 1);
            VisibleCharacters += IntLettersToShow;
            _charactersToShow -= IntLettersToShow;

			if (PopSound)
				GetNode<AudioStreamPlayer>("Pop").Play();

			if (VisibleCharacters >= Text.Length) // Stopping appearing when all text is shown
				Appearing = false;
		}
	}

	public void SetAppearing(bool value)
	{
        _appearing = value;

		if (value == true && AppearSound)
			GetNode<AudioStreamPlayer>("Appear").Play();
	}
}
