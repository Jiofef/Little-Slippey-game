using Godot;
using System;

public partial class RedElementalCrossPart : ElementalCrossPart
{
    public override void _Ready()
    {
        // Initializing nodes
        NodesInit();

        LifeTime = 2.0f;
        MoveCoeff = 2;
        RandomizePathVec(new Rect2(-90, -300, 90 * 2, 240));
        UpdatePosition(MoveCoeff);

        CrossSprite.Modulate = new Color(CrossSprite.Modulate.R, CrossSprite.Modulate.G, CrossSprite.Modulate.B, 0);
    }
}
