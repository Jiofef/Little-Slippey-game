using Godot;
using static OtherExtension.RandomTools;

public interface CrossesNodesAbstract
{
    public void Respawn();

    [Signal] public delegate void SaveEventHandler();

    [Signal] public delegate void UnSaveEventHandler();
}

public class CrossRotator
{
    public CrossRotator(UnusualCrossNode cross, int initialRotationRange, float finalRotationRange)
    {
        Cross = cross;

        InitialRotationRange = initialRotationRange;
        FinalRotationRange = finalRotationRange;

        Randomize();
        cross.Ready += () => cross.RotationDegrees = InitialRotation;
    }

    public void Randomize()
    {
        InitialRotation = RandomIn(-InitialRotationRange, InitialRotationRange);
        Cross.Rotation = InitialRotation;

        RotationGoal = RandomIn(-FinalRotationRange, FinalRotationRange);
    }
    public UnusualCrossNode Cross;

    public float RotationGoal, InitialRotation, InitialRotationRange, FinalRotationRange;

    public void Rotate(float ticks, float maxTicks)
    {
        float TicksCoeff = 1 - (ticks / maxTicks);
        TicksCoeff = Mathf.Lerp(0.0f, 1.0f, 1 - (1 - TicksCoeff) * (1 - TicksCoeff) * (1 - TicksCoeff));

        Cross.RotationDegrees = InitialRotation + RotationGoal * TicksCoeff;
    }

    public void Rotate(float ticksCoeff)
    {
        Cross.RotationDegrees = InitialRotation + RotationGoal * ticksCoeff;
    }
}

public abstract partial class UnusualCrossNode : Node2D, CrossesNodesAbstract
{
    // Respawn properties and methods
    [Signal] public delegate void SaveEventHandler();
    [Signal] public delegate void UnSaveEventHandler();

    [Signal] public delegate void RespawnedEventHandler();
    [Signal] public delegate void FinishedEventHandler();

    public bool ShouldBeSavedInPool = false;

    protected bool _isInRespawnPool = false;
    public bool IsInRespawnPool { get => _isInRespawnPool; }

    public virtual void Respawn()
    {
        ProcessMode = ProcessModeEnum.Inherit;
        Visible = true;

        _isInRespawnPool = false;

        EmitSignal("UnSave");
    }

    public void SendToRespawnPool()
    {
        ProcessMode = ProcessModeEnum.Disabled;
        Visible = false;
        TicksLived = 0;

        _isInRespawnPool = true;

        EmitSignal("Save");
    }

    public virtual void OnFinished()
    {
        EmitSignal("Finished");

        if (ShouldBeSavedInPool)
            SendToRespawnPool();
        else
            QueueFree();
    }

    public virtual void OnPositionSetted() { }

    // Life cycle
    public float TicksLived = 0;
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        TicksLived++;
    }
}

abstract public partial class CrossNode : UnusualCrossNode
{
    [Signal] public delegate void ExplodedEventHandler();

    public CollisionShape2D ExplosiveArea;
    public ExplosionAnimation ExplosionAnimation;
    public AudioStreamPlayer ExplosionSound;

    public Sprite2D CrossSprite;
    public Sprite2D WarningSprite;

    public bool IsExploded = false;

    public async virtual void Explode()
    {
        IsExploded = true;
        // Visual
        if (CrossSprite != null) CrossSprite.Visible = false;

        if (WarningSprite != null) WarningSprite.Visible = false;

        if (ExplosionAnimation != null)
        {
            ExplosionAnimation.Visible = true;
            ExplosionAnimation.Play();
        }

        // Audio
        if (ExplosionSound != null)
        {
            const float PITCH_RANGE = 0.2f, PITCH_MIN = 1 - PITCH_RANGE, PITCH_MAX = 1 + PITCH_RANGE;
            const float VOLUME_RANGE = 0.15f, VOLUME_MIN = 0 - VOLUME_RANGE, VOLUME_MAX = 0 + VOLUME_RANGE;
            ExplosionSound.PitchScale = RandomIn(PITCH_MIN, PITCH_MAX);
            ExplosionSound.VolumeDb = RandomIn(VOLUME_MIN, VOLUME_MAX);
            ExplosionSound.Play();
        }

        // Physics
        if (ExplosiveArea != null)
            ExplosiveArea.SetDeferred("disabled", false);
        SetPhysicsProcess(false);

        // Waiting one frame to disable explosion collision
        await G.WaitForFrame();
        await G.WaitForFrame();
        if (ExplosiveArea != null)
            ExplosiveArea.Disabled = true;

        // Groups
        foreach (var group in GetGroups())
            RemoveFromGroup(group);

        EmitSignal("Exploded");
    }

    public override void Respawn()
    {
        base.Respawn();

        IsExploded = false;

        SetPhysicsProcess(true);

        MoveToFront();

        EmitSignal("Respawned");
    }

    /// <summary>
    /// Initializes all standard nodes for cross to standard paths. Nodes: CrossSprite, WarningSprite, ExplosionAnimation, ExplosiveArea, ExplosionSound.
    /// <para>By default all have the same path as their name, except ExplosiveArea, its path is "ExplosiveArea/CollisionShape2D"</para>
    /// </summary>
    public virtual void NodesInit()
    {
        CrossSprite = GetNode<Sprite2D>("CrossSprite");
        WarningSprite = GetNode<Sprite2D>("WarningSprite");
        ExplosionAnimation = GetNode<ExplosionAnimation>("ExplosionAnimation");
        ExplosiveArea = GetNode<CollisionShape2D>("ExplosiveArea/CollisionShape2D");
        ExplosionSound = GetNode<AudioStreamPlayer>("ExplosionSound");
    }
}

