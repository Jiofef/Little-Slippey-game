using Godot;
using System;

public partial class Level7HopelessnessLayer : CanvasLayer
{
	PackedScene _mindTumor = new PackedScene();

	private float _tumorSpawnTimer = 10;

    public override void _ExitTree()
    {
        AudioServer.SetBusEffectEnabled(0, 0, false);
    }

    public override void _PhysicsProcess(double delta)
    {
        _tumorSpawnTimer -= 0.016667f;
		if (_tumorSpawnTimer <= 0)
		{
			_tumorSpawnTimer = 30;

			for (int i = 0; i < (G.Scores >= 120 ? 2 : 1); i++)
			{
				var mindTumor = ResourceLoader.Load<PackedScene>("res://Content/Scenes/Other/Level7MindTumor.tscn").Instantiate<Area2D>();
				Random random = new Random();
                mindTumor.Position = new Vector2(random.Next(-512, 512), random.Next(-288, 288));
                GetNode<CharacterBody2D>("../Player").AddChild(mindTumor);
            }
		}
    }
    public void AgeTheSound()
    {
        AudioServer.SetBusEffectEnabled(0, 0, true);
    }
}
