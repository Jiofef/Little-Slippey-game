using Godot;
using System;
using System.Threading.Tasks;
using OtherExtension;
using static OtherExtension.RandomTools;
using static OtherExtension.GeometryTools;
using static OtherExtension.GodotExtensions;
using static G;
using System.Linq;
using System.Collections.Generic;
using System.Reflection.PortableExecutable;

public partial class GlitchLevelScript : BaseLevelScript
{
    // Nodes
    private Player _player;
    private MainScript _main;

    // Other variables
    Random _random = new Random();
    private List<Vector2I> _spawnedGlitchBlocks = [];
    private TileMapLayer _glitchTileMap, _unHologramedTiles;
    private ColorRect _dark;


    public Dictionary<string, Node2D> LevelParts = new Dictionary<string, Node2D>();
    private float _glitchBlockDeSpawnSpeed = 1f;
    private float _maxGlitchBlocks = 13;

	private float _scoresUpdateChance = 10f;
	private bool _enableGlitchedTilesSpawning = false;

    // For async methods
    private bool _isDisposed = false;

    public class GlitchShader
    {
        public Shader Shader;
        public float ShakePower
        {
            get => (float)Shader.Get("shake_power"); 
            set => Shader.Set("shake_power", value);
        }

        public float ShakeRate
        {
            get => (float)Shader.Get("shake_rate");
            set => Shader.Set("shake_rate", value);
        }

        public float ShakeSpeed
        {
            get => (float)Shader.Get("shake_speed");
            set => Shader.Set("shake_speed", value);
        }

        public float ShakeBlockSize
        {
            get => (float)Shader.Get("shake_block_size");
            set => Shader.Set("shake_block_size", value);
        }

        public float ShakeColorRate
        {
            get => (float)Shader.Get("shake_color_rate");
            set => Shader.Set("shake_color_rate", value);
        }
    }
    GlitchShader _glitchShader = new();
    
    public override void _Ready()
    {
        G.LevelAdditionalLink = "000000000";
        base._Ready();
        //Initializing nodes
        _player = GetNode<Player>("Player");
        _main = GetNode<MainScript>("..");

        _glitchTileMap = GetNode<TileMapLayer>("GlitchTileMap");
        _unHologramedTiles = GetNode<TileMapLayer>("UnHologramedTiles");

        _dark = GetNode<ColorRect>("CanvasLayer/Dark");

        _glitchShader.Shader = ((ShaderMaterial)GetNode<ColorRect>("../AdditionalGUILayer/GlitchRect").Material).Shader;

        // "Caching" the level parts
        foreach (var levelPart in GetNode("LevelParts").GetChildren().OfType<Node2D>())
            LevelParts.Add(levelPart.Name, levelPart);

        GetNode<AudioStreamPlayer>("CanvasLayer/Glare/ShuttingDownTheTestChamber").Play();

        // For async methods
        TreeExited += () => _isDisposed = true;
		TreeEntered += () => _isDisposed = false;

        //Removing transitive values from level 10
        G.TransitiveVariant[0] = "";
        G.TransitiveObject[0] = null;
        G.TransitiveObject[1] = null;

        //Loading saved values from dictionary
        float savedScores = (float)G.TakeAndRemoveFromTrVaD("SavedScores");
        G.Scores = savedScores;

        Vector2 playerSavedPos = (Vector2)G.TakeAndRemoveFromTrVaD("PlayerSavedPos");
        _player.Position = playerSavedPos;

        //Binding important events
        _main.LevelReset += OnLevelReset;

        // !!
        G.IsCrossesEnabled = false;
        G.Scores = -5;
        G.IsProgressPaused = true;

        // Binding the film layer signal
        var filmLayer = FindNodeOfType<Level7HopelessnessLayer>(this);
        if (filmLayer != null)
        {
            filmLayer.NegativeValueSquared += ProgressScript;
        }
    }

    public void OnLevelReset()
    {
        G.TransitiveVariantD.Add("ImFromLevel000000000", true);
        LevelAdditionalLink = "True";
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
    }

    public async void ProgressScript() // Starts when the timer brokes due to the root of the negative number
    {
        // When film jams
        var darkeningTween =_dark.CreateTween();
        darkeningTween.TweenProperty(_dark, "modulate", new Color(1, 1, 1, 1), 4f);
		try { 
        await ToSignal(darkeningTween, "finished");

        ChangeScene(Scenes.Level3);
        var lighteringTween =_dark.CreateTween();
        lighteringTween.TweenProperty(_dark, "modulate", new Color(1, 1, 1, 0.235f), 4f);
        await ToSignal(lighteringTween, "finished");
        
        await ToScore(75, () => _isDisposed);
		Scores = 0;
        ChangeScene(Scenes.Level4);

        await ToScore(75, () => _isDisposed);
		Scores = 0;
        ChangeScene(Scenes.Level5);

        await ToScore(75, () => _isDisposed);
		Scores = 0;
        ChangeScene(Scenes.Level6);

        await ToScore(75, () => _isDisposed);
		Scores = 0;
        ChangeScene(Scenes.Level7);
		} catch (ObjectDisposedException) {return;}

    }


	public override void _EnterTree()
	{
		base._EnterTree();
	}
    public async void GlitchTheScreen()
    {
        float lastShakeRate = _glitchShader.ShakeRate;
        _glitchShader.ShakeRate = 1;

        await Task.Delay(333);
        _glitchShader.ShakeRate = lastShakeRate;
    }

    public Node2D GetPart(string name)
    {
        return GetNode<Node2D>("LevelParts/" + name);
    }

    #region TileMaps manipulating
    public void SetChildrenTileMapsEnabled(Node parent, bool value)
    {
        foreach (var child in parent.GetChildren().OfType<TileMapLayer>())
        {
            child.Enabled = value;
        }
    }

	public void SetChildrenTileMapsCollision(Node parent, bool value)
	{
		foreach (var child in parent.GetChildren().OfType<TileMapLayer>())
        {
            child.CollisionEnabled = value;
        }
	}
    public void SpawnRandomGlitchBlock(Rect2 spawnRect)
    {
        Rect2 correctedSpawnRect = new Rect2(
            spawnRect.Position / _glitchTileMap.GlobalScale / _glitchTileMap.TileSet.TileSize,
            spawnRect.Size / _glitchTileMap.GlobalScale / _glitchTileMap.TileSet.TileSize);

        Vector2I spawnPos = RandomVectorIn(correctedSpawnRect);

        if (_spawnedGlitchBlocks.Contains(spawnPos))
            return;

        int sourceId = _random.Next(_glitchTileMap.TileSet.GetSourceCount());
        Vector2I atlasCoord = RandomVectorIn(((TileSetAtlasSource)_glitchTileMap.TileSet.GetSource(sourceId)).GetAtlasGridSize());
        _glitchTileMap.SetCell(spawnPos, sourceId, atlasCoord);

        _spawnedGlitchBlocks.Add(spawnPos);
    }

    public void DespawnRandomGlitchBlock()
    {
        if (_spawnedGlitchBlocks.Count == 0) return;

        int blockIndex = _random.Next(_spawnedGlitchBlocks.Count);

        _glitchTileMap.SetCell(_spawnedGlitchBlocks[blockIndex]);

        _spawnedGlitchBlocks.RemoveAt(blockIndex);
    }

    public void DespawnRandomNonGlitchBlock(TileMapLayer tileMap)
    {
        Vector2I[] usedCells = tileMap.GetUsedCells().ToArray();
        if (usedCells.Length == 0) return;

        tileMap.SetCell(usedCells.GetRandom());
    }

    public void DespawnAllTheRandomGlitchBlocks()
    {
        foreach (Vector2I block in _spawnedGlitchBlocks)
            _glitchTileMap.SetCell(block);

        _spawnedGlitchBlocks.Clear();
    }

	public void OnGlitchBlockTimerFinished()
	{
		float  spawnChance = _spawnedGlitchBlocks.Count == 0 ? 1 : 1f -_spawnedGlitchBlocks.Count / _maxGlitchBlocks;
		if (TryRand(spawnChance, 1f))
			SpawnRandomGlitchBlock(CameraLimits);

		if (TryRand(50))
			DespawnRandomGlitchBlock();
	}

	public void SetGlitchBlockSpawning(bool value)
	{
		_enableGlitchedTilesSpawning = value;
	}
    public void CopyToUnHologramedTileMap(params TileMapLayer[] tileMaps)
    {
        _unHologramedTiles.Clear();
        foreach (var tileMap in tileMaps)
        {
            foreach (Vector2I cell in tileMap.GetUsedCells())
            {
                _unHologramedTiles.SetCell(cell, 0, new Vector2I(0, 1));
            }
        }
    }
    #endregion

    #region Visual or small script effects
    Dictionary<Effects, bool> EffectsCycleDic = new Dictionary<Effects, bool>
    {
        { Effects.GlitchBlocksSpawning, false },
        { Effects.Level7BackgroundChanging, false },
        { Effects.Level7SadWritings, false },
    };
    public async void CycleTheEffect(Effects effect, Func<Task> task)
    {
        EffectsCycleDic[effect] = true;
        while (EffectsCycleDic[effect])
        {
            await task();
        }
    }
    public void StopEffectCycle(Effects effect)
    {
        EffectsCycleDic[effect] = false;
    }
    public enum Effects {GlitchBlocksSpawning, Level7BackgroundChanging, Level7SadWritings}
    public async void ActivateEffect(Effects effect)
    {
        switch (effect)
        {
            case Effects.GlitchBlocksSpawning:
                CycleTheEffect(effect, async () =>
                {
                    const int CYCLE_DELAY_MS = 3000;

                    const int DEFAULT_CHANCE_TO_SPAWN = 75;
                    float chanceToDespawnPerSpawnedBlock = (100 - DEFAULT_CHANCE_TO_SPAWN) / _maxGlitchBlocks;
                    float chanceToSpawn = DEFAULT_CHANCE_TO_SPAWN - chanceToDespawnPerSpawnedBlock * _spawnedGlitchBlocks.Count;

                    if (TryRand(chanceToSpawn))
                    {
                        Rect2 spawnRect = GeometryTools.RectFromCenter(G.Player.GlobalPosition, new Vector2(2560, 1440));
                        SpawnRandomGlitchBlock(spawnRect);
                    }
                    else
                    {
                        DespawnRandomGlitchBlock();
                    }

                    int waitTime = _random.Next((int)(CYCLE_DELAY_MS * _glitchBlockDeSpawnSpeed));
                    await Task.Delay(waitTime);
                });
                break;

            case Effects.Level7BackgroundChanging:
                CycleTheEffect(effect, async () =>
                {
                    var level7Background1 = GetNode<ParallaxBackground>("Level/LevelParts/Level7/Background");
                    var level7Background2 = GetNode<ParallaxBackground>("Level/LevelParts/Level7/Background2");

                    level7Background1.Visible = true;
                    level7Background2.Visible = false;

                    const int MAX_MS_DELAY = 4259;
                    await Task.Delay(_random.Next(MAX_MS_DELAY));

                    level7Background1.Visible = false;
                    level7Background2.Visible = true;

                    await Task.Delay(_random.Next(MAX_MS_DELAY));
                });
                break;

            case Effects.Level7SadWritings:
                var writings1 = GetNode<AppearingText>("LevelParts/Level7/Background/SadWritings/IBurnedAllThe");
                var writings2 = GetNode<AppearingText>("LevelParts/Level7/Background/SadWritings/IBurnedDownMy");

                writings1.Show();
                writings1.SetAppearing(true);

                await ToSignal(writings1, "AppearingFinished");
                await Task.Delay(2000);

                writings1.Hide();

                writings2.Show();
                writings2.SetAppearing(true);

                await ToSignal(writings1, "AppearingFinished");

                writings2.Hide();
                GlitchTheScreen();

                ChangeScene(Scenes.Revelation);
                break;
        }
    }    

    public void GlareEffect()
    {
        var colorRect = GetNode<ColorRect>("CanvasLayer/Glare");
        colorRect.GetNode<AnimationPlayer>("AnimationPlayer").Play();

        colorRect.GetNode<AudioStreamPlayer>("LampBulp").Play();
    }

	private List<string>  _availableScoresTexts = 
	[""];
	public void TryRandomizeScoresLabelText()
	{
		if (TryRand(_scoresUpdateChance))
			_player.GUI.SetScoresText(GetRandomGlitchedShit());
	}
    #endregion
// 
    public enum Scenes {Default = 0, Level3 = 1, Level4 = 2, Level5 = 3, Level6 = 4, Level7 = 5, Revelation = 6}
    private int _lastSceneNumber = 0;
    /// <summary>
    /// Doesn't actually change the scene, it switches the stages of the level
    /// </summary>
    public async void ChangeScene(Scenes scene)
    {
        // In order not to face undesirable consequences of async and godot node interaction
        await ToSignal(GetTree(), "process_frame");

        // So that everyone who needs to know what stage of the level is now
        _lastSceneNumber = (int)scene;

        switch (scene)
        {
            case Scenes.Default:
                break;

            case Scenes.Level3:
				GetNode<Timer>("GlitchActionsTimer").Start();
				SetGlitchBlockSpawning(true);
				_maxGlitchBlocks = 5;
                _unHologramedTiles.CollisionEnabled = false;
				_unHologramedTiles.Visible = false;
                _player.GlobalPosition = new Vector2(1280, 640);

                SwitchSceneNode(LevelParts["Default"], LevelParts["Level3"]);

                await WaitFor(7f, false);
                // Enabling the lights
                GetNode<AnimationPlayer>("LevelParts/Level3/Background/AppearingAnimationPlayer").Play("BackgroundAppearing");

                // Fun begins
                Scores = 0;
                IsProgressPaused = false;
                IsCrossesEnabled = true;
				CrossesProgressCoeff = 2f;

                MusicPlayer.PlayMusic("Born To Pyramid Demo");

                break;

            case Scenes.Level4:
				_scoresUpdateChance = 20f;
				_maxGlitchBlocks = 25;
				DespawnAllTheRandomGlitchBlocks();
                CopyToUnHologramedTileMap(LevelParts["Level3"].GetNode<TileMapLayer>("TileMap"));
				_player.AddImmortality(5f);

				SetChildrenTileMapsCollision(LevelParts["Level3"], false);
                await Task.WhenAll(MoveNodeTo(_player, new Vector2(1280, 640)), HideNodeSlowly(LevelParts["Level3"]));

				Crosses.RemoveAllSpawnedCrosses();
				Crosses.UpdateCrossesWeight();
				MusicPlayer.NextStage();
				SwitchSceneNode(LevelParts["Level3"], LevelParts["Level4"]);
				LevelParts["Level4"].ProcessMode = ProcessModeEnum.Inherit;
				SetChildrenTileMapsEnabled(LevelParts["Level4"].GetNode("TileMap"), true);
				LevelParts["Level4"].GetNode<ParallaxBackground>("Background").Show();
                GlareEffect();


                break;

            case Scenes.Level5:
				_scoresUpdateChance = 30f;
				_maxGlitchBlocks = 20;
				DespawnAllTheRandomGlitchBlocks();
                CopyToUnHologramedTileMap(LevelParts["Level4"].GetNode<TileMapLayer>("TileMap/Layer0"));
				_player.AddImmortality(5f);

				SetChildrenTileMapsCollision(LevelParts["Level4"], false);
				SetChildrenTileMapsCollision(LevelParts["Level4"].GetNode("TileMap"), false);
                await Task.WhenAll(MoveNodeTo(_player, new Vector2(1280, 1248)), HideNodeSlowly(LevelParts["Level4"]));

				Crosses.RemoveAllSpawnedCrosses();
				Crosses.UpdateCrossesWeight();
				MusicPlayer.NextStage();
				SwitchSceneNode(LevelParts["Level4"], LevelParts["Level5"]);
				LevelParts["Level5"].ProcessMode = ProcessModeEnum.Inherit;
				LevelParts["Level5"].GetNode<ParallaxBackground>("Background").Show();
                GlareEffect();

                Scores = 0;
                break;

            case Scenes.Level6:
				_scoresUpdateChance = 60f;
				_maxGlitchBlocks = 40;
				DespawnAllTheRandomGlitchBlocks();
                CopyToUnHologramedTileMap(LevelParts["Level5"].GetNode<TileMapLayer>("Layer1"));
				_player.AddImmortality(5f);

				SetChildrenTileMapsCollision(LevelParts["Level5"], false);
                await Task.WhenAll(MoveNodeTo(_player, new Vector2(1216, 320)), HideNodeSlowly(LevelParts["Level5"]));
				
				Crosses.RemoveAllSpawnedCrosses();
				Crosses.UpdateCrossesWeight();
				MusicPlayer.NextStage();
				SwitchSceneNode(LevelParts["Level5"], LevelParts["Level6"]);
				LevelParts["Level6"].ProcessMode = ProcessModeEnum.Inherit;
                GlareEffect();

                Scores = 0;
                break;

            case Scenes.Level7:
				_scoresUpdateChance = 20f;
				_maxGlitchBlocks = 10;
				DespawnAllTheRandomGlitchBlocks();
                CopyToUnHologramedTileMap(LevelParts["Level6"].GetNode<TileMapLayer>("TileMap"), LevelParts["Level6"].GetNode<TileMapLayer>("ToEnable"));
				_player.AddImmortality(5f);

				SetChildrenTileMapsCollision(LevelParts["Level6"], false);
                await Task.WhenAll(MoveNodeTo(_player, new Vector2(640, 320)), HideNodeSlowly(LevelParts["Level6"]));
				
				Crosses.RemoveAllSpawnedCrosses();
				Crosses.UpdateCrossesWeight();
				MusicPlayer.StopMusic(5f);
				SwitchSceneNode(LevelParts["Level6"], LevelParts["Level7"]);
				LevelParts["Level7"].ProcessMode = ProcessModeEnum.Inherit;
				LevelParts["Level7"].GetNode<ParallaxBackground>("Background").Show();
                GlareEffect();

                Scores = 0;
                break;

            case Scenes.Revelation:
				_scoresUpdateChance = 80f;
				SetGlitchBlockSpawning(false);
                // Removing the floor
                Rect2I rectToRemove = (Rect2I)PosEndRect(new Vector2I(0, 10), new Vector2(19, 12));
                SetRectOnTileMap(_glitchTileMap, rectToRemove);
				LevelParts["Revelation"].ProcessMode = ProcessModeEnum.Inherit;

                _player.Gravity = Player.DEFAULT_GRAVITY / 2; 
                break;
        }
    }

    public void SwitchSceneNode(Node2D from, Node2D to)
    {
        // Visibility
        from.QueueFree();
        to.Visible = true;

        // TileMaps
        SetChildrenTileMapsEnabled(to, true);
    }    
}