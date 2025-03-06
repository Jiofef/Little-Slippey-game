using Godot;
using static OtherExtension.RandomTools;
using static IRespawnable;

public interface IRespawnable
{
    public void Respawn();

    public delegate void SaveEventHandler();
    public event SaveEventHandler Save;

    public delegate void UnSaveEventHandler();
    public event UnSaveEventHandler UnSave;
}

public class CrossRotator
{
    public CrossRotator(UnusualCrossNode node, int initialRotationRange, float finalRotationRange)
    {
        this.node = node;

        _initialRotationRange = initialRotationRange;
        _finalRotationRange = finalRotationRange;

        Randomize();
        node.Ready += () => node.RotationDegrees = _initialRotation;
    }

    public void Randomize()
    {
        _initialRotation = RandomIn(-_initialRotationRange, _initialRotationRange);
        node.Rotation = _initialRotation;

        _rotationGoal = RandomIn(-_finalRotationRange, _finalRotationRange);
    }
    UnusualCrossNode node;

    private float _rotationGoal, _initialRotation, _initialRotationRange, _finalRotationRange;

    public void Rotate(float ticks, float maxTicks)
    {
        float TicksCoeff = 1 - (ticks / maxTicks);
        TicksCoeff = Mathf.Lerp(0.0f, 1.0f, 1 - (1 - TicksCoeff) * (1 - TicksCoeff) * (1 - TicksCoeff));

        node.RotationDegrees = _initialRotation + _rotationGoal * TicksCoeff;
    }

    public void Rotate(float ticksCoeff)
    {
        node.RotationDegrees = _initialRotation + _rotationGoal * ticksCoeff;
    }
}

public abstract partial class UnusualCrossNode : Node2D, IRespawnable
{
    // Respawn properties and methods
    public event SaveEventHandler Save = delegate { };
    public event UnSaveEventHandler UnSave = delegate { };

    public bool ShouldBeSavedInPool;

    protected bool _isInRespawnPool = false;
    public bool IsInRespawnPool { get => _isInRespawnPool; }

    public virtual void Respawn()
    {
        UnSave?.Invoke();

        _isInRespawnPool = false;
    }

    public void SendToRespawnPool()
    {
        ProcessMode = ProcessModeEnum.Disabled;
        Visible = false;
        TicksLived = 0;

        _isInRespawnPool = true;

        Save?.Invoke();
    }

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
    public CollisionShape2D ExplosiveArea;
    public ExplosionAnimation ExplosionAnimation;
    public AudioStreamPlayer ExplosionSound;

    public Sprite2D CrossSprite;
    public Sprite2D WarningSprite;

    public async virtual void Explode()
    {
        // Visual
        if (CrossSprite != null) CrossSprite.Visible = false;

        if (WarningSprite != null) WarningSprite.Visible = false;

        if (ExplosionAnimation != null)
        {
            ExplosionAnimation.Visible = true;
            ExplosionAnimation.Play();
        }

        // Audio
        ExplosionSound?.Play();

        // Physics
        if (ExplosiveArea != null)
            ExplosiveArea.Disabled = false;
        SetPhysicsProcess(false);

        // Waiting one frame to disable explosion collision
        await G.WaitForFrame();
        await G.WaitForFrame();
        if (ExplosiveArea != null)
            ExplosiveArea.Disabled = true;

        // Groups
        foreach (var group in GetGroups())
            RemoveFromGroup(group);
    }

    public virtual void OnFinished() 
    {
        if (ShouldBeSavedInPool)
            SendToRespawnPool();
        else
            QueueFree();
    }

    public override void Respawn()
    {
        base.Respawn();
        ProcessMode = ProcessModeEnum.Inherit;
        SetPhysicsProcess(true);

        Visible = true;

        _isInRespawnPool = false;
    }
}
