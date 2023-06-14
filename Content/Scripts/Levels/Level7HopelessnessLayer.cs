using Godot;
using System;

public partial class Level7HopelessnessLayer : CanvasLayer
{
	PackedScene _mindTumor = new PackedScene();

	float _tumorSpawnTimer = 10;

    public override void _PhysicsProcess(double delta)
    {
        _tumorSpawnTimer -= 0.01667f;
		if (_tumorSpawnTimer <= 0)
		{
			_tumorSpawnTimer = 30;

			for (int i = 0; i < (G.Scores >= 120 ? 2 : 1); i++)
			{
				var mindTumor = ResourceLoader.Load<PackedScene>("res://Content/Scenes/Other/Level7MindTumor.tscn").Instantiate<Area2D>();
				Random random = new Random();

                mindTumor.Position = new Vector2(random.Next(-425, 425), random.Next(-240, 240));
                GetNode<CharacterBody2D>("../Player").AddChild(mindTumor);
			}
		}
    }
}
