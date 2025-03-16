using Godot;
using OtherExtension;
using System;

public partial class BlueElementalCrossPart : ElementalCrossPart
{
    public override void _Ready()
    {
        // Initializing nodes
        NodesInit();

        LifeTime = 1.25f;
        MoveCoeff = 2;
        SpawnVecBounds = new Rect2(-125, 360 / 5, 125 * 2, 360 - 360 / 5);
        RandomizePathVec();
        UpdatePosition(MoveCoeff);

        CrossSprite.Modulate = new Color(CrossSprite.Modulate.R, CrossSprite.Modulate.G, CrossSprite.Modulate.B, 0);

        var spawnSound = GetNode<AudioStreamPlayer>("SpawnSound");
        Random random = new Random();
        spawnSound.Stream = ResourceLoader.Load<AudioStream>("res://Content/Sounds/Crosses/BlueElementalCrossPartSoundVar" + (random.Next(3) + 1) + ".mp3");
        spawnSound.Play();
    }

    public override void UpdatePosition(float coeff)
    {
        GlobalPosition = StartPosition + PathVec * MathTools.EaseIn(TimeLived / LifeTime, coeff);
    }
}
