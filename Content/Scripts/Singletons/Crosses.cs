using Godot;
using System;
using System.Collections.Generic;
using static G;
using static OtherExtension.RandomTools;
using System.Linq;

public partial class Crosses : Node
{
    static Random _random = new Random();
    public class Cross
    {
        public PackedScene PackedScene;

        public float AppearStartTime;

        public float Weight;

        public enum SpawnRectModeEnum { Viewport, CameraLimits }
        public SpawnRectModeEnum SpawnRectMode = SpawnRectModeEnum.Viewport;

        public float MinimumDistanceToPlayer = 0;

        public Rect2 SpawnRectCorrection = new Rect2(-80, -80, 80, 80);

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

        public string[] GetOnlyAllowedSpawnSides()
        {
            return AllowedSpawnSidesDic
                .Where(pair => pair.Value == true)
                .Select(pair => pair.Key)
                .ToArray();
        }

        public string GetRandomAllowedSide()
        {
            string[] allowedSides = GetOnlyAllowedSpawnSides();
            return allowedSides[_random.Next(allowedSides.Length)];
        }


        public Cross(PackedScene packedScene, float appearStartTime = 0f, float weight = 1f, float minimumDistanceToPlayer = 0)
        {
            SetPackedScene(packedScene);

            AppearStartTime = appearStartTime;
            Weight = weight;
            MinimumDistanceToPlayer = minimumDistanceToPlayer;
        }
        /// <summary>
        /// vanillaName can be Cross1, EnhancedCross3, GoldenCross etc.
        /// </summary>
        public Cross(string vanillaName, float appearStartTime = 0f, float weight = 1f, float minimumDistanceToPlayer = 0)
        {
            SetPackedScene(vanillaName);

            AppearStartTime = appearStartTime;
            Weight = weight;
            MinimumDistanceToPlayer = minimumDistanceToPlayer;
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


        public CanvasItem GetInstance()
        {
            return (CanvasItem)PackedScene.Instantiate();
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
        }
    }

    public class CrossSpawner
    {
        private string[] _scenesPathes;
        public string[] ScenesPathes
        {
            get { return _scenesPathes; }

            set
            {
                _scenesPathes = value;
                _scenes = new PackedScene[ScenesPathes.Length];
                for (int i = 0; i < _scenes.Length; i++)
                {
                    _scenes[i] = GD.Load<PackedScene>(ScenesPathes[i]);
                }
            }
        }
        private PackedScene[] _scenes = { };



        public Node GetCross(int index)
        {
            return _scenes[index].Instantiate();
        }

        private int _lastAviableCrossNumber = 0;
        private float _lastCheckedScoresValue = 0;

        private bool _didAllCrossWeigthsSetted;
        public Node GetRandomCross()
        {
            if (!_didAllCrossWeigthsSetted)
            {
                if (_crossesWeight[_lastAviableCrossNumber] + Scores - _lastCheckedScoresValue < _defaultCrossesWeight[_lastAviableCrossNumber])
                {
                    _crossesWeight[_lastAviableCrossNumber] += Scores - _lastCheckedScoresValue;
                    _lastCheckedScoresValue = Scores;
                }
                else
                {
                    _crossesWeight[_lastAviableCrossNumber] = _defaultCrossesWeight[_lastAviableCrossNumber];
                    _lastAviableCrossNumber++;
                    _lastCheckedScoresValue = 0;
                }
                if (_lastAviableCrossNumber >= _scenes.Length)
                {
                    _lastAviableCrossNumber = _scenes.Length - 1;
                    _didAllCrossWeigthsSetted = true;
                }
            }

            int SelectedCrossNumber;
            float RandomNumber = _random.NextSingle() * _crossesWeight.Sum();
            for (int i = 0; ; i++)
            {
                if (RandomNumber < _defaultCrossesWeight[i])
                {
                    SelectedCrossNumber = i;
                    break;
                }
                else RandomNumber -= _defaultCrossesWeight[i];
            }
            return GetCross(SelectedCrossNumber);
        }

        public void RecalculateCrossWeight()
        {
            float WeightMultiplierExtender = Scores / 30 * CrossesProgressCoeff * _defaultCrossesWeight[_lastAviableCrossNumber];
            _crossesWeight = new float[CrossesInGameTotal];
            _lastAviableCrossNumber = 0;
            for (int i = 0; WeightMultiplierExtender > 0; i++)
            {
                if (i >= 5)
                {
                    _didAllCrossWeigthsSetted = true;
                    _lastAviableCrossNumber = 4;
                    break;
                }
                if (_crossesWeight[_lastAviableCrossNumber] + WeightMultiplierExtender * _defaultCrossesWeight[_lastAviableCrossNumber] < _defaultCrossesWeight[_lastAviableCrossNumber])
                {
                    _crossesWeight[_lastAviableCrossNumber] += WeightMultiplierExtender * _defaultCrossesWeight[_lastAviableCrossNumber];
                    WeightMultiplierExtender = 0;
                }
                else
                {
                    _crossesWeight[_lastAviableCrossNumber] = _defaultCrossesWeight[_lastAviableCrossNumber];
                    _lastAviableCrossNumber++;
                    WeightMultiplierExtender -= 1;
                }
            }
        }

        public static CanvasItem SpawnRandomCrossIn(CanvasItem parentNode)
        {
            Cross cross = CurrentCrossesPack[_random.Next(CurrentCrossesPack.Length)];

            var scene = cross.GetInstance();

            parentNode.AddChild(scene);

            Vector2 position = new Vector2();

            Rect2 spawnRect = new Rect2();

            if (cross.SpawnRectMode == Cross.SpawnRectModeEnum.Viewport)
            {
                var CanvasTransfrom = parentNode.GetCanvasTransform();

                spawnRect = new Rect2(-CanvasTransfrom.Origin, parentNode.GetViewportRect().Size / CanvasTransfrom.Scale);
            }
            else if (cross.SpawnRectMode == Cross.SpawnRectModeEnum.CameraLimits)
            {
                spawnRect = CameraLimits;
            }
            spawnRect = new Rect2(spawnRect.Position + cross.SpawnRectCorrection.Position, spawnRect.Size + cross.SpawnRectCorrection.Size);

            if (cross.AllowedSpawnSidesDic["FullRect"])
            {
                position = RandomVectorAt(spawnRect.GetCenter(), spawnRect.Size, cross.MinimumDistanceToPlayer);
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

            return scene;
        }

    }


    public static string CurrentPackName;
    public static Cross[] CurrentCrossesPack;

    public static Cross CurrentGoldenCross;
    public static bool EnableGoldenCrossSpawning;
    public static int GoldenCrossSpawnRarity = 100;

    public int CurrentCrossesCount => CurrentCrossesPack.Length + (CurrentGoldenCross != null ? 1 : 0);

    private static float[] _defaultCrossesWeight, _crossesTimings;
    private static float[] _crossesWeight;

    public static void SetDefaultCrossesPack()
    {
        if (CurrentPackName == "LightweightTNT1.0") return;

        Cross[] crosses = {
            new Cross("Cross1", 0, 600),
            new Cross("Cross2", 30, 170),
            new Cross("Cross3", 60, 80),
            new Cross("Cross4", 90, 40, 100),
            new Cross("Cross5", 120, 110),
        };
        Cross goldenCross = new Cross("GoldenCross");
        goldenCross.SpawnRectMode = Cross.SpawnRectModeEnum.CameraLimits;
        goldenCross.MinimumDistanceToPlayer = 200;

        SetCurrentCrossesPack("LightweightTNT1.0", crosses, goldenCross);
    }

    public static void SetEnhancedCrossesPack()
    {
        if (CurrentPackName == "LightweightTNT2.0") return;

        Cross[] crosses = {
            new Cross("EnhancedCross1", 0, 650),
            new Cross("EnhancedCross2", 30, 265),
            new Cross("EnhancedCross3", 60, 45),
            new Cross("EnhancedCross4", 90, 15),
            new Cross("EnhancedCross5", 120, 25),
        };
        Cross goldenCross = new Cross("EnhancedGoldenCross");
        goldenCross.SpawnRectMode = Cross.SpawnRectModeEnum.CameraLimits;
        goldenCross.MinimumDistanceToPlayer = 200;

        // Helicopter cross setting up
        crosses[4].SetSpawnSide(false, false, false, false, true, false); // Helicopter must spawn at the top camera limit corner
        crosses[4].SpawnRectCorrection = new Rect2(-640, -1280, 640, 0);

        SetCurrentCrossesPack("LightweightTNT2.0", crosses, goldenCross);
    }

    /// <summary>
    /// If you want the golden crosses to not spawn, leave null there
    /// </summary>
    public static void SetCurrentCrossesPack(string packName, Cross[] crosses, Cross goldenCross = null, int goldenCrossSpawnRarity = 100)
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
    }
}
