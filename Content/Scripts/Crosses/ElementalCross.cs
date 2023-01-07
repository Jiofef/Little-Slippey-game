using Godot;
using System;

public class ElementalCross : Node2D
{
    private enum ElementalType { Red = 0, Green = 1, Blue = 2 };
    private ElementalType _elementaltype;
    private int _operationstotal = 80, _timertonextspawn, _elementstospawn;
    private bool _doelementsspawnbegun = false;
    private bool _lastelementdeleted = false;
    private readonly int _elementaltypestotal = 3, _defaulttimertonextspawnvalue = 7;
    private float _xspritemotion, _yspritemotion, _gravity = 9.8f;
    private Random random = new Random();
    private PackedScene summonableElemental;
    private Sprite sprite;
    public override void _Ready()
    {
        sprite = GetNode<Sprite>("Sprite");
        sprite.Modulate = new Color(Modulate.r, Modulate.g, Modulate.b, 0);
        _elementstospawn = random.Next(6, 11);
        _timertonextspawn = _defaulttimertonextspawnvalue;
    }
    public override void _PhysicsProcess(float delta)
    {
        if (_lastelementdeleted)
        {
            if (sprite.Modulate.a != 0)
            {
                sprite.Modulate = new Color(sprite.Modulate.r, sprite.Modulate.g, sprite.Modulate.b, sprite.Modulate.a - 0.02f);
                sprite.Position = new Vector2(sprite.Position.x + _xspritemotion, sprite.Position.y + _yspritemotion);
                _yspritemotion += _gravity / 100;
                sprite.Rotation += 0.04f;
                return;
            }
            else QueueFree();
        }

        if (_operationstotal > 0)
        {
            _operationstotal--;
            Scale = new Vector2(Scale.x - 0.025f, Scale.y - 0.025f);
            sprite.Modulate = new Color((float)random.NextDouble(), (float)random.NextDouble(), (float)random.NextDouble(), Modulate.a + 0.0125f);
        }
        else if (!_doelementsspawnbegun)
        {
            _doelementsspawnbegun = true;
            _elementaltype = (ElementalType)random.Next(_elementaltypestotal);
            summonableElemental = ResourceLoader.Load<PackedScene>("res://Content/Scenes/Crosses/" + _elementaltype.ToString() + "ElementalCrossPart.tscn");
            var sprite = GetNode<Sprite>("Sprite");
            switch (_elementaltype)
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
            _timertonextspawn = _timertonextspawn <= 0 ? _defaulttimertonextspawnvalue + random.Next(-2, 3): _timertonextspawn - 1;
            if (_timertonextspawn == 0 && _elementstospawn != 0)
            {
                _elementstospawn--;
                Node2D element = (Node2D)summonableElemental.Instance();
                element.Position = new Vector2(random.Next(-30, 30), random.Next(-30, 30));
                if (_elementstospawn <= 0)
                    element.Connect("ElementExploded", this, "SelfDeletingStart");
                AddChild(element);
            }
        }
    }
    public void SelfDeletingStart()
    {
        _yspritemotion = -3;
        _xspritemotion = random.Next(-2, 3);
        _lastelementdeleted = true;
    }
}
