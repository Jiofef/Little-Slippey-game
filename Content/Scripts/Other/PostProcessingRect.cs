using Godot;
using System;

public partial class PostProcessingRect : ColorRect
{
	private enum Mode { Brightness, Saturation}
	[Export] float _value = 1;
	[Export] Mode _mode = Mode.Brightness;

	ShaderMaterial _material;

	public override void _Ready()
	{
		_material = (ShaderMaterial)Material;
	}

	public override void _Process(double delta)
	{
		_material.SetShaderParameter(_mode.ToString().ToLower(), _value);
    }
}
