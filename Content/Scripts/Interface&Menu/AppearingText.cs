using Godot;
using System;

public partial class AppearingText : RichTextLabel
{
	public const float DEFAULT_CHARS_PER_SEC = 15;
	[Export] public float CharactersPerSecond = DEFAULT_CHARS_PER_SEC;
	[Export] public bool AutoStart = false, PopSound = true, AppearSound = true;
	private bool _appearing = false;
	[Export] public bool Appearing 
	{
		get => _appearing;
		set { SetAppearing(value); }
	}
	private double _charactersToShow = 0; //This is not the number of characters in the text, but the number of characters to show in the next frame
    private string _currentText;

    public override void _Ready()
    {
		VisibleCharacters = 0;
		if (AutoStart)
			SetAppearing(true);
		_currentText = Tr(Text);
    }

    public override void _Process(double delta)
	{
		if (!_appearing) return;

		_charactersToShow += delta * CharactersPerSecond;
		
		if (_charactersToShow >= 1)
		{
            int IntLettersToShow = (int)(_charactersToShow - _charactersToShow % 1);
            VisibleCharacters += IntLettersToShow;
            _charactersToShow -= IntLettersToShow;

			if (PopSound)
				GetNode<AudioStreamPlayer>("Pop").Play();

			if (VisibleCharacters >= _currentText.Length) // Stopping appearing when all text is shown
				Appearing = false;
		}
	}

	public void SetAppearing(bool value)
	{
        _appearing = value;

		if (value == true && AppearSound)
			GetNode<AudioStreamPlayer>("Appear").Play();
	}

	public void SkipAppearing()
	{
        VisibleCharacters = _currentText.Length;
		Appearing = false;

        _charactersToShow = 0;
    }



	public void SetText(string value, float charactersPerSecond = DEFAULT_CHARS_PER_SEC)
	{
		VisibleCharacters = 0;
		SetAppearing(true);

        Text = value;
        _currentText = Tr(value);

		CharactersPerSecond = charactersPerSecond;
	}
	public void SetText(PhraseProperties properties)
	{
		SetText(properties.Text, properties.CharsPerSecond);
    }

    public class PhraseProperties
    {
        public string Text;
        public float CharsPerSecond;

        public PhraseProperties(string text, float charsPerSecond = DEFAULT_CHARS_PER_SEC)
        {
            Text = text;
            CharsPerSecond = charsPerSecond;
        }
    }
}
