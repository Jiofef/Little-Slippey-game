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
            else if (G.ResetTimer != 0)
                G.ResetTimer = 0;
            if (G.ResetTimer >= 2f)
                GetTree().ChangeSceneToFile("res://Content/Scenes/Interface&Menu/FinalLetter.tscn");
        }
    }

    public void Departure()
    {
        if (G.Scores > 300)
        {
            GetNode<AnimationPlayer>("AnimationPlayer").Play("TakingSlippey");

            var player = GetNode<CharacterBody2D>("../../Player");
            GetParent<Node2D>().GlobalPosition = player.GlobalPosition;
            GetNode<Node2D>("../../Player/SkinContainer").Rotation = player.Rotation;
            player.Rotation = 0;
        }
    }
    public void TrueFinale()
    {
        _isTrueFinaling = true;
    }
}
