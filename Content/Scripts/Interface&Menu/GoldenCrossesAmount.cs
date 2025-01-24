using Godot;
using System;

public partial class GoldenCrossesAmount : MarginContainer
{
	[Export] public bool UpdateAutomatically = true;
	UnchangableMeta.GoldenCrossesAmountChangedEventHandler GoldenCrossesAmountChanged;

	public Label AmountLabel;

	public override void _Ready()
	{
		// Initializing the nodes
		AmountLabel = GetNode<Label>("HBoxContainer/AmountLabel");

        UpdateAmount();

		if (UpdateAutomatically)
		{
			UnchangableMeta.GoldenCrossesAmountChanged += UpdateAmount;

			TreeExited += () =>
			{
				UnchangableMeta.GoldenCrossesAmountChanged -= UpdateAmount;
			};
		}
	}

	public void UpdateAmount(int value)
	{
		UpdateAmount();
    }
    public void UpdateAmount()
    {
        AmountLabel.Text = UnchangableMeta.GoldenCrossesAmount.ToString();
    }
}
