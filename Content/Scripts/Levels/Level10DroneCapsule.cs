using Godot;

public partial class Level10DroneCapsule : Node2D
{
    bool _isTrueFinaling;

    public override void _PhysicsProcess(double delta)
    {
        if (G.Scores > 300)
        {
            if (Input.IsActionPressed("Reset") || _isTrueFinaling)
                G.ResetTimer += 0.003f;
            else if (G.ResetTimer > 0)
                G.ResetTimer -= 0.01f;
            if (G.ResetTimer >= 2f)
                GetTree().ChangeSceneToFile("res://Content/Scenes/Interface&Menu/FinalLetter.tscn");
        }
    }

    public void Departure()
    {
        if (G.Scores > 300)
        {
            GetNode<AnimationPlayer>("AnimationPlayer").Play("TakingSlippey");

            GetParent<Node2D>().GlobalPosition = G.Player.GlobalPosition;
            G.Player.GetNode<Node2D>("SkinContainer").Rotation = G.Player.Rotation;
            G.Player.Rotation = 0;

            G.Main.GetNode<Pause>("Pause").ResetProcessDisabled = true;
            G.Main.IsResetDisabled = true;
        }
    }
    public void TrueFinale()
    {
        _isTrueFinaling = true;
    }
}
