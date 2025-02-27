using Godot;
using System;

public partial class ModulateTransmitter : Node2D
{
    private CanvasItem _modulateSource;
    [Export]
    public CanvasItem ModulateSource
    {
        get => _modulateSource;
        set
        {
            _modulateSource = value;
            UpdateProcess();

            if (value != null)
                _modulateSource.VisibilityChanged += UpdateVisiblity;
        }
    }

    private CanvasItem _target;
    [Export]
    public CanvasItem Target
    {
        get => _target;
        set
        {
            _target = value;
            UpdateProcess();

            UpdateVisiblity();
        }
    }

    public bool BothSetted()
    {
        return _modulateSource != null && _target != null;
    }

    public override void _Ready()
    {
        UpdateProcess();
    }

    public void UpdateProcess()
    {
        SetProcess(BothSetted());
    }

    public void UpdateVisiblity()
    {
        if (BothSetted())
            _target.Visible = Visible;
    }

    public override void _Process(double delta)
    {
        _target.Modulate = _modulateSource.Modulate;
    }
}
