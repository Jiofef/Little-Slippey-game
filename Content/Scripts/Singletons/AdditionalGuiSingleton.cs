using Godot;

public partial class AdditionalGuiSingleton : Node
{
	public static AdditionalGuiLayer Layer;
	public override void _Ready()
	{
		var layer = (AdditionalGuiLayer)GD.Load<PackedScene>("res://Content/Scenes/Interface&Menu/AdditionalGuiLayer.tscn").Instantiate();
		AddChild(layer);

		Layer = G.AdditionalGuiLayer = layer;
	}
}
