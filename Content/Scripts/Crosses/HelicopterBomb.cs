using Godot;
using System;

public partial class HelicopterBomb : Node2D
{
	float _rotationVelocity, _velocity;
	public override void _Ready()
	{
		Random random = new Random();
		_rotationVelocity = (float)random.Next(-250, 250) / 100;
        AddToGroup("Crosses");
    }


	public override void _PhysicsProcess(double delta)
	{
		var explosiveArea = GetNode<CollisionShape2D>("ExplosiveArea/CollisionShape2D");
		if (!explosiveArea.Disabled)
		{
			explosiveArea.SetDeferred("disabled", true);
			SetPhysicsProcess(false);
		}

		Translate(new Vector2(0, _velocity));
		_velocity += 0.15f;

		RotationDegrees += _rotationVelocity;
	}

	public void Explosion()
	{
        GetNode<CollisionShape2D>("ExplosiveArea/CollisionShape2D").SetDeferred("disabled", false);
		GetNode<Sprite2D>("Sprite2D").QueueFree();
		GetNode<Area2D>("Hitbox").QueueFree();

		var explosionAnimation = GetNode<AnimatedSprite2D>("ExplosionAnimation");
		explosionAnimation.Visible = true;
		explosionAnimation.Play();

		GetNode<AudioStreamPlayer>("ExplosionSound").Play();

        var Groups = GetGroups();
        for (int i = 0; i < Groups.Count; i++)
            RemoveFromGroup(Groups[i]);
    }
}
