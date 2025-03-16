using Godot;
using System;
using System.Diagnostics;
using static Crosses;

public partial class ElementaryParticle : UnusualCrossNode
{
    [Signal] public delegate void OnAnihilatedEventHandler();

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

    public float RotationSpeed = 0;



    public override void _Ready()
    {
        QueueFree();
        // Rotation randomizing
        const int MAX_ROTATION_SPEED = 3;
        RotationSpeed = OtherExtension.RandomTools.RandomIn(-MAX_ROTATION_SPEED, MAX_ROTATION_SPEED);

        // Price determination
        Price = _random.Next(DEFAULT_MIN_PRICE, DEFAULT_MAX_PRICE);

        // Connecting a pointer to hide when player dies
        Callable updateOffscreenPointer = new Callable(this, "UpdateOffscreenPointer");
        if (G.Player != null)
            G.Player.Connect("PlayerDied", updateOffscreenPointer);

        // Connecting a pointer to hide when collected
        Connect("OnAnihilated", updateOffscreenPointer);

        // Adding in group
        AddToGroup("UnstableCrosses");
    }

    private Vector2 _acceleration, _playerGlobalPos;
    public override void _PhysicsProcess(double delta)
    {
        if (G.Player == null) return;

        // Rotation
        RotationDegrees += RotationSpeed;


        // Moving
        _playerGlobalPos = G.Player.GlobalPosition;

        _acceleration = GlobalPosition.DirectionTo(_playerGlobalPos) * AttractionForce / GlobalPosition.DistanceSquaredTo(_playerGlobalPos);

        foreach(ElementaryParticle cross in GetTree().GetNodesInGroup("UnstableCrosses"))
        {
            if (cross != this)
            {
                // IsMainParticle is different for particles with different symbols. If the symbols are different, the particles attract, and vice versa.
                float symbol = cross.IsMainParticle != IsMainParticle ? 1 : -1;
                _acceleration += GlobalPosition.DirectionTo(cross.GlobalPosition) * AttractionForce / GlobalPosition.DistanceSquaredTo(cross.GlobalPosition) * symbol;
            }
        }

        if (_acceleration.Length() > AccelerationCap)
            _acceleration = _acceleration.Normalized() * AccelerationCap;
        
        Velocity += _acceleration;

        if (Velocity.Length() > SpeedCap)
            Velocity = Velocity.Normalized() * SpeedCap;

        Translate(Velocity);
        Velocity /= DecelerationMultiplier;


        // DisAppearing
        LifeTime -= G.FLOAT_DELTA;

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
        OnFinished();
    }

    // Yes, I know that a proton and an electron do not anihilate together. Think of it as a metaphor
    public async void Anihilate(Area2D particleArea)
    {
        _state = StateEnum.Collected;

        EmitSignal(nameof(OnAnihilated));

        var animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        animationPlayer.Stop(true); // If you don't save the state, the next animation fails.
        animationPlayer.Play("OnCollected");

        RemoveFromGroup("UnstableCrosses");

        // Disabling area
        SetAreaDisabled(true);

        if (IsMainParticle)
        {
            var antiParticle = particleArea.GetParent<ElementaryParticle>();
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
        OnFinished();
    }

    public void SetAreaDisabled(bool value)
    {
        var area = GetNode<Area2D>("ChargeArea");
        area.SetDeferred("monitoring", !value);
        area.SetDeferred("monitorable", !value);
    }

    public void UpdateOffscreenPointer()
    {
        GetNode<OffscreenPointer>("OffscreenPointer").IsHidden = G.IsPlayerDead || _state == StateEnum.Collected;
    }

    public override void Respawn()
    {
        base.Respawn();


        _state = StateEnum.Default;

        Velocity = Vector2.Zero;
        RotationDegrees = _random.Next(-360, 360);
        GlobalRotationDegrees = 0;
        const int MAX_ROTATION_SPEED = 3;
        RotationSpeed = OtherExtension.RandomTools.RandomIn(-MAX_ROTATION_SPEED, MAX_ROTATION_SPEED);

        LifeTime = 15f;
        Price = _random.Next(DEFAULT_MIN_PRICE, DEFAULT_MAX_PRICE);

        GetNode<Area2D>("ChargeArea").SetDeferred("monitoring", true);
        var animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        animationPlayer.Stop();
        animationPlayer.Play("Appearing");
        GetNode<CpuParticles2D>("MicroParticles").Emitting = true;

        AddToGroup("UnstableCrosses");

        UpdateOffscreenPointer();
    }
}
