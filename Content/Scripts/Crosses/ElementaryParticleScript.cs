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
    const int DEFAULT_MIN_PRICE = 7;
    const int DEFAULT_MAX_PRICE = 17;
    public int Price;

    Random _random = new Random();

    // Moving
    public float LifeTime = 15f;
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

        // Adding in group
        AddToGroup("UnstableCrosses");
    }


    public override void _PhysicsProcess(double delta)
    {
        if (G.Player == null) return;

        // Rotation
        RotationDegrees += _rotationSpeed;


        // Moving
        Vector2 globalPlayerPos = G.Player.GlobalPosition;

        Vector2 acceleration = GlobalPosition.DirectionTo(globalPlayerPos) * AttractionForce / GlobalPosition.DistanceSquaredTo(globalPlayerPos);

        foreach(ElementaryParticleScript cross in GetTree().GetNodesInGroup("UnstableCrosses"))
        {
            if (cross != this)
            {
                // IsMainParticle is different for particles with different symbols. If the symbols are different, the particles attract, and vice versa.
                float symbol = cross.IsMainParticle != IsMainParticle ? 1 : -1;
                acceleration += GlobalPosition.DirectionTo(cross.GlobalPosition) * AttractionForce / GlobalPosition.DistanceSquaredTo(cross.GlobalPosition) * symbol;
            }
        }

        if (acceleration.Length() > AccelerationCap)
            acceleration = acceleration.Normalized() * AccelerationCap;
        
        Velocity += acceleration;

        if (Velocity.Length() > SpeedCap)
            Velocity = Velocity.Normalized() * SpeedCap;

        Translate(Velocity);
        Velocity /= DecelerationMultiplier;


        // DisAppearing
        const float FLOAT_DELTA = 0.016667f;
        LifeTime -= FLOAT_DELTA;

        const float DISAPPEARING_ANIMATION_LENGTH = 3f;
        if (LifeTime < DISAPPEARING_ANIMATION_LENGTH && _state == StateEnum.Default) // Default state in the condition so that the tag can't start disappearing while being collected.
        {
            _state = StateEnum.Disappearing;
            GetNode<AnimationPlayer>("AnimationPlayer").Play("Disappearing");
        }
    }

    public async void OnDisappeared()
    {
        var microParticles = GetNode<CpuParticles2D>("MicroParticles");
        microParticles.Emitting = false;
        SetAreaDisabled(true);

        RemoveFromGroup("UnstableCrosses");

        await ToSignal(microParticles, "finished");
        QueueFree();
    }

    // Yes, I know that a proton and an electron do not anihilate together. Think of it as a metaphor
    public async void Anihilate(Area2D particleArea)
    {
        _state = StateEnum.Collected;

        var animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        animationPlayer.Stop(true); // If you don't save the state, the next animation fails.
        animationPlayer.Play("OnCollected");

        RemoveFromGroup("UnstableCrosses");

        // Disabling area
        SetAreaDisabled(true);

        if (IsMainParticle)
        {
            var antiParticle = particleArea.GetParent<ElementaryParticleScript>();
            antiParticle.Anihilate(GetNode<Area2D>("ChargeArea"));

            // Collect effects
            var onCollectedEffects = GetNode<Node2D>("OnCollectedEffects");
            onCollectedEffects.GlobalPosition = (GlobalPosition + antiParticle.GlobalPosition) / 2;

            var onCollectedParticles1 = onCollectedEffects.GetNode<CpuParticles2D>("OnCollectedParticles1");
            onCollectedParticles1.Emitting = true;
            onCollectedParticles1.Amount = Price;

            onCollectedEffects.GetNode<CpuParticles2D>("OnCollectedParticles2").Emitting = true;


            // Playing a random collect sound
            GetNode<AudioStreamPlayer>("OnGoldenCrossCollected" + _random.Next(1, 3)).Play();

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

        }

        var microParticles = GetNode<CpuParticles2D>("MicroParticles");
        microParticles.Emitting = false;

        await ToSignal(microParticles, "finished");
        QueueFree();
    }

    public void SetAreaDisabled(bool value)
    {
        var area = GetNode<Area2D>("ChargeArea");
        area.SetDeferred("monitoring", !value);
        area.SetDeferred("monitorable", !value);
    }
}
