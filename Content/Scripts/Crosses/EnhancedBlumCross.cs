using Godot;
using Godot.Collections;
using System;

public partial class EnhancedBlumCross : CrossNode
{
    // Nodes
    public AudioStreamPlayer ExplosionSignal;

    // Explosion
    private const float TIME_TO_CONTROLLER_EXPLOSION = 5f;
    private float _timerToControllerExplosion = TIME_TO_CONTROLLER_EXPLOSION, _timerToExplosion = 1;
    private bool _isWearAccelerated = false;

    // Nodes moving
    private const byte CONTROLLERS_COUNT = 4;
    public byte ControllersLeft { get; private set; } = CONTROLLERS_COUNT;
    private string _controlledCrossesGroupIndex;


    private Random _random = new Random();

    public override void _Ready()
	{
        // Initializing nodes
        NodesInit();
        ExplosionSignal = GetNode<AudioStreamPlayer>("ExplosionSignal");

        AddToGroup("Crosses");

        // Spawn properties
        Modulate = new Color(Modulate.R, Modulate.G, Modulate.B, 0);

        _controlledCrossesGroupIndex = "ControlledCrosses_" + Name;

        // Binding the signals
        var wearAcceleratingArea = GetNode<Area2D>("WearAcceleratingArea2D");
        wearAcceleratingArea.AreaEntered += (a) => _isWearAccelerated = true;
        wearAcceleratingArea.AreaExited += (a) => _isWearAccelerated = false;
        ExplosionSound.Finished += OnFinished;

        for (int i = 0; i < ControllersLeft; i++)
            GetNode<Control>("Controllers/Crystal" + (4 - i) + "/EnergyBeam").Modulate = new Color(1, 1, 1, 1f / ControllersLeft);
    }

    private Array<Node> _allTheCrossesOnScreen, _allTheControlledCrosses;
	public override void _PhysicsProcess(double delta)
	{
        if (Modulate.A < 1)
            Modulate = new Color(Modulate.R, Modulate.G, Modulate.B, Modulate.A + 0.01f);
        else if (ControllersLeft > 0)
        {
            _timerToControllerExplosion -= 0.016667f * (_isWearAccelerated ? 8 : 1);
            WarningSprite.Modulate = new Color(WarningSprite.Modulate.R, WarningSprite.Modulate.G, WarningSprite.Modulate.B, _timerToControllerExplosion / 5);
            GetNode<Control>("Controllers/Crystal" + (5 - ControllersLeft) + "/Sprite").Size = new Vector2(170, _timerToControllerExplosion * 44);
            if (_timerToControllerExplosion < 0)
            {
                CrystalExplode();
            }

            _allTheCrossesOnScreen = GetTree().GetNodesInGroup("Crosses");
            if (GetTree().GetNodesInGroup(_controlledCrossesGroupIndex).Count < ControllersLeft && _allTheCrossesOnScreen.Count > 0)
            {
                var addableCross = (Node2D)_allTheCrossesOnScreen.PickRandom();

                if (addableCross.SceneFilePath != "res://Content/Scenes/Crosses/EnhancedCross5.tscn")
                    if (addableCross.SceneFilePath != "res://Content/Scenes/Crosses/EnhancedCross4.tscn" || _random.Next(25) == 0)
                        addableCross.AddToGroup(_controlledCrossesGroupIndex);
            }
                

            _allTheControlledCrosses = GetTree().GetNodesInGroup(_controlledCrossesGroupIndex);

            for (int i = 0; i < ControllersLeft; i++)
            {
                var energyBeam = GetNode<Control>("Controllers/Crystal" + (4 - i) + "/EnergyBeam");

                if (_allTheControlledCrosses.Count > i)
                {
                    if (!energyBeam.Visible)
                        energyBeam.Visible = true;
                    
                    var controlledCross = (Node2D)_allTheControlledCrosses[i];
                    if (controlledCross.Name != "Ball" || controlledCross.Visible)
                        controlledCross.GlobalTranslate(controlledCross.GlobalPosition.DirectionTo(G.Player.GlobalPosition) * 5 / ControllersLeft);
                    energyBeam.Size = new Vector2(energyBeam.GlobalPosition.DistanceTo(controlledCross.GlobalPosition), energyBeam.Size.Y);

                    energyBeam.Rotation = new Vector2(energyBeam.GlobalPosition.X, energyBeam.GlobalPosition.Y + energyBeam.PivotOffset.Y).AngleToPoint(controlledCross.GlobalPosition) - energyBeam.GetParent<Node2D>().Rotation;
                }
                else
                    energyBeam.Visible = false;
            }

        }
        else if (_timerToExplosion > 0)
        {
            _timerToExplosion -= 0.016667f;
            WarningSprite.Modulate = new Color(WarningSprite.Modulate.R, WarningSprite.Modulate.G, WarningSprite.Modulate.B, (float)_random.NextDouble());
        }
        else
        {
            Explode();
        }
    }

    public void CrystalExplode()
    {
        // Breaking sound
        GetNode<AudioStreamPlayer>("CrystalBreaking").Play();

        // Breaking visual effects
        int controllerId = CONTROLLERS_COUNT + 1 - ControllersLeft;
        var controller = GetNode<Node2D>("Controllers/Crystal" + controllerId);
        controller.Hide();
        controller.ProcessMode = ProcessModeEnum.Disabled;
        GetNode<CpuParticles2D>("Controllers/CrystallParticles" + controllerId).Emitting = true;

        //
        _allTheControlledCrosses = GetTree().GetNodesInGroup(_controlledCrossesGroupIndex);
        if (_allTheControlledCrosses.Count == ControllersLeft)
            _allTheControlledCrosses[ControllersLeft - 1].RemoveFromGroup(_controlledCrossesGroupIndex);

        ControllersLeft--;

        if (ControllersLeft == 0)
            GetNode<AudioStreamPlayer>("ExplosionSignal").Play();

        for (int i = 0; i < ControllersLeft; i++)
            GetNode<Control>("Controllers/Crystal" + (4 - i) + "/EnergyBeam").Modulate = new Color(1, 1, 1, 1f / ControllersLeft);

        _timerToControllerExplosion = TIME_TO_CONTROLLER_EXPLOSION;
    }

    public override void Respawn()
    {
        base.Respawn();

        AddToGroup("Crosses");

        Modulate = new Color(Modulate.R, Modulate.G, Modulate.B, 0);

        // Returning the old settings
        CrossSprite.Visible = true;
        WarningSprite.Visible = true;

        ExplosionAnimation.Visible = false;

        _timerToControllerExplosion = TIME_TO_CONTROLLER_EXPLOSION;
        _timerToExplosion = 1;
        _isWearAccelerated = false;
        ControllersLeft = CONTROLLERS_COUNT;

        ExplosionSound.Stop();
        // Returning crystals to the initial state
        for (int i = 0; i < CONTROLLERS_COUNT; i ++)
        {
            int controllerId = CONTROLLERS_COUNT - i;
            var controller = GetNode<Node2D>("Controllers/Crystal" + controllerId);
            controller.Show();
            controller.ProcessMode = ProcessModeEnum.Inherit;
            controller.GetNode<Control>("Sprite").Size = new Vector2(170, 220); // 170X220 is the default size of the control
        }
    }
}
