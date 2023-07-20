using Godot;
using System;

public partial class Level5Rain : Node2D
{
    ColorRect _fog, _thunderLight;
    Camera2D _playerCamera;
    CpuParticles2D _particles;

    private Color _defaultFogColor = new Color(0, 0.12f, 0.12f, 0.70f);
    private Random _random = new Random();

    public override void _Ready()
    {
        _fog = GetNode<ColorRect>("CanvasLayer/Fog?");
        _thunderLight = GetNode<ColorRect>("CanvasLayer/ThunderLight");
        _playerCamera = GetNode<Camera2D>("../Player/Camera2D");
        _particles = GetNode<CpuParticles2D>("Particles");

        ThunderStart();
    }

    public override void _PhysicsProcess(double delta)
    {
        _particles.GlobalPosition = new Vector2(_playerCamera.GlobalPosition.X, _particles.GlobalPosition.Y);

        if (_thunderLight.Color.A > 0)
            _thunderLight.Color = new Color(_thunderLight.Color.R, _thunderLight.Color.G, _thunderLight.Color.B, _thunderLight.Color.A - 0.0075f);

        if (_random.Next(600) == 0)
        {
            GetNode<AudioStreamPlayer>("AmbiencePlayer" + (_random.Next(2) + 1)).Play();
        }
    }

    public void LightChange()
    {
        _fog.Color = new Color(_defaultFogColor.R, _defaultFogColor.G, _defaultFogColor.B, _fog.Color.A + 0.003f);
    }

    public void ThunderStart()
    {
        GetNode<AudioStreamPlayer>("ThunderSound").Play();

        var thunderTimer = GetNode<Timer>("ThunderTimer");
        thunderTimer.WaitTime = _random.Next(15, 25);
        thunderTimer.Start();
        _fog.Color = _defaultFogColor;

        _thunderLight.Color = new Color(_thunderLight.Color.R, _thunderLight.Color.G, _thunderLight.Color.B, 1);
    }
}
