using Godot;
using System;
using OtherExtension;
using static OtherExtension.RandomTools;
using static IRespawnable;

interface IRespawnable
{
    public void Respawn();

    public delegate void SaveEventHandler();

    public event SaveEventHandler Save;
}

class CrossRotator
{
    public CrossRotator(UnusualCrossNode node, int initialRotationRange, float finalRotationRange)
    {
        this.node = node;

        initialRotation = RandomIn(-initialRotationRange, initialRotationRange);
        node.Ready += () => node.RotationDegrees = initialRotation;

        _rotationGoal = RandomIn(-finalRotationRange, finalRotationRange);
    }
    UnusualCrossNode node;

    private float _rotationGoal, initialRotation;

    public void Rotate(float ticks, float maxTicks)
    {
        float TicksCoeff = 1 - (ticks / maxTicks);
        TicksCoeff = Mathf.Lerp(0.0f, 1.0f, 1 - (1 - TicksCoeff) * (1 - TicksCoeff) * (1 - TicksCoeff));

        node.RotationDegrees = initialRotation + _rotationGoal * TicksCoeff;
    }
}

abstract partial class UnusualCrossNode : Node2D, IRespawnable
{
    // Respawn properties and methods
    public event SaveEventHandler Save = delegate { };

    public bool ShouldBeSavedInPool;

    protected bool _isInRespawnPool = false;
    public bool IsInRespawnPool { get => _isInRespawnPool; }

    public abstract void Respawn();

    public void SendToRespawnPool()
    {
        ProcessMode = ProcessModeEnum.Disabled;
        Visible = false;
        TicksLived = 0;

        _isInRespawnPool = true;
    }

    // Life cycle
    public int TicksLived = 0;
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        TicksLived++;
    }
}

abstract partial class CrossNode : UnusualCrossNode
{
    public CollisionShape2D ExplosiveArea;
    public ExplosionAnimation ExplosionAnimation;
    public AudioStreamPlayer ExplosionSound;

    public Sprite2D CrossSprite;
    public Sprite2D WarningSprite;

    public async virtual void Explode()
    {
        // Visual
        CrossSprite.QueueFree();
        WarningSprite.QueueFree();
        ExplosionAnimation.Visible = true;
        ExplosionAnimation.Play();

        // Audio
        ExplosionSound.Play();

        // Physics
        ExplosiveArea.Disabled = false;

        // Waiting one frame to disable explosion collision
        await GodotExtensions.WaitForFrame();
        ExplosiveArea.Disabled = true;
        SetPhysicsProcess(false);

        // Groups
        foreach (var group in GetGroups())
            RemoveFromGroup(group);
    }

    public override void Respawn()
    {
        ProcessMode = ProcessModeEnum.Inherit;
        SetPhysicsProcess(true);

        Visible = true;

        _isInRespawnPool = false;
    }
}
