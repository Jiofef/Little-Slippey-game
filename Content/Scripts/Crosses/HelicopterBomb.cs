using Godot;
using System;
using OtherExtension;

public partial class HelicopterBomb : CrossNode
{
	float _rotationVelocity, _velocity = 0;
	public override void _Ready()
	{
		NodesInit();

		_rotationVelocity = RandomTools.RandomIn(-2.5f, 2.5f);
        AddToGroup("Crosses");

		GetNode<Area2D>("Hitbox").BodyEntered += (a) => Explode();
    }


	public override void _PhysicsProcess(double delta)
	{
		Translate(new Vector2(0, _velocity));
		_velocity += 0.15f;

		RotationDegrees += _rotationVelocity;
	}

	public override void Explode()
	{
		base.Explode();

		GetNode<CollisionShape2D>("Hitbox/CollisionShape2D").SetDeferred("disabled", true);

        GetNode<Timer>("MaxLifeTimer").Stop();
    }

    public override void NodesInit()
    {
		CrossSprite = GetNode<Sprite2D>("CrossSprite");
		ExplosiveArea = GetNode<CollisionShape2D>("ExplosiveArea/CollisionShape2D");
		ExplosionAnimation = GetNode<ExplosionAnimation>("ExplosionAnimation");
		ExplosionSound = GetNode<AudioStreamPlayer>("ExplosionSound");
    }

    public override void Respawn()
    {
        base.Respawn();

        // Returning the old settings
        CrossSprite.Visible = true;
		RotationDegrees = 0;

        ExplosionAnimation.Visible = false;

        AddToGroup("Crosses");

        _velocity = 0;
        _rotationVelocity = RandomTools.RandomIn(-2.5f, 2.5f);
		ExplosiveArea.SetDeferred("disabled", false);

        GetNode<Timer>("Hitbox/EnablingTimer").Start();
        GetNode<Timer>("MaxLifeTimer").Start();
    }
}
