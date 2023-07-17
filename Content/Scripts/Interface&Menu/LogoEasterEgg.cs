using Godot;

public partial class LogoEasterEgg : TextureButton
{
    private float _xMotion = 0, _yMotion = 0, _gravity = 9.8f;
    AnimatedSprite2D _slippey;
    public override void _Ready()
    {
        SetPhysicsProcess(false);
    }
    public void GoingFall()
    {
        SetPhysicsProcess(true);
        _xMotion = 3;
        _yMotion = -5;
        _slippey = GetNode<AnimatedSprite2D>("Slippey");
        _slippey.Animation = "Death";
    }
    public override void _PhysicsProcess(double delta)
    {
        if (_slippey.Position.Y > 1000) QueueFree();
        _slippey.Position = new Vector2(_slippey.Position.X + _xMotion, _slippey.Position.Y + _yMotion);
        _slippey.Rotation += _xMotion / 50;
        _yMotion += _gravity / 100;
    }
}
