using Godot;
using System;

public class ElementalCross : Node2D
{
    private enum ElementalType { Red = 0, Green = 1, Blue = 2 };
    private ElementalType _elementalType;
    private int _ticksToNextPhase = 80, _timerToNextSpawn, _elementsToSpawn;
    private bool _doElementsSpawnBegun = false;
    private bool _lastElementDeleted = false;
    private readonly int _elementalTypesTotal = 3, _defaultNextSpawnTimerValue = 7;
    private float _xSpriteMotion, _ySpriteMotion = -3, _gravity = 9.8f;
    private Random _random = new Random();
    private PackedScene _summonableElemental;
    private Sprite _sprite;
    public override void _Ready()
    {
        _sprite = GetNode<Sprite>("Sprite");
        _sprite.Modulate = new Color(Modulate.r, Modulate.g, Modulate.b, 0);
        _elementsToSpawn = _random.Next(6, 11);
        _timerToNextSpawn = _defaultNextSpawnTimerValue;
        _xSpriteMotion = _random.Next(-2, 3);
    }
    public override void _PhysicsProcess(float delta)
    {
        if (_lastElementDeleted)
        {
            if (_sprite.Modulate.a != 0)
            {
                _sprite.Modulate = new Color(_sprite.Modulate.r, _sprite.Modulate.g, _sprite.Modulate.b, _sprite.Modulate.a - 0.02f);
                _sprite.Position = new Vector2(_sprite.Position.x + _xSpriteMotion, _sprite.Position.y + _ySpriteMotion);
                _ySpriteMotion += _gravity / 100;
                _sprite.Rotation += 0.04f;
                return;
            }
            else QueueFree();
        }

        if (_ticksToNextPhase > 0)
        {
            _ticksToNextPhase--;
            Scale = new Vector2(Scale.x - 0.025f, Scale.y - 0.025f);
            _sprite.Modulate = new Color((float)_random.NextDouble(), (float)_random.NextDouble(), (float)_random.NextDouble(), Modulate.a + 0.0125f);
        }
        else if (!_doElementsSpawnBegun)
        {
            _doElementsSpawnBegun = true;
            _elementalType = (ElementalType)_random.Next(_elementalTypesTotal);
            _summonableElemental = ResourceLoader.Load<PackedScene>("res://Content/Scenes/Crosses/" + _elementalType.ToString() + "ElementalCrossPart.tscn");
            var sprite = GetNode<Sprite>("Sprite");
            switch (_elementalType)
            { 
                case ElementalType.Red:
                    sprite.Modulate = new Color(1, 0, 0);
                    break;
                case ElementalType.Green:
                    sprite.Modulate = new Color(0, 1, 0);
                    break;
                case ElementalType.Blue:
                    sprite.Modulate = new Color(0, 0, 1);
                    break;
            }
        }
        else
        {
            _timerToNextSpawn = _timerToNextSpawn <= 0 ? _defaultNextSpawnTimerValue + _random.Next(-2, 3): _timerToNextSpawn - 1;
            if (_timerToNextSpawn == 0 && _elementsToSpawn != 0)
            {
                _elementsToSpawn--;
                Node2D element = (Node2D)_summonableElemental.Instance();
                element.Position = new Vector2(_random.Next(-30, 30), _random.Next(-30, 30));
                if (_elementsToSpawn <= 0)
                    element.Connect("ElementExploded", this, "SelfDeletingStart");
                AddChild(element);
            }
        }
    }
    public void SelfDeletingStart()
    {
        _lastElementDeleted = true;
    }
}
