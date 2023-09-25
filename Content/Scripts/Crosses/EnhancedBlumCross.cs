using Godot;
using System;
using System.Linq;

public partial class EnhancedBlumCross : Node2D
{
    private float _xSpriteMotion, _ySpriteMotion = -3, _gravity = 9.8f, _timerToControllerExplosion = 5, _timerToExplosion = 1;
    private byte _controllersLeft = 4;
    private string _controlledCrossesGroupIndex;
    private bool _isWearAccelerated = false;
    private Sprite2D _crossSprite, _warningSprite;
    private AudioStreamPlayer _explosiveSignal;
    private CharacterBody2D _player;
    private Random _random = new Random();

    private Node2D[] ControlledCrosses = new Node2D[4];

    public override void _Ready()
	{
        _crossSprite = GetNode<Sprite2D>("CrossSprite");
        _warningSprite = GetNode<Sprite2D>("WarningSprite");
        _explosiveSignal = GetNode<AudioStreamPlayer>("ExplosionSignal");
        _player = GetNode<CharacterBody2D>("../Player");

        _explosiveSignal.Play();

        _controlledCrossesGroupIndex = "ControlledCrosses_" + Name;

        for (int i = 0; i < _controllersLeft; i++)
            GetNode<Node2D>("EnergyPoints/Point" + (4 - i) + "/EnergyBeam").Modulate = new Color(1, 1, 1, 1f / _controllersLeft);
    }

	public override void _PhysicsProcess(double delta)
	{
        if (Modulate.A < 1)
            Modulate = new Color(Modulate.R, Modulate.G, Modulate.B, Modulate.A + 0.01f);
        else if (_controllersLeft > 0)
        {
            _timerToControllerExplosion -= 0.016667f * (_isWearAccelerated ? 8 : 1);
            _warningSprite.Modulate = new Color(_warningSprite.Modulate.R, _warningSprite.Modulate.G, _warningSprite.Modulate.B, _timerToControllerExplosion / 5);
            GetNode<Control>("EnergyPoints/Point" + (5 - _controllersLeft) + "/Crystal").Size = new Vector2(170, _timerToControllerExplosion * 44);
            if (_timerToControllerExplosion < 0)
            {
                GetNode<Node2D>("EnergyPoints/Point" + (5 - _controllersLeft)).QueueFree();
                GetNode<CpuParticles2D>("EnergyPoints/CrystallParticles" + (5 - _controllersLeft)).Emitting = true;
                var controlledCrosses = GetTree().GetNodesInGroup(_controlledCrossesGroupIndex);
                if (controlledCrosses.Count == _controllersLeft)
                    controlledCrosses[_controllersLeft - 1].RemoveFromGroup(_controlledCrossesGroupIndex);
                _controllersLeft--;

                for (int i = 0; i < _controllersLeft; i++)
                    GetNode<Node2D>("EnergyPoints/Point" + (4 - i) + "/EnergyBeam").Modulate = new Color(1, 1, 1, 1f / _controllersLeft);

                _timerToControllerExplosion = 5;
            }

            var AllCrossesOnScreen = GetTree().GetNodesInGroup("Crosses");
            if (GetTree().GetNodesInGroup(_controlledCrossesGroupIndex).Count < _controllersLeft && AllCrossesOnScreen.Count > 0)
                AllCrossesOnScreen.PickRandom().AddToGroup(_controlledCrossesGroupIndex);

            var AllControlledCrosses = GetTree().GetNodesInGroup(_controlledCrossesGroupIndex);

            for (int i = 0; i < _controllersLeft; i++)
            {
                var showingRangeController = GetNode<Control>("EnergyPoints/Point" + (4 - i) + "/EnergyBeam/ShowingRangeController");
                var energyBeam = GetNode<Node2D>("EnergyPoints/Point" + (4 - i) + "/EnergyBeam");

                if (AllControlledCrosses.Count > i)
                {
                    if (!energyBeam.Visible)
                        energyBeam.Visible = true;
                    
                    var ControlledCross = (Node2D)AllControlledCrosses[i];
                    ControlledCross.Translate(ControlledCross.Position.DirectionTo(_player.Position) * 5 / _controllersLeft);
                    showingRangeController.Size = new Vector2(showingRangeController.GlobalPosition.DistanceTo(ControlledCross.GlobalPosition), showingRangeController.Size.Y);

                    energyBeam.Rotation += energyBeam.GetAngleTo(ControlledCross.GlobalPosition);           
                }
                else
                {
                    energyBeam.Visible = false;
                }
            }

        }
        else if (_timerToExplosion > 0)
        {
            _timerToExplosion -= 0.016667f;
            _warningSprite.Modulate = new Color(_warningSprite.Modulate.R, _warningSprite.Modulate.G, _warningSprite.Modulate.B, (float)_random.NextDouble());
        }
        else
        {
            var explosionAnimation = GetNode<AnimatedSprite2D>("ExplosionAnimation");
            var explosiveArea = GetNode<CollisionShape2D>("ExplosiveArea/CollisionShape2D");
            if (explosionAnimation.IsPlaying())
            {
                explosiveArea.Disabled = true;
                SetPhysicsProcess(false);
                return;
            }
            _crossSprite.QueueFree();
            _warningSprite.QueueFree();
            GetNode<AudioStreamPlayer>("ExplosionSound").Play();
            explosionAnimation.Visible = true;
            explosionAnimation.Play();
            explosiveArea.Disabled = false;

            var Groups = GetGroups();
            for (int i = 0; i < Groups.Count; i++)
                RemoveFromGroup(Groups[i]);
        }
    }

    public void EnableWearAccelerating()
    {
        _isWearAccelerated = true;
    }
    public void DisableWearAccelerating()
    {
        _isWearAccelerated = false;
    }
}
