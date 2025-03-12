using Godot;
using System;
using OtherExtension;

public partial class GreenElementalCrossPart : ElementalCrossPart
{
    private const int _flowerSpritesCount = 4;
    private Sprite2D _vineSprite;
    public override async void _Ready()
    {
        // Initializing nodes
        NodesInit();

        _vineSprite = GetNode<Sprite2D>("Vine");
        Random random = new Random();
        CrossSprite.Texture = ResourceLoader.Load("res://Content/Sprites/Crosses/GreenElementalCrossPartVar" + (random.Next(_flowerSpritesCount) + 1) + ".png") as Texture2D;

        LifeTime = 1.5f;
        MoveCoeff = 2;
        RandomizePathVec(new Rect2(-225, -60, 170, 120));
        PathVec.X *= RandomTools.FiftyFifty() ? 1 : -1; // Mirroring the vec randomly
        UpdatePosition(MoveCoeff);

        _vineSprite.LookAt(StartPosition - PathVec);

        _vineSprite.GlobalScale = new Vector2(4, 4);
        CrossSprite.Modulate = _mod;
        GetNode<CpuParticles2D>("ScrapsParticles").Emitting = true;


    }

    public override void NodesInit()
    {
        CrossSprite = GetNode<Sprite2D>("Sprite");
        ExplosionAnimation = GetNode<ExplosionAnimation>("Sprite/ExplosionAnimation");
        ExplosiveArea = GetNode<CollisionShape2D>("Sprite/ExplosiveArea/CollisionShape2D");
        ExplosionSound = GetNode<AudioStreamPlayer>("ExplosionSound");
    }

    public override void Explode()
    {
        base.Explode();

        CrossSprite.Visible = true;
        CrossSprite.SelfModulate = new Color(1, 1, 1, 0);
        var fireParticles = GetNode<CpuParticles2D>("Vine/FireParticles");
        fireParticles.Position = new Vector2(_vineSprite.RegionRect.Size.X / 2, 0);
        fireParticles.EmissionRectExtents = new Vector2(fireParticles.Position.X * 9, 1);
        fireParticles.Emitting = true;

        // Vine sprite disappearing (burning)
        CreateTween().TweenProperty(_vineSprite, "modulate", new Color(0, 0, 0, 0), 0.5);
    }

    public override void UpdatePosition(float coeff)
    {
        coeff = MathTools.EaseOut(TimeLived / LifeTime, coeff);
        GlobalPosition = StartPosition + PathVec * coeff;

        _vineSprite.RegionRect = new Rect2(0, 0, new Vector2(GlobalPosition.DistanceTo(GlobalPosition + PathVec * coeff) / 4, 7));
    }

    public override void Respawn()
    {
        base.Respawn();

        _vineSprite.LookAt(StartPosition - PathVec);
    }
}
