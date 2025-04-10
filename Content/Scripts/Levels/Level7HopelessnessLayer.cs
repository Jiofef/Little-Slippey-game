using Godot;
using System;
using System.Collections;
using System.Threading.Tasks;
using static OtherExtension.ActionTools;

public partial class Level7HopelessnessLayer : CanvasLayer
{
    [Signal] public delegate void NegativeValueSquaredEventHandler();

	PackedScene _mindTumor = new PackedScene();

	private float _tumorSpawnTimer = 10;


    AnimatedSprite2D _0;
    public override void _Ready()
    {
        _mindTumor = ResourceLoader.Load<PackedScene>("res://Content/Scenes/Other/Level7MindTumor.tscn");
        BindEventToNodeSafelyWithoutArgs(this, "OnLevelStarted", G.OnLevelStarted);

        _0 = GetNode<AnimatedSprite2D>("2D");
    }
    private void OnLevelStarted()
    {
        GetNode<AudioStreamPlayer>("FilmCracking").Playing = true;
    }
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
                var mindTumor = _mindTumor.Instantiate<Area2D>();
                Random random = new Random();
                mindTumor.Position = new Vector2(random.Next(-512, 512), random.Next(-288, 288));
                GetNode<CharacterBody2D>("../Player").AddChild(mindTumor);

                mindTumor.AreaEntered += (area2D) => n2evf7yUH3ZLT3x3N0___();
            }
        }
    }

    public void AgeTheSound()
    {
        AudioServer.SetBusEffectEnabled(0, 0, true);
    }



    Color _savedScoresModulate;
    public void n2evf7yUH3ZLT3x3N0___() // On hole entered
    {
        _savedScoresModulate = G.Player.GUI.Scores.Modulate;

        G.Player.GUI.Scores.Modulate = new Color(0, 0, 0);

        _0.Show();
        _0.Play();
        GetNode<AudioStreamPlayer>("2D/0").Play();
    }

    public void __() // On _0 animation finished
    {
        GetNode<Timer>("RootTimer").Start();
        _0.Hide();

        Node root = GetTree().Root;

        var endOfEverything = (Level7EndOfAnEverything)GD.Load<PackedScene>("res://Content/Scenes/Other/EndOfAnEverything.tscn").Instantiate();
        endOfEverything.Backup = G.Main;

        root.CallDeferred("remove_child", G.Main);

        root.AddChild(endOfEverything);
    }

    public async void Root() // On RootTimer timeout
    {
        G.Player.GUI.Scores.Modulate = _savedScoresModulate;

        if (G.Scores >= 0)
            G.Scores = Mathf.Sqrt(G.Scores);
        else
        {
            EmitSignal(nameof(NegativeValueSquared));

            G.Player.GUI.SetScoresText("NaN");

            // Jaming the film
            var vintageFilm = GetNode<VideoStreamPlayer>("VintageFilter");
            G.PlayOneshotSound("Levels/Level10WTH.mp3", this, "Master", 12);
            vintageFilm.Stream = GD.Load<VideoStreamTheora>("res://Content/Other/FilmJam.ogv");

            vintageFilm.Play();
            _ = OtherExtension.GodotExtensions.ShowNodeSlowly(vintageFilm, 1f);
            await G.WaitFor(4f);
            var tween = CreateTween();
            tween.TweenProperty(vintageFilm, "position", new Vector2(0, -720), 0.2f);

            G.PlayOneshotSound("Levels/Level7FilmJamming.mp3", this, "Master", 15);

            await ToSignal(tween, "finished");

            //* Need to do something with scores label

            QueueFree();
        }
    }
}
