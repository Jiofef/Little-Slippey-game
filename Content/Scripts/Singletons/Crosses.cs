using Godot;
using System;
using System.Collections.Generic;
using OtherExtension;
using System.Linq;
using static G;
using static OtherExtension.RandomTools;

public partial class Crosses : Node
{
    static Random _random = new Random();
    public class Cross
    {
        public PackedScene PackedScene;

        public float MaxWeightGainTime;

        public float Weight;

        public enum SpawnRectModeEnum { Viewport, CameraLimits }
        public SpawnRectModeEnum SpawnRectMode = SpawnRectModeEnum.Viewport;

        public float MinimumSpawnDistanceToPlayer = 0;

        public Rect2 SpawnRectCorrection = new Rect2(-80, -80, 80, 80);

        public bool IsSelfPositioningOnSpawn = false;

        public Dictionary<string, bool> AllowedSpawnSidesDic = new Dictionary<string, bool>
        {
            {"FullRect", true },
            {"TopLeft", false },
            {"Top", false },
            {"TopRight", false },
            {"Left", false },
            {"Middle", false },
            {"Right", false },
            {"BottomLeft", false },
            {"Bottom", false },
            {"BottomRight", false },
        };

        public string[] OnlyAllowedSpawnSides =
        {
            "FullRect"
        };

        /// <summary>
        /// When you create a class and change the sides via SetSpawnSide, it is updated automatically. However, if you change AllowedSpawnSidesDic manually, call UpdateOnlyAllowedSpawnSidesArray for correct work
        /// </summary>
        public void UpdateOnlyAllowedSpawnSidesArray()
        {
            OnlyAllowedSpawnSides = AllowedSpawnSidesDic
                .Where(pair => pair.Value == true)
                .Select(pair => pair.Key)
                .ToArray();
        }


        public string GetRandomAllowedSide()
        {
            return OnlyAllowedSpawnSides[_random.Next(OnlyAllowedSpawnSides.Length)];
        }


        public Cross(PackedScene packedScene, float maxWeightGainTime = 30f, float weight = 1f, float minimumDistanceToPlayer = 0)
        {
            SetPackedScene(packedScene);

            MaxWeightGainTime = maxWeightGainTime;
            Weight = weight;
            MinimumSpawnDistanceToPlayer = minimumDistanceToPlayer;

            UpdateOnlyAllowedSpawnSidesArray();
        }
        /// <summary>
        /// vanillaName can be Cross1, EnhancedCross3, GoldenCross etc.
        /// </summary>
        public Cross(string vanillaName, float maxWeightGainTime = 30f, float weight = 1f, float minimumSpawnDistanceToPlayer = 0)
        {
            SetPackedScene(vanillaName);

            MaxWeightGainTime = maxWeightGainTime;
            Weight = weight;
            MinimumSpawnDistanceToPlayer = minimumSpawnDistanceToPlayer;

            UpdateOnlyAllowedSpawnSidesArray();
        }


        public void SetPackedScene(PackedScene packedScene)
        {
            PackedScene = packedScene;
        }
        /// <summary>
        /// vanillaName can be Cross1, EnhancedCross3, GoldenCross etc.
        /// </summary>
        public void SetPackedScene(string vanillaName)
        {
            PackedScene = GD.Load<PackedScene>("res://Content/Scenes/Crosses/" + vanillaName + ".tscn");
        }

        List<UnusualCrossNode> savedPool = new();
        public UnusualCrossNode GetInstance(out bool isReusing)
        {
            UnusualCrossNode inst;

            if (savedPool.Count > 0)
            {
                int id = savedPool.Count - 1;
                inst = savedPool[id];
                savedPool.RemoveAt(id);
                inst.Respawn();

                isReusing = true;
                return inst;
            }

            inst = (UnusualCrossNode)PackedScene.Instantiate();
            inst.TreeExited += () => savedPool.Remove(inst);
            inst.Save += () => savedPool.Add(inst);
            inst.ShouldBeSavedInPool = true;

            isReusing = false;
            return inst;
        }


        public void ResetSpawnSides()
        {
            foreach(var item in AllowedSpawnSidesDic)
            {
                AllowedSpawnSidesDic[item.Key] = false;
            }

            AllowedSpawnSidesDic["FullRect"] = true;
        }

        public void SetSpawnSide(bool fullRect, bool middle = false, bool left = false, bool right = false, bool top = false, bool bottom = false)
        {
            ResetSpawnSides();

            AllowedSpawnSidesDic["FullRect"] = fullRect;
            AllowedSpawnSidesDic["Middle"] = middle;
            AllowedSpawnSidesDic["Left"] = left;
            AllowedSpawnSidesDic["Right"] = right;
            AllowedSpawnSidesDic["Top"] = top;
            AllowedSpawnSidesDic["Bottom"] = bottom;

            UpdateOnlyAllowedSpawnSidesArray();
        }

        public void SetSpawnSide(bool fullRect, bool middle = false, bool left = false, bool right = false, bool top = false, bool bottom = false, bool topLeft = false, bool topRight = false, bool bottomLeft = false, bool bottomRight = false)
        {
            ResetSpawnSides();

            AllowedSpawnSidesDic["FullRect"] = fullRect;
            AllowedSpawnSidesDic["Middle"] = middle;

            AllowedSpawnSidesDic["Left"] = left;
            AllowedSpawnSidesDic["Right"] = right;
            AllowedSpawnSidesDic["Top"] = top;
            AllowedSpawnSidesDic["Bottom"] = bottom;

            AllowedSpawnSidesDic["TopLeft"] = topLeft;
            AllowedSpawnSidesDic["TopRight"] = topRight;
            AllowedSpawnSidesDic["BottomLeft"] = bottomLeft;
            AllowedSpawnSidesDic["BottomRight"] = bottomRight;

            UpdateOnlyAllowedSpawnSidesArray();
        }
    }


    public static string CurrentPackName;
    public static Cross[] CurrentCrossesPack;

    public static Cross CurrentGoldenCross;
    public static bool EnableGoldenCrossSpawning, KeepGoldenCrossSpawningWhenProgressProgress = false;
    public const int DEFAULT_GOLDEN_CROSS_RARITY = 100;
    public static int GoldenCrossSpawnRarity;

    public int CurrentCrossesCount => CurrentCrossesPack.Length + (CurrentGoldenCross != null ? 1 : 0);

    private static float[] _defaultCrossesWeight = new float[0];


    public static Cross GetCross(int index)
    {
        return CurrentCrossesPack[index];
    }
    public static CanvasItem GetCrossInstance(int index, out bool isReusing)
    {
        var inst = CurrentCrossesPack[index].GetInstance(out bool _isReusing);
        isReusing = _isReusing;
        return inst;
    }

    public static void SetDefaultCrossesPack()
    {
        if (CurrentPackName == "LightweightTNT1.0") return;

        Cross[] crosses = {
            new Cross("Cross1", 30, 600),
            new Cross("Cross2", 30, 170),
            new Cross("Cross3", 30, 80),
            new Cross("Cross4", 30, 40, 400),
            new Cross("Cross5", 30, 110),
        };
        Cross goldenCross = new Cross("GoldenCross");
        goldenCross.SpawnRectMode = Cross.SpawnRectModeEnum.CameraLimits;
        goldenCross.MinimumSpawnDistanceToPlayer = 350;
        KeepGoldenCrossSpawningWhenProgressProgress = false;

        // Cannon cross setting up
        crosses[4].IsSelfPositioningOnSpawn = true;

        SetCurrentCrossesPack("LightweightTNT1.0", crosses, goldenCross);
    }

    public static void SetEnhancedCrossesPack()
    {
        if (CurrentPackName == "LightweightTNT2.0") return;

        Cross[] crosses = {
            new Cross("EnhancedCross1", 30, 650),
            new Cross("EnhancedCross2", 30, 265),
            new Cross("EnhancedCross3", 30, 20),
            new Cross("EnhancedCross4", 30, 15, 400),
            new Cross("EnhancedCross5", 30, 25),
        };
        Cross goldenCross = new Cross("EnhancedGoldenCross");
        goldenCross.SpawnRectMode = Cross.SpawnRectModeEnum.CameraLimits;
        goldenCross.MinimumSpawnDistanceToPlayer = 350;
        KeepGoldenCrossSpawningWhenProgressProgress = false;

        // Helicopter cross setting up
        crosses[4].SetSpawnSide(false, false, false, false, true, false); // Helicopter must spawn at the top camera limit corner
        crosses[4].SpawnRectCorrection = new Rect2(-640, -1280, 640, 0);

        SetCurrentCrossesPack("LightweightTNT2.0", crosses, goldenCross, 125);
    }

    /// <summary>
    /// If you want the golden crosses to not spawn, leave null there
    /// </summary>
    public static void SetCurrentCrossesPack(string packName, Cross[] crosses, Cross goldenCross = null, int goldenCrossSpawnRarity = DEFAULT_GOLDEN_CROSS_RARITY)
    {
        if (CurrentPackName == packName) return;

        if (goldenCross != null)
        {
            EnableGoldenCrossSpawning = true;
            CurrentGoldenCross = goldenCross;
        }
        else
        {
            EnableGoldenCrossSpawning = false;
            CurrentGoldenCross = null;
        }
        GoldenCrossSpawnRarity = goldenCrossSpawnRarity;

        CurrentPackName = packName;
        CurrentCrossesPack = crosses;

        _defaultCrossesWeight = new float[crosses.Length];
        for (int i = 0; i < _defaultCrossesWeight.Length; i++)
        {
            _defaultCrossesWeight[i] = crosses[i].Weight;
        }

        ResetLocalValues();
    }

    /// <summary>
    /// Returns a random cross guided by the maximum weight, not the current weight
    /// </summary>
    public static Cross GetGlobalRandomCross()
    {
        if (EnableGoldenCrossSpawning && _random.Next(GoldenCrossSpawnRarity) == 0) return CurrentGoldenCross;

        return PickRandomByWeight(CurrentCrossesPack, _defaultCrossesWeight);
    }

    // Local values
    public static float[] CrossesWeight;

    public static int LastAviableCrossNumber = 0;
    public static float LastCheckedScoresValue = 0;

    public static bool AreAllCrossWeightsSet;
    public static void UpdateCrossesWeight()
    {
        // Calculate the weight multiplier based on scores and progress coefficient
        float weightMultiplier = Scores / 30 * CrossesProgressCoeff;

        CrossesWeight = new float[_defaultCrossesWeight.Length];
        LastAviableCrossNumber = 0;

        AreAllCrossWeightsSet = false;

        // Distribute the weight across the crosses
        while (weightMultiplier > 0 && LastAviableCrossNumber < CrossesWeight.Length)
        {
            // Calculate the remaining weight that can be added to the current cross
            float remainingWeight = _defaultCrossesWeight[LastAviableCrossNumber] - CrossesWeight[LastAviableCrossNumber];

            // Determine how much weight to add (either the remaining weight or the full multiplier)
            float weightToAdd = Math.Min(weightMultiplier * _defaultCrossesWeight[LastAviableCrossNumber], remainingWeight);

            // Add the calculated weight to the current cross
            CrossesWeight[LastAviableCrossNumber] += weightToAdd;

            // Reduce the multiplier by the proportion of weight added
            weightMultiplier -= weightToAdd / _defaultCrossesWeight[LastAviableCrossNumber];

            // If the current cross has reached its maximum weight
            if (CrossesWeight[LastAviableCrossNumber] >= _defaultCrossesWeight[LastAviableCrossNumber])
            {
                LastAviableCrossNumber++;
            }
        }

        // If all crosses have been processed
        if (LastAviableCrossNumber >= CrossesWeight.Length)
        {
            AreAllCrossWeightsSet = true;
            LastAviableCrossNumber = CrossesWeight.Length - 1;
        }

        OtherTools.PrintAnArray(CrossesWeight);
    }

    private static float _lastWeightUpdateScore = 0;
    private static float _currentCrossWeightTimer = 0;
    private static void UpdateLastCrossWeight()
    {
        if (AreAllCrossWeightsSet) return;

        _currentCrossWeightTimer += Scores - _lastWeightUpdateScore;
        _lastWeightUpdateScore = Scores;

        float weightCapTime = CurrentCrossesPack[LastAviableCrossNumber].MaxWeightGainTime / CrossesProgressCoeff;

        if (_currentCrossWeightTimer < weightCapTime)
        {
            CrossesWeight[LastAviableCrossNumber] = _defaultCrossesWeight[LastAviableCrossNumber] * _currentCrossWeightTimer / weightCapTime;
        }
        else
        {
            _currentCrossWeightTimer -= weightCapTime;
            CrossesWeight[LastAviableCrossNumber] = _defaultCrossesWeight[LastAviableCrossNumber];
            LastAviableCrossNumber++;

            if (LastAviableCrossNumber >= CrossesWeight.Length)
            {
                LastAviableCrossNumber = CrossesWeight.Length - 1;
                AreAllCrossWeightsSet = true;
            }
        }
    }


    /// <summary>
    /// For correct operation, call only on the level. In the interface it is better to use GetGlobalRandomCross
    /// </summary>
    public static Cross GetLocalRandomCross()
    {
        UpdateLastCrossWeight();

        if (EnableGoldenCrossSpawning && (KeepGoldenCrossSpawningWhenProgressProgress || !IsProgressPaused) && _random.Next(GoldenCrossSpawnRarity) == 0) return CurrentGoldenCross;

        return PickRandomByWeight(CurrentCrossesPack, CrossesWeight, false);
    }

    public static void ResetLocalValues()
    {
        CrossesWeight = new float[_defaultCrossesWeight.Length];

        LastAviableCrossNumber = 0;
        LastCheckedScoresValue = 0;
        _lastWeightUpdateScore = 0;
        _currentCrossWeightTimer = 0;

        AreAllCrossWeightsSet = false;
    }

    public static CanvasItem SpawnRandomCrossIn(CanvasItem parentNode)
    {
        Cross cross = GetLocalRandomCross();

        var scene = cross.GetInstance(out bool isReusing);

        if (!isReusing)
            parentNode.AddChild(scene);

        Vector2 position = new Vector2();

        Rect2 spawnRect = new Rect2();

        if (cross.IsSelfPositioningOnSpawn) return scene;

        // Positioning
        if (cross.SpawnRectMode == Cross.SpawnRectModeEnum.Viewport)
        {
            var CanvasTransfrom = parentNode.GetCanvasTransform();

            spawnRect = new Rect2(-CanvasTransfrom.Origin / CanvasTransfrom.Scale, parentNode.GetViewportRect().Size / CanvasTransfrom.Scale);
        }
        else if (cross.SpawnRectMode == Cross.SpawnRectModeEnum.CameraLimits)
        {
            spawnRect = CameraLimits;
        }
        spawnRect = new Rect2(spawnRect.Position + cross.SpawnRectCorrection.Position, spawnRect.Size + cross.SpawnRectCorrection.Size);

        if (cross.AllowedSpawnSidesDic["FullRect"])
        {
            // The less the player moves, the more often the crosses will spawn near the center of the screen
            float CrossGathering = _random.Next(100) < (1 - PlayerMoveCoeff) * 50 ? 3 - PlayerMoveCoeff * 2 : 1;
            spawnRect = GeometryTools.ResizeRectWithAbsoluteAnchor(spawnRect, spawnRect.Size / CrossGathering, G.Player.GlobalPosition);

            position = RandomVectorInAlt(spawnRect, G.Player.GlobalPosition, cross.MinimumSpawnDistanceToPlayer);
        }
        else
        {
            switch (cross.GetRandomAllowedSide())
            {
                case "Top":
                    position.X = RandomIn(spawnRect.Position.X, spawnRect.End.X);
                    position.Y = spawnRect.Position.Y;
                    break;
                case "Right":
                    position.X = spawnRect.End.X;
                    position.Y = RandomIn(spawnRect.Position.Y, spawnRect.End.Y);
                    break;
                case "Bottom":
                    position.X = RandomIn(spawnRect.Position.X, spawnRect.End.X);
                    position.Y = spawnRect.End.Y;
                    break;
                case "Left":
                    position.X = spawnRect.Position.X;
                    position.Y = RandomIn(spawnRect.Position.Y, spawnRect.End.Y);
                    break;
                case "Middle":
                    position = spawnRect.GetCenter();
                    break;
                case "Topleft":
                    position.X = spawnRect.Position.X;
                    position.Y = spawnRect.Position.Y;
                    break;
                case "TopRight":
                    position.X = spawnRect.End.X;
                    position.Y = spawnRect.Position.Y;
                    break;
                case "BottomLeft":
                    position.X = spawnRect.Position.X;
                    position.Y = spawnRect.End.Y;
                    break;
                case "BottomRight":
                    position.X = spawnRect.End.X;
                    position.Y = spawnRect.End.Y;
                    break;
            }
        }

        scene.Set("global_position", position);

        scene.OnPositionSetted();

        return scene;
    }
}
