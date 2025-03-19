using Godot;
using System;
using OtherExtension;

public partial class ExplosionAnimation : AnimatedSprite2D
{
	[Export] public bool DeleteWhenFinished = true;
	[Export] public bool HideWhenFinished = true;
	[ExportGroup("Bloom")]
	[Export] public bool EnableBloom = true;
	[Export] public Color BloomColor = new Color(1.0f, 0.627f, 0.322f, 0.5f);
	[Export] public CompressedTexture2D BloomTextureOverride;
	public override void _Ready()
	{
		VisibilityChanged += () =>
		{
			if (Visible) 
				GlobalRotation = 0;
		};

		AnimationFinished += () => {
			if (DeleteWhenFinished)
				QueueFree();
			else if (HideWhenFinished)
				Hide();
		};

		if (EnableBloom && Meta.Instance.Video.ExplosionBloom)
			CreateBloomSprite();
	}

	public void CreateBloomSprite()
	{
		Sprite2D sprite = FastInstanceCreator.LoadResScene<Sprite2D>("Crosses/ExplosionBloom.tscn");

		if (BloomTextureOverride != null)
			sprite.Texture = BloomTextureOverride;

		AddChild(sprite);

		VisibilityChanged += () =>
		{
			if (!Visible) return;
			double animDuration = SpriteFrames.GetFrameCount("default") / SpriteFrames.GetAnimationSpeed("default");

			sprite.Modulate = BloomColor with {A = 0};
            CreateTween().TweenProperty(sprite, "modulate", new Color(BloomColor.R, BloomColor.G, BloomColor.B, BloomColor.A), animDuration / 2);
			var tween2 = CreateTween().TweenProperty(sprite, "modulate", new Color(BloomColor.R, BloomColor.G, BloomColor.B, 0), animDuration / 2);
			tween2.SetDelay(animDuration / 2);
		};
	}
}
