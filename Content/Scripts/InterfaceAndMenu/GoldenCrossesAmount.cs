using Godot;
using System;

public partial class GoldenCrossesAmount : MarginContainer
{
	[Export] public bool UpdateAutomatically = true, NotificateWhenCannotBuy = true, NotificateWhenBought = true;
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
		if (NotificateWhenCannotBuy)
		{
			UnchangableMeta.CannotBuy += OnCannotBuy;

			TreeExited += () =>
			{
				UnchangableMeta.CannotBuy -= OnCannotBuy;
			};
		}
        if (NotificateWhenBought)
        {
            UnchangableMeta.Bought += OnBought;

            TreeExited += () =>
            {
                UnchangableMeta.Bought -= OnBought;
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

	public void OnCannotBuy()
	{
        G.PlayOneshotSound("Interface&Menu/CantBuy.mp3", GetTree().Root, "Interface");
    }

	public void OnBought()
	{
        G.PlayOneshotSound("Interface&Menu/Buy.mp3", GetTree().Root, "Interface");
    }
}
