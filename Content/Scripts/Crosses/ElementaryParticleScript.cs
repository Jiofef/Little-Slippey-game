using Godot;
using System;

public partial class ElementaryParticleScript : Node2D
{
    // One of the particles must be the primary particle to interact with the second particle
    [Export] public bool IsMainParticle;
    [Export] public float AttractionForce = 3000f;
    [Export] public float DecelerationMultiplier = 1.02f;
    [Export] public float AccelerationCap = 0.5f, SpeedCap = 50;

    Random random = new Random();
    private float _rotationSpeed = 0;

    public override void _Ready()
    {
        const int MAX_ROTATION_SPEED = 3;
        _rotationSpeed = random.Next(-MAX_ROTATION_SPEED, MAX_ROTATION_SPEED + 1) + random.NextSingle() - 0.5f;
    }

    public Vector2 Velocity;
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

    public void Anihilate()
    {

    }
}
