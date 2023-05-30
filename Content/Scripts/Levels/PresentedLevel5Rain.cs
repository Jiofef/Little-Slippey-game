using Godot;
using System;

public partial class PresentedLevel5Rain : Node2D
{
    ColorRect _fog, _thunderLight;

    private Color _defaultFogColor = new Color(0, 0.12f, 0.12f, 0.70f);
    private Random _random = new Random();

    public override void _Ready()
    {
        _fog = GetNode<ColorRect>("Fog?");
        _thunderLight = GetNode<ColorRect>("ThunderLight");
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_thunderLight.Color.A > 0)
            _thunderLight.Color = new Color(_thunderLight.Color.R, _thunderLight.Color.G, _thunderLight.Color.B, _thunderLight.Color.A - 0.0075f);
    }

    public void LightChange()
    {
        _fog.Color = new Color(_defaultFogColor.R, _defaultFogColor.G, _defaultFogColor.B, _fog.Color.A + 0.003f);
    }

    public void ThunderStart()
    {
        var thunderTimer = GetNode<Timer>("ThunderTimer");
        thunderTimer.WaitTime = _random.Next(15, 25);
        thunderTimer.Start();
        _fog.Color = _defaultFogColor;

        _thunderLight.Color = new Color(_thunderLight.Color.R, _thunderLight.Color.G, _thunderLight.Color.B, 1);
    }
}
