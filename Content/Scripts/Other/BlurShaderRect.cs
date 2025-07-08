using Godot;
using System;

public partial class BlurShaderRect : ColorRect
{
	private float _lod = 0;
	[Export]
	public float LOD
	{
		get => _lod;
		set
		{
			if (_shaderMaterial == null) return;

			_lod = value;
			_shaderMaterial.Set("shader_parameter/lod", value);
		}
	}
	private ShaderMaterial _shaderMaterial;
	public override void _Ready()
	{
		if (Material is ShaderMaterial shaderMaterial)
		{
			_shaderMaterial = shaderMaterial;
		}
		else
		{
			ShaderMaterial newShaderMaterial = new ShaderMaterial();
			newShaderMaterial.Shader = GD.Load<Shader>("res://Content/Other/BlurShader.gdshader");
			_shaderMaterial = newShaderMaterial;
		}
		LOD = _lod;
	}

	public override void _ExitTree()
	{
		// Resetting the shader material options
		LOD = 0;
	}
}
