using Godot;
using System;
using static OtherExtension.GodotExtensions;

public partial class RemoteModulate : Node2D
{
    private CanvasItem _remotePath;
	[Export] public CanvasItem RemotePath { 
        get => _remotePath;
        set 
        {
            _remotePath = value;
            UpdateProcess();
        } 
    }

	[Export] public bool UseGlobalModulate = true;

    public override void _Ready()
    {
        UpdateProcess();

        VisibilityChanged += () =>
        {
            UpdateVisiblity();

            if (Visible)
                UpdateProcess();
            else
                SetProcess(false);
        };
    }

    public void UpdateProcess()
    {
        SetProcess(_remotePath != null);
    }

    public void UpdateVisiblity()
    {
        if (_remotePath != null)
            _remotePath.Visible = Visible;
    }

    public override void _Process(double delta)
	{
        if (UseGlobalModulate)
            _remotePath.Modulate = GetGlobalModulateOf(this);
        else
            _remotePath.Modulate = Modulate;
	}
}
