using Godot;
using System;
using System.Linq;

public partial class CrossSpawner : Node2D
{
    Random _random = new Random();

    public G.CrossSpawner Spawner = new G.CrossSpawner();

    PackedScene[] _crosses = new PackedScene[G.CrossesInGameTotal];


    private int[] _crossDefaultWeight = { 600, 170, 80, 40, 110 };
    private float[] _crossWeight = new float[G.CrossesInGameTotal];
    private int _lastAviableCrossNumber = 0;
    private float _weightMultiplierExtenderToCurrentCross = 0;
    private bool _doAllCrossWeigthsSetted, _isCrossesEnhanced;
    private readonly float _floatDelta = 0.016667f;

    public override void _Ready()
	{

	}


	public override void _PhysicsProcess(double delta)
	{
        if (!G.IsProgressPaused)
        {
            G.Scores += _floatDelta;
            _weightMultiplierExtenderToCurrentCross += (_floatDelta * _crossDefaultWeight[_lastAviableCrossNumber]) / 30 * G.CrossesProgressCoeff;
            if (G.Scores > G.LevelCompleteTime && UnchangableMeta.LevelCompleteStatus[G.CurrentLevel - 1] < 1 + Meta.Instance.Gameplay.Dificulty && G.IsLevelVanilla)
                UnchangableMeta.SaveRecords();
        }


        if (G.IsCrossesEnabled)
        {
            int RandomRange = 20 - Meta.Instance.Gameplay.Dificulty * 5;
            RandomRange = (int)((RandomRange - (RandomRange / 2 - G.PlayerMoveCoeff * RandomRange / 2)) / G.CrossSpawnMultiplier);

            if (_random.Next(RandomRange) == 0)
            {
                Node Cross = Spawner.GetRandomCross();
                float CrossGathering = _random.Next(100) < (1 - G.PlayerMoveCoeff) * 50 ? 3 - G.PlayerMoveCoeff * 2 : 1;
                if (Cross is Node2D or Control)
                {
                    Cross.Set("position", new Vector2(G.Player.Position.X + (-750 + _random.Next(1500)) / CrossGathering, G.Player.Position.Y + (-450 + _random.Next(900)) / CrossGathering));

                    switch (Cross.Name)
                    {
                        case "DefaultCross" or "EnhancedDefaultCross":
                            Cross.Set("modulate", new Color(1, 1, 1, 0));
                            Cross.Set("scale", new Vector2(3, 3));
                            break;

                        case "RestlessCross" or "EnhancedRestlessCross":
                            Cross.Set("modulate", new Color(1, 1, 1, 0));
                            Cross.Set("scale", new Vector2(3, 3));
                            float XPos = _random.Next(
                                G.Player.Position.X - 425 > G.CameraLimits.W ? (int)G.Player.Position.X - 425 : 0,
                                G.Player.Position.X + 425 < G.CameraLimits.Y ? (int)G.Player.Position.X + 425 : (int)G.Player.Position.X + 425
                                );
                            float YPos = _random.Next(
                                G.Player.Position.Y - 240 > G.CameraLimits.X ? (int)G.Player.Position.Y - 240 : 0,
                                G.Player.Position.Y + 240 < G.CameraLimits.Z ? (int)G.Player.Position.Y + 240 : (int)G.Player.Position.Y + 240
                                );
                            Cross.Set("position", new Vector2(XPos, YPos));
                            break;

                        case "ElementalCross":
                            Cross.Set("scale", new Vector2(3, 3));
                            break;

                        case "BlumCross" or "EnhancedBlumCross":
                            Cross.Set("modulate", new Color(1, 1, 1, 0));
                            if (((Vector2)Cross.Get("position") - G.Player.Position).X < 300)
                                Cross.Set("position", new Vector2(
                                    ((Vector2)Cross.Get("position")).X,
                                    _random.Next(100) < 50 ?
                                    _random.Next((int)G.Player.Position.Y - 750, (int)G.Player.Position.Y - 250) :
                                    _random.Next((int)G.Player.Position.Y + 250, (int)G.Player.Position.Y + 750)
                                    ));
                            break;

                        case "CannonCross":
                            if (G.CurrentLevel == 8)
                            {
                                Cross.Set("rotation_degrees", _random.Next(100) <= 50 ? 90 : -90);
                                Cross.Set("position", new Vector2(((Vector2)Cross.Get("position")).X + 1280, (float)Cross.Get("rotation_degrees") == 90 ? -25 : 760));
                                break;
                            }

                            Cross.Set("scale", new Vector2(_random.Next(100) <= 50 ? 1 : -1, 1));
                            Cross.Set("position", ((Vector2)Cross.Get("scale")).X == -1 ? new Vector2(G.LevelXYSizes[G.CurrentLevel].X + 25, ((Vector2)Cross.Get("position")).Y) : new Vector2(-25, ((Vector2)Cross.Get("position")).Y));
                            break;

                        case "EnhancedCannonCross":
                            Cross.Set("position", new Vector2(_random.Next(2) == 0 ? G.Player.Position.X - 1280 : G.Player.Position.X + 1280, G.Player.Position.Y + _random.Next(-750, -250)));
                            break;
                    }
                }

                AddChild(Cross);

                if (G.CurrentLevel == 9 || Meta.Instance.Gameplay.AdditionStatuses[2] || _isCrossesEnhanced)
                    Cross.AddToGroup("Crosses");
            }
        }
    }
}
