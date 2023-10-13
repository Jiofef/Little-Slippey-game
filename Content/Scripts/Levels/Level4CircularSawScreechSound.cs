using Godot;

public partial class Level4CircularSawScreechSound : AudioStreamPlayer
{
	AudioStream[] _soundsQueue = new AudioStream[3];
	private int _sawCicle = 0;
	public override void _Ready()
	{
		for (int i = 0; i < _soundsQueue.Length; i++)
			_soundsQueue[i] = ResourceLoader.Load<AudioStream>("res://Content/Sounds/Levels/Level4CircularSawScreech" + (i + 1) + ".mp3");
	}

	public void PlayReccurentSound()
	{
        Stream = _soundsQueue[_sawCicle];

        _sawCicle = _sawCicle < 2 ? _sawCicle + 1 : 0;

		Play();
	}
}
