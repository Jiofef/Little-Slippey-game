using Godot;

public partial class JioYobaFefskiSkinAnimationPlayer : AnimationPlayer
{
	AnimatedSprite2D _sprite;
	public override void _Ready()
	{
		_sprite = GetParent<AnimatedSprite2D>();
	}

	public void SpriteAnimationChanged()
	{
		Play(_sprite.Animation);
	}
}
