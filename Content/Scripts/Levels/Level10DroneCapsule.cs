using Godot;
using System.Threading.Tasks;

public partial class Level10DroneCapsule : Node2D
{
    bool _isTrueFinaling;

    public override void _Ready()
    {
        G.Player.PlayerDied += Departure;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (G.Scores < 300) return;


        if (_isTrueFinaling)
            G.ResetTimer += 0.003f;

        if (G.ResetTimer >= 2f)
            GetTree().ChangeSceneToFile("res://Content/Scenes/Interface&Menu/FinalLetter.tscn");
    }

    public async void Departure()
    {
        if (G.Scores > 300)
        {
            await ToSignal(GetTree().CreateTimer(4.5f), SceneTreeTimer.SignalName.Timeout);

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
