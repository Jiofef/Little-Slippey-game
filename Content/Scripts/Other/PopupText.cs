using Godot;
using System;

public partial class PopupText : Label
{
    public static PopupText Instance()
    {
        PopupText instance = (PopupText)GD.Load<PackedScene>("res://Content/Scenes/Other/PopupText.tscn").Instantiate();
        instance.Position -= instance.Size / 2;

        return instance;
    }
    public static PopupText Instance(Vector2 position)
    {
        PopupText instance = (PopupText)GD.Load<PackedScene>("res://Content/Scenes/Other/PopupText.tscn").Instantiate();
        instance.Position = position - instance.Size / 2;
        return instance;
    }

    Random _random = new Random();

    [Export] public float LifeTime = 0.5f;
    private float _startLifeTime;

    // Moving
    [Export] public Vector2 Velocity = new Vector2(0, -10);
    [Export] public float SpawnSpreadDegrees = 0;

    [Export] public Vector2 Gravity = new Vector2(0, 9.8f);

    public override void _Ready()
    {
        _startLifeTime = LifeTime;

        if (SpawnSpreadDegrees != 0)
            Velocity = Velocity.Rotated((_random.Next(int.MinValue, int.MaxValue) + _random.NextSingle()) % Mathf.DegToRad(SpawnSpreadDegrees));
    }

    public override void _PhysicsProcess(double delta)
    {
        const float FLOAT_DELTA = 0.016667f;

        float lifeTimeRatio = LifeTime / _startLifeTime;

        // Moving
        Position += Velocity * FLOAT_DELTA;
        Velocity += Gravity;

        // Disappearing
        LifeTime -= FLOAT_DELTA;

        Modulate = new Color(Modulate.R, Modulate.G, Modulate.B, lifeTimeRatio);

        if (LifeTime < 0)
        {
            QueueFree();
        }
    }
}
