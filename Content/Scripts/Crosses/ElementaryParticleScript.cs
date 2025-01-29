using Godot;
using System;
using System.Diagnostics;

public partial class ElementaryParticleScript : Node2D
{
    // One of the particles must be the primary particle to interact with the second particle
    [Export] public bool IsMainParticle;
    [Export] public float AttractionForce = 3000f;
    [Export] public float DecelerationMultiplier = 1.02f;
    [Export] public float AccelerationCap = 0.5f, SpeedCap = 50;

    // Price
    const int DEFAULT_MIN_PRICE = 3;
    const int DEFAULT_MAX_PRICE = 7;
    public int Price;

    Random _random = new Random();

    // Moving
    public float LifeTime = 6f;
    public enum StateEnum { Default, Disappearing, Collected }
    private StateEnum _state = StateEnum.Default;

    public Vector2 Velocity;

    private float _rotationSpeed = 0;



    public override void _Ready()
    {
        // Rotation randomizing
        const int MAX_ROTATION_SPEED = 3;
        _rotationSpeed = OtherExtension.RandomTools.RandomIn(-MAX_ROTATION_SPEED, MAX_ROTATION_SPEED);

        // Price determination
        Price = _random.Next(DEFAULT_MIN_PRICE, DEFAULT_MAX_PRICE);

        // Connecting a pointer to despawn when player dies
        var offscreenPointer = GetNode<OffscreenPointer>("OffscreenPointer");
        if (G.Player != null)
            G.Player.Connect("PlayerDied", new Callable(offscreenPointer, "queue_free"));
    }


    public override void _PhysicsProcess(double delta)
    {
        if (G.Player == null) return;

        // Rotation
        RotationDegrees += _rotationSpeed;


        // Moving
        Vector2 globalPlayerPos = G.Player.GlobalPosition;

        Vector2 acceleration = GlobalPosition.DirectionTo(globalPlayerPos) * AttractionForce / GlobalPosition.DistanceSquaredTo(globalPlayerPos);
        if (acceleration.Length() > AccelerationCap)
            acceleration = acceleration.Normalized() * AccelerationCap;
        
        Velocity += acceleration;

        if (Velocity.Length() > SpeedCap)
            Velocity = Velocity.Normalized() * SpeedCap;

        Translate(Velocity);
        Velocity /= DecelerationMultiplier;
    }

    public void OnDisappeared()
    {

    }

    public async void Anihilate(Area2D particleArea)
    {
        _state = StateEnum.Collected;

        var antiParticle = particleArea.GetParent<Node2D>();

        if (IsMainParticle)
        {
            // Collect effects
            var onCollectedEffects = GetNode<Node2D>("OnCollectedEffects");
            onCollectedEffects.GlobalPosition = (GlobalPosition + antiParticle.GlobalPosition) / 2;

            var onCollectedParticles1 = onCollectedEffects.GetNode<CpuParticles2D>("OnCollectedParticles1");
            onCollectedParticles1.Emitting = true;
            onCollectedParticles1.Amount = Price;

            onCollectedEffects.GetNode<CpuParticles2D>("OnCollectedParticles2").Emitting = true;


            var animationPlayer = GetNode<AnimationPlayer>("Path2D/Cross/AnimationPlayer");
            animationPlayer.Stop(true); // If you don't save the state, the next animation fails.
            animationPlayer.Play("OnCollected");

            // Giving the reward
            UnchangableMeta.GoldenCrossesAmount += Price;
            if (Achievements.CurrentAdditionalGuiLayer != null)
            {
                Achievements.CurrentAdditionalGuiLayer.OnGoldenCrossesRecieved(Price);
            }

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

        }

        var photonParticles = GetNode<CpuParticles2D>("PhotonParticles");
        photonParticles.Emitting = false;

        await ToSignal(photonParticles, "finished");
        QueueFree();
    }
}
