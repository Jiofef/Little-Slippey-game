using Godot;

public partial class TutorialCanvasLayer : CanvasLayer
{
	public override void _Ready()
	{
		G.IsProgressPaused = true;
		G.IsCrossesEnabled = false;
		if (UnchangableMeta.IsTutorialPlayed)
		{
			GetNode<AnimationPlayer>("Label/AnimationPlayer").Play("Disappearing");
            void SetHoldIMG(string ImageName)
            {
                GetNode<RichTextLabel>("Label").Text = "[center]Press [img]res://Content/Sprites/Interface/ControllerButtons/" + ImageName + ".png[/img] to skip intro";
            }
            switch (G.TypeOfUsedController)
            {
                case "Keyboard":
                    SetHoldIMG("KeyboardButtonBigF");
                    break;
                case "PS Gamepad":
                    SetHoldIMG("PSControllerBigX");
                    break;
                default:
                    SetHoldIMG("XControllerBigAmogus");
                    break;
            }
        }
		else
		{
			SetPhysicsProcess(false);
			UnchangableMeta.IsTutorialPlayed = true;
			UnchangableMeta.SaveToFile();
			GetNode<RichTextLabel>("Label").QueueFree();
		}
	}
    public override void _PhysicsProcess(double delta)
    {
        if (Input.IsPhysicalKeyPressed(Key.F) || Input.IsJoyButtonPressed(0, JoyButton.A))
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
