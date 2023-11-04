using Godot;

public partial class TutorialCanvasLayer : CanvasLayer
{
	public override void _Ready()
	{
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
}
