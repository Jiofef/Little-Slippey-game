using Godot;
using System;
using OtherExtension;

public partial class ElementalCross : UnusualCrossNode
{
    private enum ElementalType { Red = 0, Green = 1, Blue = 2 };
    private ElementalType _elementalType;
    private int _ticksToNextSpawn = 7, _elementsToSpawn, _defaultElementsToSpawn;
    private bool _isLastElementExploded = false;
    private float _xSpriteMotion, _ySpriteMotion = -3, _gravity = 9.8f;

    private float _defaultTicksToAppear = 60;
    private float _ticksToAppear = 0;

    private float _defaultRotation;
    private float _rotationDirection;

    private Random _random = new Random();
    private Color _currentDefaultColor;
    private PackedScene _summonableElemental;
    public Sprite2D Core, RedPart, GreenPart, BluePart;
    public Node2D Sprites;

    // Visual rotating effect
    private bool _shouldRotate = Meta.Instance.Video.CrossRotationWhenSpawning;

    public override void _Ready()
    {
        // Initializing nodes
        Core = GetNode<Sprite2D>("Sprites/Core");
        RedPart = GetNode<Sprite2D>("Sprites/RedPart");
        GreenPart = GetNode<Sprite2D>("Sprites/GreenPart");
        BluePart = GetNode<Sprite2D>("Sprites/BluePart");
        Sprites = GetNode<Node2D>("Sprites");

        //Spawn properties
        Scale = new Vector2(3, 3);

        _elementalType = (ElementalType)_random.Next(0, 3);

        _ticksToAppear = _defaultTicksToAppear;

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
        if (_ticksToAppear > 0)
        {
            _ticksToAppear--;
            float TicksCoeff = 1 - (_ticksToAppear / _defaultTicksToAppear);
            TicksCoeff = Mathf.Lerp(0.0f, 1.0f, 1 - (1 - TicksCoeff) * (1 - TicksCoeff) * (1 - TicksCoeff));

            if (_shouldRotate)
                RotationDegrees += _rotationDirection * Mathf.Sqrt(_ticksToAppear / _defaultTicksToAppear);
            Scale = new Vector2(3 - 2 * TicksCoeff, 3 - 2 * TicksCoeff);

            Modulate = new Color(Modulate.R, Modulate.G, Modulate.B, TicksCoeff);
            Sprites.Modulate = new Color(Core.SelfModulate.R, Core.SelfModulate.G, Core.SelfModulate.B, Sprites.SelfModulate.A + 0.0125f);

            if (_ticksToAppear % 20 == 0 && _ticksToAppear > 0)
            {
                _rotationDirection *= -1.25f;
                ChangeElementType();
            }
        }
        if (_ticksToAppear < 15)
        {
            if (_summonableElemental == null)
            {
                Modulate = new Color(1, 1, 1);
                _summonableElemental = ResourceLoader.Load<PackedScene>("res://Content/Scenes/Crosses/" + _elementalType.ToString() + "ElementalCrossPart.tscn");
            }
            else if (_elementsToSpawn > 0)
            {
                _ticksToNextSpawn--;
                if (_ticksToNextSpawn == 0)
                {
                    _ticksToNextSpawn = 7 + _random.Next(-2, 3);

                    _elementsToSpawn--;
                    Node2D element = (Node2D)_summonableElemental.Instantiate();
                    if (_elementsToSpawn == 0)
                    {
                        element.Connect("tree_exited", new Callable(this, "LastElementExited"));
                        if (_elementalType == ElementalType.Green)
                            element.Connect("ElementExploded", new Callable(this, "LastElementExploded"));
                    }


                    AddChild(element);
                    element.GlobalRotation = 0;
                    element.Position = new Vector2(_random.Next(-30, 31), _random.Next(-30, 31));

                    Core.Modulate += (new Color(1, 1, 1) - _currentDefaultColor) / _defaultElementsToSpawn;

                    if (_elementsToSpawn <= 0 && _elementalType != ElementalType.Green)
                        ExplodeCore();
                }
            }
            else if (_elementalType != ElementalType.Green || _isLastElementExploded)
            {
                Sprites.Modulate = new Color(Sprites.Modulate.R, Sprites.Modulate.G, Sprites.Modulate.B, Sprites.Modulate.A - 0.02f);
                _ySpriteMotion += _gravity / 100;
                RedPart.GlobalTranslate(new Vector2(0.5f, _ySpriteMotion * 2));
                RedPart.GlobalRotation += -0.02f;
                GreenPart.GlobalTranslate(new Vector2(-2, _ySpriteMotion));
                GreenPart.GlobalRotation += -0.04f;
                BluePart.GlobalTranslate(new Vector2(2, _ySpriteMotion));
                BluePart.GlobalRotation += 0.04f;
            }
        }
    }
    public void LastElementDeleted()
    {
        OnFinished();
    }
    public void LastElementExploded()
    {
        ExplodeCore();
        _isLastElementExploded = true;
    }

    private void ExplodeCore()
    {
        GetNode("Sprites/Core").QueueFree();
        GetNode<CpuParticles2D>("Sprites/CoreDestrucionParticles").Emitting = true;
        foreach (var group in GetGroups())
            RemoveFromGroup(group);
    }

    private void ChangeElementType()
    {
        _elementalType += 1;
        if ((int)_elementalType > 2)
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

        Core.Modulate = new Color(1, 1, 1, 1);

        RedPart.Position = new Vector2(0, -31.5f);
        GreenPart.Position = new Vector2(-28, 14);
        BluePart.Position = new Vector2(31.5f, 17.5f);

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
}


abstract public partial class ElementalCrossPart : CrossNode
{
    public Vector2 PathVec;
    public Vector2 StartPosition;
    public void RandomizePathVec(Rect2 vecBounds)
    {
        StartPosition = Position;
        PathVec = RandomTools.RandomVectorIn(vecBounds);
    }
    public float LifeTime, TimeLived = 0f;

    public void UpdatePosition(float coeff)
    {
        Position = StartPosition + PathVec * (TimeLived / LifeTime);
    }

    public override void NodesInit()
    {
        CrossSprite = GetNode<Sprite2D>("Sprite");
        ExplosionAnimation = GetNode<ExplosionAnimation>("ExplosionAnimation");
        ExplosiveArea = GetNode<CollisionShape2D>("ExplosiveArea/CollisionShape2D");
        ExplosionSound = GetNode<AudioStreamPlayer>("ExplosionSound");
    }
}
