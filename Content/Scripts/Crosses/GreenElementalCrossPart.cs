using Godot;
using System;
using OtherExtension;

public partial class GreenElementalCrossPart : ElementalCrossPart
{
    private const int _flowerSpritesCount = 4;
    private Sprite2D _vineSprite;
    public override void _Ready()
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

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        if (TimeLived < LifeTime)
        {
            _vineSprite.RegionRect = new Rect2(0, 0, new Vector2(GlobalPosition.DistanceTo(GlobalPosition + PathVec * TimeLived / LifeTime) / 4, 7));
        }
        else if (IsExploded)
        {
            _vineSprite.Modulate = new Color(_vineSprite.Modulate.R - 0.03f, _vineSprite.Modulate.G - 0.03f, _vineSprite.Modulate.B - 0.03f, _vineSprite.Modulate.A - 0.03f);
        }
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
        GetNode<CpuParticles2D>("Vine/FireParticles").Emitting = true;
    }

    public override void Respawn()
    {
        base.Respawn();

        // ¬Œ“ “”“  –»¬¿
        _vineSprite.LookAt(StartPosition - PathVec);
    }
}
