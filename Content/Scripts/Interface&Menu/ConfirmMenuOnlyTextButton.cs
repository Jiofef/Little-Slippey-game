using Godot;
using System;

[Tool]
public partial class ConfirmMenuOnlyTextButton : MenuOnlyTextButton
{
    public override void _Ready()
    {
        base._Ready();

        FocusExited += OnFocusExited;
    }

    #region Confirmating
    private string _confirmationText = "Press again to confirm";
    [Export(PropertyHint.MultilineText)]
    public string ConfirmationText
    {
        get => _confirmationText;
        set
        {
            _confirmationText = value;

            if (_isWaitingForConfirmation)
            {
                SetLabelText(value);
            }
        }
    }

    private bool _isWaitingForConfirmation;
    public bool IsWaitingForConfirmation { get => _isWaitingForConfirmation; }

    public override void _Pressed()
    {
        // We don't need to call base because it's calling TryBuy every time
        // base._Pressed();

        if (_isWaitingForConfirmation)
        {
            Confirm();
        }
        else if (!EnableBuying || UnchangableMeta.CanBuy(BuyingPrice))
        {
            WaitForConfirmation();
        }
    }
    public void WaitForConfirmation()
    {
        _isWaitingForConfirmation = true;

        SetLabelText(_confirmationText);
    }

    public void Confirm()
    {
        EmitSignal("Confirmed");
        AConfirmed?.Invoke();

        if (EnableBuying)
            TryBuy();

        DeclineConfirmation();
    }

    [Signal] public delegate void ConfirmedEventHandler();
    public Action AConfirmed;
    public virtual void OnConfirmed(){ }
    #endregion


    #region Declining
    public void DeclineConfirmation()
    {
        if (!_isWaitingForConfirmation) return;
        _isWaitingForConfirmation = false;

        // It returns the text to the original state
        UpdateText();
    }

    [Export] public bool DeclineConfirmationWhenUnfocused = true;
    public void OnFocusExited()
    {
        if (DeclineConfirmationWhenUnfocused)
            DeclineConfirmation();
    }
    #endregion

}
