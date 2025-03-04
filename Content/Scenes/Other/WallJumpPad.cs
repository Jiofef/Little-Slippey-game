using Godot;
using System;
using System.Runtime.ConstrainedExecution;

public partial class WallJumpPad : Node2D
{
    [Export] public float CharacterToss = 1200f;
    public void TossTheCharacter(Node2D node)
    {
        if (node is not CharacterBody2D character) return;

        if (character is Player player)
        {
            player.CallDeferred("Toss", CharacterToss);
        }
        else
            character.Velocity = new Vector2(0, -CharacterToss);

        // Visual and audio effects
        var animation = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        animation.Stop();
        animation.Play();

        GetNode<AudioStreamPlayer>("TossSound").Play();
    }
}
