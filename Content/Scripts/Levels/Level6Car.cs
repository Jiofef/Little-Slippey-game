using Godot;
using System;
using OtherExtension;
using System.Security.Cryptography.X509Certificates;

public partial class Level6Car : SwooshObject
{
    [Export] private bool _isPreSpawned;
    [Export] private float _minSpeed = 5, _maxSpeed = 7;
    // Nodes
    public AnimatableBody2D Car;
    public CollisionShape2D Collision, AreaCollision;

    // Moving
    public float YVelocity;

    public override void _Ready()
	{
        base._Ready();
        // Initializing nodes
        Car = GetNode<AnimatableBody2D>("Car");
        Collision = Car.GetNode<CollisionShape2D>("CollisionShape2D");
        AreaCollision = Car.GetNode<CollisionShape2D>("CarArea/CollisionShape2D");

        if (!_isPreSpawned)
        {
            const int PATH_X_LENGTH = 2560;
            const int PATH_Y_RANGE = 250;

            Curve = new Curve2D();
            Curve.AddPoint(new Vector2(0, 0));

            Random random = new Random();
            Curve.AddPoint(new Vector2(PATH_X_LENGTH, random.Next(-PATH_Y_RANGE / 2, PATH_Y_RANGE / 2)));

            Speed = RandomTools.RandomIn(_minSpeed, _maxSpeed);

            SetCollisionEnabled(false);
        }
        else
        {
            SetCollisionEnabled(true);
        }
	}

	public override void _PhysicsProcess(double delta)
	{
        base._PhysicsProcess(delta);

        ManeuverProcess();
    }

    public void SetCollisionEnabled(bool value)
    {
        Collision.SetDeferred("disabled", !value);
        AreaCollision.SetDeferred("disabled", !value);

        var additionalExplosiveArea = GetNodeOrNull<CollisionShape2D>("Car/AdditionalExplosiveArea/CollisionShape2D");
        additionalExplosiveArea?.SetDeferred("disabled", !value);
        var trampolineCollision = GetNodeOrNull<CollisionShape2D>("Car/TrampolineArea/CollisionShape2D");
        trampolineCollision?.SetDeferred("disabled", !value);
    }

    public override void OnObjectAppeared()
    {
        SetCollisionEnabled(true);
    }

    public override void OnObjectDisappearing()
    {
        SetCollisionEnabled(false);
    }

    // When player's hitbox or other car's area enters the collision of this car
    public bool IsExploded = false;
    public void Explode()
    {
        if (IsExploded) return;

        IsExploded = true;

        IsStopped = true;

        SetCollisionEnabled(false);

        Car.GetNode<AnimationPlayer>("AnimationPlayer").Play("Explode");
    }
    public void Delete()
    {
        QueueFree();
    }

    // Maneuvering
    private int _carsAhead = 0;
    private float _thisAndFrontCarHeightDifference = 0;
    public void ManeuverProcess()
    {
        if (IsStopped) return;

        if (_carsAhead > 0)
            YVelocity += MathF.Sign(_thisAndFrontCarHeightDifference) * 0.02f * (5 - Mathf.Abs(YVelocity));
        else
            YVelocity /= 1.1f;

        PathFollow.VOffset += YVelocity;
    }

    public void TheCarAhead(Area2D area)
    {
        _carsAhead++;
        _thisAndFrontCarHeightDifference = GlobalPosition.Y - area.GlobalPosition.Y;
    }

    public void TheCarNoLongerAhead()
    {
        _carsAhead--;
    }

    private const float NEEDED_PLAYER_VELOCITY_TO_RAM_THE_BOTTOM = 600;
    public void OnBottomHitted(Node2D body)
    {
        if (body is Player player && -player.Velocity.Y >= NEEDED_PLAYER_VELOCITY_TO_RAM_THE_BOTTOM)
            Explode();
    }
    private const float PLAYER_COLLIDE_EXPLOSION_VELOCITY_CEILING = 900f;
    public void OnCollided(Node2D body)
    {
        if (body is Player player)
        {
            if (player.Velocity.Y <= PLAYER_COLLIDE_EXPLOSION_VELOCITY_CEILING)
                Explode();
        }
    }

    // For Trampoline cars
    [Export] public float CharacterToss = 1200f;
    public void TossTheCharacter(Node2D node)
    {
        if (node is not CharacterBody2D character) return;

        character.Velocity = new Vector2(0, -CharacterToss);

        Car.GetNode<AudioStreamPlayer>("TrampolineSound").Play();
    }
}
