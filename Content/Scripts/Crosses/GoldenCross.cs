using Godot;
using System;
using static Godot.TextServer;

public partial class GoldenCross : Node2D
{
    [Signal] public delegate void OnCollectedEventHandler();

    // Price
    const int DEFAULT_MIN_PRICE = 10;
    const int DEFAULT_MAX_PRICE = 20;
    private Random _random = new Random();
    public int Price;


    // Moving
    public float LifeTime = 6f;
    public bool IsDisappearing = false;

    public const float MAX_ROTATION_SPEED = 2f;
    public float RotationSpeed = 0;

    public const float MAX_ROTATION_ACCELERATION = 0.04f;
    public float RotationAcceleration = 0;

    public Vector2 SideMovingDirection;
    public float MovingSpeed = 4f;

    //Nodes
    PathFollow2D _cross;

    public override void _Ready()
    {
        // Initializing the nodes
        _cross = GetNode<PathFollow2D>("Path2D/Cross");
        // Randomizing the transform
        RotationDegrees = _random.Next(-360, 360);
        _cross.GlobalRotationDegrees = 0;

        RotationAcceleration = -MAX_ROTATION_ACCELERATION + (_random.NextSingle() * MAX_ROTATION_ACCELERATION * 2);

        // Price determination
        UnchangableMeta.GoldenCrossesAmount = _random.Next(DEFAULT_MIN_PRICE, DEFAULT_MAX_PRICE);
    }

    public override void _PhysicsProcess(double delta)
    {
        // Moving
        _cross.Progress += MovingSpeed;

        //Rotation
        if (Meta.Instance.Video.CrossRotationWhenSpawning)
        {
            _cross.RotationDegrees += RotationSpeed;

            RotationSpeed += RotationAcceleration;
            RotationSpeed = Mathf.Clamp(RotationSpeed, -MAX_ROTATION_SPEED, MAX_ROTATION_SPEED);

            // Changing rotation direction
            const int REVERSE_PROBABILITY_OF_DIRECTIONAL_CHANGE = 120;
            if (_random.Next(REVERSE_PROBABILITY_OF_DIRECTIONAL_CHANGE) == 0)
            {
                RotationAcceleration = -MAX_ROTATION_ACCELERATION + (_random.NextSingle() * MAX_ROTATION_ACCELERATION * 2); // Something like _random.Next(-RANGE, RANGE) but for float
            }
        }

        // DisAppearing
        const float FLOAT_DELTA = 0.016667f;
        LifeTime -= FLOAT_DELTA;

        const float DISAPPEARING_ANIMATION_LENGTH = 3f;
        if (LifeTime < DISAPPEARING_ANIMATION_LENGTH && !IsDisappearing)
        {
            IsDisappearing = true;
            GetNode<AnimationPlayer>("Path2D/Cross/AnimationPlayer").Play("Disappearing");
        }
    }

    public void OnDisappeared()
    {
        GetNode<CpuParticles2D>("Path2D/Cross/ShineParticles").Emitting = false;

        // To remove the node when the last particles are gone.
        GetNode<Timer>("Path2D/Cross/ShineParticles/ParticlesDisappearingTimer").Start();
    }

    public void OnShineParticlesDisappeared()
    {
        QueueFree();
    }

    public void onCollected()
    {
        EmitSignal(nameof(OnCollected));

        GetNode<CpuParticles2D>("Path2D/Cross/ShineParticles").Emitting = false;

        GetNode<CpuParticles2D>("Path2D/Cross/OnCollectedParticles1").Emitting = true;
        GetNode<CpuParticles2D>("Path2D/Cross/OnCollectedParticles2").Emitting = true;

        GetNode<AnimationPlayer>("Path2D/Cross/AnimationPlayer").Play("OnCollected");

        // To remove the node when the last particles are gone.
        GetNode<Timer>("Path2D/Cross/ShineParticles/ParticlesDisappearingTimer").Start();

        // Playing a random collect sound
        GetNode<AudioStreamPlayer>("Path2D/Cross/OnGoldenCrossCollected" + _random.Next(1, 3)).Play();

        // Giving the reward
        UnchangableMeta.GoldenCrossesAmount += Price;
        if (Achievements.CurrentAdditionalGuiLayer != null)
        {
            Achievements.CurrentAdditionalGuiLayer.OnGoldenCrossesRecieved();
        }
    }
}
