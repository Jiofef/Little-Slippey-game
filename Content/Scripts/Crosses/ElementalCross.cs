using Godot;
using System;
using OtherExtension;
using System.Collections.Generic;

public partial class ElementalCross : UnusualCrossNode
{
    // For object pooling
    public CrossSpawner ParentSpawner;

    // Types of elements
    public const int ELEMENTAL_TYPES_COUNT = 3;
    public enum ElementalType { Red = 0, Green = 1, Blue = 2 };
    private ElementalType _elementalType;

    // Physics etc.
    private int _ticksToNextSpawn = 7, _elementsToSpawn, _defaultElementsToSpawn;
    private bool _isLastElementExploded = false;
    private float _xSpriteMotion, _ySpriteMotion = -3;

    private const float GRAVITY = 9.8f;

    // Life cycle
    const float TICKS_TO_APPEAR = 60;
    const float TICKS_TO_START_SPAWNING = 45;
    const float TICKS_TO_CHANGE_TYPE = 20;

    // Rotating
    private float _defaultRotation;
    private float _rotationDirection;

    // Other variables
    private Random _random = new Random();
    private Color _currentDefaultColor;
    private PackedScene _summonableElemental;

    // Nodes
    public Sprite2D Core, RedPart, GreenPart, BluePart;
    public Node2D Sprites;

    // Visual rotating effect
    private bool _shouldRotate = Meta.Instance.Video.CrossRotationWhenSpawning;

    private bool _isDisposed = false;

    public override void _Ready()
    {
        base._Ready();

        // Initializing nodes
        Core = GetNode<Sprite2D>("Sprites/Core");
        RedPart = GetNode<Sprite2D>("Sprites/RedPart");
        GreenPart = GetNode<Sprite2D>("Sprites/GreenPart");
        BluePart = GetNode<Sprite2D>("Sprites/BluePart");
        Sprites = GetNode<Node2D>("Sprites");

        //Spawn properties
        Scale = new Vector2(3, 3);

        _elementalType = (ElementalType)_random.Next(0, ELEMENTAL_TYPES_COUNT);
        if (GetParent() is CrossSpawner spawner)
        {
            ParentSpawner = spawner;
            if (!spawner.EverythingImportant.ContainsKey("RedEESavedPool"))
            {
                spawner.EverythingImportant.Add("RedEESavedPool", new List<ElementalCrossPart>());
                spawner.EverythingImportant.Add("GreenEESavedPool", new List<ElementalCrossPart>());
                spawner.EverythingImportant.Add("BlueEESavedPool", new List<ElementalCrossPart>());
            }
            ElementalType finalType = _elementalType + (int)(TICKS_TO_APPEAR / TICKS_TO_CHANGE_TYPE);
            if ((int)finalType >= ELEMENTAL_TYPES_COUNT)
                finalType = (ElementalType)((int)finalType % (ELEMENTAL_TYPES_COUNT - 1));
            _currentPartsPool = ParentSpawner.EverythingImportant[finalType.ToString() + "EESavedPool"] as List<ElementalCrossPart>;
        }

        if (_shouldRotate)
        {
            _defaultRotation = _random.Next(-30, 30);
            _rotationDirection = _random.Next(2) == 0 ? 2 : -2;

            RotationDegrees = _defaultRotation;
        }

        Sprites.Modulate = new Color(Core.SelfModulate.R, Core.SelfModulate.G, Core.SelfModulate.B, 0);
        _elementsToSpawn = _random.Next(6, 11);
        _defaultElementsToSpawn = _elementsToSpawn;
        _xSpriteMotion = _random.Next(-2, 3);
        ChangeElementType();
    }
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        if (TicksLived < TICKS_TO_APPEAR)
        {
            float TicksCoeff = TicksLived / TICKS_TO_APPEAR;
            TicksCoeff = MathTools.EaseOut(TicksCoeff, 2);

            if (_shouldRotate)
                RotationDegrees += _rotationDirection * Mathf.Sqrt(TicksLived / TICKS_TO_APPEAR);
            Scale = new Vector2(3 - 2 * TicksCoeff, 3 - 2 * TicksCoeff);

            Modulate = new Color(Modulate.R, Modulate.G, Modulate.B, TicksCoeff);
            Sprites.Modulate = new Color(Core.SelfModulate.R, Core.SelfModulate.G, Core.SelfModulate.B, Sprites.SelfModulate.A + 0.0125f);

            if (TicksLived % TICKS_TO_CHANGE_TYPE == 0 && TicksLived > 0)
            {
                _rotationDirection *= -1.25f;
                ChangeElementType();
            }
        }
        if (TicksLived > TICKS_TO_START_SPAWNING)
        {
            if (_summonableElemental == null)
            {
                Modulate = new Color(1, 1, 1);
                _summonableElemental = ResourceLoader.Load<PackedScene>("res://Content/Scenes/Crosses/" + _elementalType.ToString() + "ElementalCrossPart.tscn");
            }
            else if (_elementsToSpawn > 0)
            {
                if (--_ticksToNextSpawn == 0)
                {
                    SpawnElement();

                    Core.Modulate += (new Color(1, 1, 1) - _currentDefaultColor) / _defaultElementsToSpawn;

                    if (_elementsToSpawn <= 0 && _elementalType != ElementalType.Green)
                        ExplodeCore();
                }
            }
            else if (_elementalType != ElementalType.Green || _isLastElementExploded)
            {
                Sprites.Modulate = new Color(Sprites.Modulate.R, Sprites.Modulate.G, Sprites.Modulate.B, Sprites.Modulate.A - 0.02f);
                _ySpriteMotion += GRAVITY / 100;
                RedPart.GlobalTranslate(new Vector2(0.5f, _ySpriteMotion * 2));
                RedPart.GlobalRotation += -0.02f;
                GreenPart.GlobalTranslate(new Vector2(-2, _ySpriteMotion));
                GreenPart.GlobalRotation += -0.04f;
                BluePart.GlobalTranslate(new Vector2(2, _ySpriteMotion));
                BluePart.GlobalRotation += 0.04f;
            }
        }
    }
    public void LastElementFinished()
    {
        OnFinished();
    }
    public void LastElementExploded()
    {
        ExplodeCore();
        _isLastElementExploded = true;
    }

    private List<ElementalCrossPart> _currentPartsPool;
    public async void SpawnElement()
    {
        ElementalCrossPart element;

        _ticksToNextSpawn = 7 + _random.Next(-2, 3);
        _elementsToSpawn--;

        if (_currentPartsPool != null && _currentPartsPool.Count > 0)
        {
            int id = _currentPartsPool.Count - 1;
            element = _currentPartsPool[id];
            _currentPartsPool.RemoveAt(id);
            element.Respawn();
        }
        else
        {
            element = (ElementalCrossPart)_summonableElemental.Instantiate();

            if (_currentPartsPool != null)
                element.Save += () => _currentPartsPool.Add(element);
        }

        AddChild(element);
        element.GlobalRotation = 0;
        element.GlobalPosition = GlobalPosition;
        element.Translate(new Vector2(_random.Next(-30, 31), _random.Next(-30, 31)));

        if (_elementsToSpawn == 0)
        {
            if (_elementalType == ElementalType.Green)
            {
                await ToSignal(element, "Exploded");
                if (_isDisposed) return;
                LastElementExploded();
            }
            await ToSignal(element, "Finished");
            if (_isDisposed) return;
            LastElementFinished();
        }
    }

    private void ExplodeCore()
    {
        GetNode<Node2D>("Sprites/Core").Visible = false;
        GetNode<CpuParticles2D>("Sprites/CoreDestrucionParticles").Emitting = true;
        foreach (var group in GetGroups())
            RemoveFromGroup(group);
    }

    private void ChangeElementType()
    {
        _elementalType++;
        if ((int)_elementalType > ELEMENTAL_TYPES_COUNT - 1)
            _elementalType = 0;

        if (G.CurrentLevel == 5)
            _elementalType = ElementalType.Blue;

        switch (_elementalType)
        {
            case ElementalType.Red:
                Core.Modulate = new Color(1, 0.302f, 0.408f);
                break;
            case ElementalType.Green:
                Core.Modulate = new Color(0.631f, 1, 0.353f);
                break;
            case ElementalType.Blue:
                Core.Modulate = new Color(0.067f, 0.678f, 1);
                break;
        }
        _currentDefaultColor = Core.Modulate;
        GetNode<CpuParticles2D>("ChangeElementParticles").Emitting = true;
    }

    public override void Respawn()
    {
        base.Respawn();

        _ticksToNextSpawn = 7;
        _ySpriteMotion = -3;
        _isLastElementExploded = false;
        _summonableElemental = null;
        GetNode<Node2D>("Sprites/Core").Visible = true;

        Core.Modulate = new Color(1, 1, 1, 1);

        RedPart.Position = new Vector2(0, -31.5f);
        GreenPart.Position = new Vector2(-28, 14);
        BluePart.Position = new Vector2(31.5f, 17.5f);
        RedPart.Rotation = 0;
        GreenPart.Rotation = 0;
        BluePart.Rotation = 0;

        ElementalType finalType = _elementalType + (int)(TICKS_TO_APPEAR / TICKS_TO_CHANGE_TYPE);
        if ((int)finalType >= ELEMENTAL_TYPES_COUNT)
            finalType = (ElementalType)((int)finalType % (ELEMENTAL_TYPES_COUNT - 1));
        _currentPartsPool = ParentSpawner.EverythingImportant[finalType.ToString() + "EESavedPool"] as List<ElementalCrossPart>;

        if (_shouldRotate)
        {
            _defaultRotation = _random.Next(-30, 30);
            _rotationDirection = _random.Next(2) == 0 ? 2 : -2;

            RotationDegrees = _defaultRotation;
        }

        Sprites.Modulate = new Color(Core.SelfModulate.R, Core.SelfModulate.G, Core.SelfModulate.B, 0);
        _elementsToSpawn = _random.Next(6, 11);
        _defaultElementsToSpawn = _elementsToSpawn;
        _xSpriteMotion = _random.Next(-2, 3);
    }

    public override void _ExitTree()
    {
        base._ExitTree();

        _isDisposed = true;
    }
}


abstract public partial class ElementalCrossPart : CrossNode
{
    [Signal] public delegate void ElementExplodedEventHandler();

    public Vector2 PathVec;
    public Vector2 StartPosition;
    public void RandomizePathVec(Rect2 vecBounds)
    {
        StartPosition = GlobalPosition;
        PathVec = RandomTools.RandomVectorIn(vecBounds);
    }
    public float LifeTime, TimeLived = 0f, MoveCoeff = 1;

    public virtual void UpdatePosition(float coeff)
    {
        GlobalPosition = StartPosition + PathVec * MathTools.EaseOut(TimeLived / LifeTime, coeff);
    }
    protected Color _mod = new Color(1, 1, 1, 0);
    const float MODULATE_GROWTH_SPEED = 0.1f;
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        TimeLived += G.FLOAT_DELTA;

        if (TimeLived < LifeTime)
        {
            UpdatePosition(MoveCoeff);

            _mod.A += MODULATE_GROWTH_SPEED;
            CrossSprite.Modulate = _mod;
        }
        else
        {
            EmitSignal("ElementExploded");
            Explode();
        }
    }

    public override void Respawn()
    {
        base.Respawn();

        UpdatePosition(MoveCoeff);
    }

    public override void NodesInit()
    {
        CrossSprite = GetNode<Sprite2D>("Sprite");
        ExplosionAnimation = GetNode<ExplosionAnimation>("ExplosionAnimation");
        ExplosiveArea = GetNode<CollisionShape2D>("ExplosiveArea/CollisionShape2D");
        ExplosionSound = GetNode<AudioStreamPlayer>("ExplosionSound");
    }
}
