using Godot;

public partial class Level1TutorialDarkeningAnimationPlayer : AnimationPlayer
{
    public void Darkening()
    {
        Play("Darkening");
    }
    public void Lightening()
    {
        Play("Lightening");
    }
}
