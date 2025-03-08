using Godot;
using System;
public partial class GoldenCross : UnusualCrossNode
{
    [Signal] public delegate void OnCollectedEventHandler();

    // Price
    const int DEFAULT_MIN_PRICE = 3;
    const int DEFAULT_MAX_PRICE = 7;
    private Random _random = new Random();
    public int Price;


    // Moving
    const float LIFETIME_PER_100PX = 0.5f;
    public float LifeTime = 6f;
    public enum StateEnum {Default, Disappearing, Collected}
    private StateEnum _state = StateEnum.Default;

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

        // Lifetime determination
        LifeTime = 3f + LIFETIME_PER_100PX * GlobalPosition.DistanceTo(G.Player.GlobalPosition) / 100;

        // Price determination
        Price = _random.Next(DEFAULT_MIN_PRICE, DEFAULT_MAX_PRICE);

        // Connecting a pointer to despawn when player dies
        var offscreenPointer = GetNode<OffscreenPointer>("Path2D/Cross/OffscreenPointer");
        if (G.Player != null)
            G.Player.Connect("PlayerDied", new Callable(offscreenPointer, "queue_free"));

        // Connecting a pointer to despawn when collected
        Connect("OnCollected", new Callable(offscreenPointer, "queue_free"));

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
        if (LifeTime < DISAPPEARING_ANIMATION_LENGTH && _state == StateEnum.Default) // Default state in the condition so that the tag can't start disappearing while being collected.
        {
            _state = StateEnum.Disappearing;
            GetNode<AnimationPlayer>("Path2D/Cross/AnimationPlayer").Play("Disappearing");
        }
    }

    public async void OnDisappeared()
    {
        var shineParticles = GetNode<CpuParticles2D>("Path2D/Cross/ShineParticles");
        shineParticles.Emitting = false;
        GetNode<Area2D>("Path2D/Cross/CollectArea").Monitoring = false;

        await ToSignal(shineParticles, "finished");
        QueueFree();
    }

    public async void onCollected()
    {
        _state = StateEnum.Collected;

        EmitSignal(nameof(OnCollected));

        var onCollectedParticles1 = GetNode<CpuParticles2D>("Path2D/Cross/OnCollectedParticles1");
        onCollectedParticles1.Emitting = true;
        onCollectedParticles1.Amount = Price;

        GetNode<CpuParticles2D>("Path2D/Cross/OnCollectedParticles2").Emitting = true;

        var animationPlayer = GetNode<AnimationPlayer>("Path2D/Cross/AnimationPlayer");
        animationPlayer.Stop(true); // If you don't save the state, the next animation fails.
        animationPlayer.Play("OnCollected");

        // Playing a random collect sound
        GetNode<AudioStreamPlayer>("Path2D/Cross/OnGoldenCrossCollected" + _random.Next(1, 3)).Play();

        // Giving the reward
        UnchangableMeta.AddGoldenCrosses(Price);

        // Creating a collect popup
        if (G.Player != null)
        {
            var collectPopup = PopupText.Instance(G.Player.Position);
            collectPopup.Text = Price.ToString();
            collectPopup.Scale = new Vector2(2, 2);

            collectPopup.LifeTime = 1f;
            collectPopup.Gravity /= 2;

            G.Player.GetParent().AddChild(collectPopup);
        }

        var shineParticles = GetNode<CpuParticles2D>("Path2D/Cross/ShineParticles");
        shineParticles.Emitting = false;

        await ToSignal(shineParticles, "finished");
        QueueFree();
    }

    public override void Respawn()
    {
        base.Respawn();

        _state = StateEnum.Default;

        RotationDegrees = _random.Next(-360, 360);
        _cross.GlobalRotationDegrees = 0;
        RotationSpeed = 0;
        RotationAcceleration = -MAX_ROTATION_ACCELERATION + (_random.NextSingle() * MAX_ROTATION_ACCELERATION * 2);

        LifeTime = 3f + (GlobalPosition.DistanceTo(G.Player.GlobalPosition) / 100 * LIFETIME_PER_100PX);
        Price = _random.Next(DEFAULT_MIN_PRICE, DEFAULT_MAX_PRICE);

        GetNode<Area2D>("Path2D/Cross/CollectArea").Monitoring = true;
        GetNode<AnimationPlayer>("Path2D/Cross/AnimationPlayer").Stop();
        GetNode<CpuParticles2D>("Path2D/Cross/ShineParticles").Emitting = true;
    }
}
