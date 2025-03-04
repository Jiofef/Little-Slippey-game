using Godot;
using System;

public partial class Bonus : Node2D
{
	[Signal] public delegate void CollectedEventHandler();
	public void OnPlayerEntered()
	{
		// Visual effects
		var orb = GetNode<AnimatedSprite2D>("Orb");
		var tween = CreateTween();

		tween.TweenProperty(orb, property: "scale", Vector2.Zero, 0.5).SetEase(Tween.EaseType.Out);

		// Particles
		GetNode<CpuParticles2D>("CPUParticles2D").Emitting = true;

		EmitSignal(nameof(Collected));

        OnCollected(G.Player);
    }

	public virtual void OnCollected(Player player) { }
}
