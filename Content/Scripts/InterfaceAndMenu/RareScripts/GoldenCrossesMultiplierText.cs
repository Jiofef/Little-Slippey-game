using Godot;
using System;
using static Additions;

public partial class GoldenCrossesMultiplierText : RichTextLabel
{
	public override void _Ready()
	{
		ContentManager.Inst.Connect(nameof(ContentManager.Inst.AdditionsGameplayEffectsUpdated), new Callable(this, nameof(UpdateText)));
		UpdateText();
	}

	public void UpdateText()
	{
		int percentMultiplierValue = (int)(GameplayEffects.GoldenCrossesMultiplier * 100);
		Text = $"[center]{Tr("Golden crosses multiplier")}\n\n{percentMultiplierValue}%[img]res://Content/Sprites/Interface/4XMiniGoldenCross.png[/img]";
	}
}
