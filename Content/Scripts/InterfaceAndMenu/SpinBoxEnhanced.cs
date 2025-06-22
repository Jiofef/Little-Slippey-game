using Godot;
using System;

public partial class SpinBoxEnhanced : SpinBox
{
    [ExportGroup("Editing Properties")]
    [Export] public bool HorizontalChanging = true;
    [Export] public bool VerticalChanging = true;
    [Export] public float MinChangingSpeed = 2;
    [Export] public float MaxChangingSpeed = 20;
    [Export] public float ChangingAccelerationPeriod = 3f;

    [ExportGroup("Step Override")]
    /// <summary>
    /// StepOverride is the step applied when editing with SpinBoxEnhanced functions. Manual input will still use the normal Step
    /// </summary>
    [Export] public bool EnableStepOverride = false;
    [Export] public double StepOverride = 1;

    [ExportGroup("Visual")]
    [Export] public Color DefaultModulate = Colors.White;
    [Export] public Color WhenEditingModulate = new Color(0.5f, 0.5f, 0.5f);
    [Export] public Color WhenFocusedModulate = new Color(0.75f, 0.75f, 0.75f);

    private enum FocusDirection { Left, Top, Right, Bottom }
    private readonly NodePath[] _defaultNeighbors = new NodePath[4];
    private bool _isEditing;
    private float _accelerationTimer;
    private float _changeTimer;

    public override void _Ready()
    {
        FocusEntered += () => Modulate = WhenFocusedModulate;
        FocusExited += () => SetEditingMode(false);

        StoreDefaultFocusNeighbors();
    }

    private void StoreDefaultFocusNeighbors()
    {
        _defaultNeighbors[(int)FocusDirection.Left] = FocusNeighborLeft;
        _defaultNeighbors[(int)FocusDirection.Top] = FocusNeighborTop;
        _defaultNeighbors[(int)FocusDirection.Right] = FocusNeighborRight;
        _defaultNeighbors[(int)FocusDirection.Bottom] = FocusNeighborBottom;
    }

    public override void _Process(double delta)
    {
        if (!HasFocus()) return;

        if (Input.IsActionJustPressed("ui_accept"))
            SetEditingMode(!_isEditing);

        if (!_isEditing) return;

        HandleValueChange((float)delta);
    }

    private void HandleValueChange(float delta)
    {
        bool isDecreasing = Input.IsActionPressed("ui_down") || (HorizontalChanging && Input.IsActionPressed("ui_left"));
        bool isIncreasing = Input.IsActionPressed("ui_up") || (VerticalChanging && Input.IsActionPressed("ui_right"));

        if (isDecreasing) UpdateValue(EnableStepOverride ? -StepOverride : -Step, delta);
        else if (isIncreasing) UpdateValue(EnableStepOverride ? StepOverride : Step, delta);
        else ResetTimers();
    }

    private void UpdateValue(double step, float delta)
    {
        _changeTimer -= delta;
        _accelerationTimer = Mathf.Min(_accelerationTimer + delta, ChangingAccelerationPeriod);

        if (_changeTimer <= 0)
        {
            _changeTimer = 1 / CalculateCurrentSpeed();
            Value += step;
        }
    }

    private float CalculateCurrentSpeed()
    {
        return MinChangingSpeed + (MaxChangingSpeed - MinChangingSpeed) * (_accelerationTimer / ChangingAccelerationPeriod);
    }

    private void ResetTimers()
    {
        _accelerationTimer = 0;
        _changeTimer = 0;
    }

    public void SetEditingMode(bool enabled)
    {
        _isEditing = enabled;
        Modulate = enabled ? WhenEditingModulate : HasFocus() ? WhenFocusedModulate : DefaultModulate;
        UpdateFocusNeighbors(enabled);
    }

    private void UpdateFocusNeighbors(bool editing)
    {
        if (editing)
        {
            FocusNeighborLeft = FocusNeighborTop = FocusNeighborRight = FocusNeighborBottom = ".";
            return;
        }

        FocusNeighborLeft = _defaultNeighbors[(int)FocusDirection.Left];
        FocusNeighborTop = _defaultNeighbors[(int)FocusDirection.Top];
        FocusNeighborRight = _defaultNeighbors[(int)FocusDirection.Right];
        FocusNeighborBottom = _defaultNeighbors[(int)FocusDirection.Bottom];
    }
}