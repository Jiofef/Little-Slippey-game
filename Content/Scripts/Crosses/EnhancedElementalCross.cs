using Godot;
using System;
using OtherExtension;
using System.Collections.Generic;

public partial class EnhancedElementalCross : UnusualCrossNode
{
    // For object pooling
    public CrossSpawner ParentSpawner;
    private List<FakeCross> _fakeCrossesPool;

    // Resources
    private PackedScene _fakeCross;

    // Moving
    public const float ACCELERATION_RATIO_MULTIPLIER = 1.005f;
    public float SpeedRatio = 0.01f;
    public float ProgressRatioLinear = 0.0f, ProgressRatio = 0.0f;

    public const int CYCLES_INIT = 25;
    public int CyclesLeft = CYCLES_INIT;

    public Vector2 StartPosition, PathVec;


    // Other
    private Color _mod = new Color(1, 1, 1, 0);

    public override void _Ready()
    {
        _fakeCross = FastInstanceCreator.LoadPackedResScene("Crosses/FakeCross.tscn");

        if (GetParent() is CrossSpawner spawner)
        {
            ParentSpawner = spawner;
            if (!spawner.EverythingImportant.ContainsKey("FakeCrossesSavedPool"))
            {
                spawner.EverythingImportant.Add("FakeCrossesSavedPool", new List<FakeCross>());
            }

            _fakeCrossesPool = spawner.EverythingImportant["FakeCrossesSavedPool"] as List<FakeCross>;
        }

        Vector2 spawnRange = G.GetCameraRect(this).Size;
        SpawnRange = new Rect2(spawnRange / 2, spawnRange);

        Modulate = _mod;
    }
    public override void OnPositionSetted()
    {
        RandomizePathVec();
    }

    public Rect2 SpawnRange;
    public void RandomizePathVec()
    {
        StartPosition = GlobalPosition;

        PathVec = RandomTools.RandomVectorInViewport(this) - StartPosition;
    }

    
    public override void _PhysicsProcess(double delta)
	{
        if (Modulate.A <= 1f)
        {
            _mod.A += 0.01f;
            Modulate = _mod;
        }

        ProgressRatioLinear += SpeedRatio;
        ProgressRatio = MathTools.EaseInOut(ProgressRatioLinear, 3);
        GlobalPosition = StartPosition + PathVec * ProgressRatio;

        SpeedRatio *= ACCELERATION_RATIO_MULTIPLIER;



        if (ProgressRatioLinear >= 1)
        {
            SpawnFakeCross();

            StartPosition = GlobalPosition;
            ProgressRatioLinear = 0f;
            RandomizePathVec();

            CyclesLeft--;
        }

        if (CyclesLeft <= 0)
        {
            GetNode<AnimatedSprite2D>("AnimatedSprite2D").Play("Collapse");
            GetNode<AudioStreamPlayer>("Glitch").Stop();
            GetNode<AudioStreamPlayer>("Finished").Play();
            SetPhysicsProcess(false);
        }
    }

    public virtual void UpdatePosition(float coeff)
    {
        GlobalPosition = StartPosition + PathVec * MathTools.EaseOut(ProgressRatioLinear, coeff);
    }

    public void SpawnFakeCross()
    {
        FakeCross fakeCross;

        if (_fakeCrossesPool != null && _fakeCrossesPool.Count > 0)
        {
            int id = _fakeCrossesPool.Count - 1;
            fakeCross = _fakeCrossesPool[id];
            _fakeCrossesPool.RemoveAt(id);

            fakeCross.Respawn();
        }
        else
        {
            fakeCross = (FakeCross)_fakeCross.Instantiate();
            fakeCross.ShouldBeSavedInPool = ShouldBeSavedInPool;

            if (_fakeCrossesPool != null)
            {
                fakeCross.Save += () => _fakeCrossesPool.Add(fakeCross);
            }

            GetParent().AddChild(fakeCross);
        }

        fakeCross.MoveToFront();
        fakeCross.GlobalRotation = 0;
        fakeCross.Position = Position;
    }

    public override void Respawn()
    {
        base.Respawn();

        CyclesLeft = CYCLES_INIT;
        ProgressRatioLinear = 0;
        SpeedRatio = 0.01f;

        StartPosition = GlobalPosition;

        _mod = new Color(1, 1, 1, 0);
        Modulate = _mod;

        Vector2 spawnRange = G.GetCameraRect(this).Size;
        SpawnRange = new Rect2(spawnRange / 2, spawnRange);

        var sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        sprite.Show();
        sprite.Play("default");

        SetPhysicsProcess(true);

        GetNode<AudioStreamPlayer>("Glitch").Play();
    }
}
