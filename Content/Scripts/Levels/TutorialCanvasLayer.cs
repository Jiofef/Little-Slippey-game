using Godot;

public partial class TutorialCanvasLayer : CanvasLayer
{
	public override void _Ready()
	{
		G.IsProgressPaused = true;
		G.IsCrossesEnabled = false;
		if (UnchangableMeta.IsTutorialPlayed)
            GetNode<AnimationPlayer>("Label/AnimationPlayer").Play("Disappearing");
		else
		{
			SetPhysicsProcess(false);
            UnchangableMeta.IsTutorialPlayed = true;
			UnchangableMeta.SaveToFile();
			GetNode<Label>("Label").QueueFree();
        }
	}
    public override void _PhysicsProcess(double delta)
    {
        if (Input.IsPhysicalKeyPressed(Key.F))
		{
			GetNode<AnimationPlayer>("../Rail/AnimationPlayer").SpeedScale = 5;
            GetNode<AudioStreamPlayer>("../Rail/Ambient").PitchScale = 5;
            SetPhysicsProcess(false);
        }
    }
	public void Glitch()
	{
		var whiteNoiseGlitch = GetNode<AnimatedSprite2D>("WhiteNoiseGlitch");
		whiteNoiseGlitch.Visible = true;
		whiteNoiseGlitch.Play();
		GetNode<AudioStreamPlayer>("WhiteNoiseGlitch/AudioStreamPlayer").Play();
    }
	public void TutorialCompleted()
	{
		G.IsCrossesEnabled = true;
		G.CrossSpawnMultiplier = 10;
	}
	public void PlayerDied()
	{
		G.LevelAdditionalLink = "";
	}
}