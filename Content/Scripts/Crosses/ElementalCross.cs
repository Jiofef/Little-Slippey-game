using Godot;
using System;

public partial class ElementalCross : Node2D
{
    private enum ElementalType { Red = 0, Green = 1, Blue = 2 };
    private ElementalType _elementalType;
    private int _ticksToNextSpawn, _elementsToSpawn;
    private bool _isLastElementExploded = false;
    private readonly int _defaultTicksToNextElementSpawn = 7;
    private float _xSpriteMotion, _ySpriteMotion = -3, _gravity = 9.8f;

    private Random _random = new Random();
    private PackedScene _summonableElemental;
    private Sprite2D _sprite;

    public override void _Ready()
    {
        _sprite = GetNode<Sprite2D>("Sprite2D");
        _sprite.SelfModulate = new Color(_sprite.SelfModulate.R, _sprite.SelfModulate.G, _sprite.SelfModulate.B, 0);
        _elementsToSpawn = _random.Next(6, 11);
        _ticksToNextSpawn = _defaultTicksToNextElementSpawn;
        _xSpriteMotion = _random.Next(-2, 3);
    }
    public override void _PhysicsProcess(double delta)
    {
        if (Scale.X > 1 && Scale.Y > 1 && _sprite.SelfModulate.A < 1)
        {
            Scale = new Vector2(Scale.X - 0.025f, Scale.Y - 0.025f);
            _sprite.Modulate = new Color((float)_random.NextDouble(), (float)_random.NextDouble(), (float)_random.NextDouble());
            _sprite.SelfModulate = new Color(_sprite.SelfModulate.R, _sprite.SelfModulate.G, _sprite.SelfModulate.B, _sprite.SelfModulate.A + 0.0125f);
        }
        else if (_summonableElemental == null)
        {
            Modulate = new Color(1, 1, 1);
            _elementalType = (ElementalType)_random.Next(3);

            if (G.CurrentLevel == 5)
                _elementalType = ElementalType.Blue;

            _summonableElemental = ResourceLoader.Load<PackedScene>("res://Content/Scenes/Crosses/" + _elementalType.ToString() + "ElementalCrossPart.tscn");
            switch (_elementalType)
            { 
                case ElementalType.Red:
                    _sprite.Modulate = new Color(1, 0, 0);
                    break;
                case ElementalType.Green:
                    _sprite.Modulate = new Color(0, 1, 0);
                    break;
                case ElementalType.Blue:
                    _sprite.Modulate = new Color(0, 0, 1);
                    break;
            }
        }
        else if (_elementsToSpawn > 0)
        {
            _ticksToNextSpawn = _ticksToNextSpawn <= 0 ? _defaultTicksToNextElementSpawn + _random.Next(-2, 3): _ticksToNextSpawn - 1;
            if (_ticksToNextSpawn == 0)
            {
                _elementsToSpawn--;
                Node2D element = (Node2D)_summonableElemental.Instantiate();
                element.Position = new Vector2(_random.Next(-30, 30), _random.Next(-30, 30));
                if (_elementsToSpawn == 0)
                    element.Connect("ElementExploded", new Callable(this, "LastElementExploded"));
                AddChild(element);
            }
        }
        else if (_isLastElementExploded)
        {
            _sprite.Modulate = new Color(_sprite.Modulate.R, _sprite.Modulate.G, _sprite.Modulate.B, _sprite.Modulate.A - 0.02f);
            _sprite.Translate(new Vector2(_xSpriteMotion, _ySpriteMotion));
            _ySpriteMotion += _gravity / 100;
            _sprite.Rotation += 0.04f;
             
            if (_sprite.Modulate.A <= 0)
                QueueFree();
        }
    }
    public void LastElementExploded()
    {
        _isLastElementExploded = true;

        var Groups = GetGroups();
        for (int i = 0; i < Groups.Count; i++)
            RemoveFromGroup(Groups[i]);
    }
}
