using Godot;
using System;

public class ElementalCrossFirstForm : Node2D
{
    private float _replacemodulate = 0, _operationstotal = 40, _deletetimer = 60;
    public override void _Ready()
    {
        Modulate = new Color(Modulate.r, Modulate.g, Modulate.b, 0);
    }
    public override void _PhysicsProcess(float delta)
    {
        
    }
}
