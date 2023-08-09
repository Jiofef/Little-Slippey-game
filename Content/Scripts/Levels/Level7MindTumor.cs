using Godot;

public partial class Level7MindTumor : Area2D
{
    CharacterBody2D _player;
    Node2D _tumorSprites;
    private Vector2 _defaultPosition, _defaultGlobalPosition, _cameraDiagonal = new Vector2(850, 480);
    private int _limitTop, _limitRight, _limitBottom, _limitLeft;
    public override void _Ready()
    {
        _player = GetNode<CharacterBody2D>("..");
        _tumorSprites = GetNode<Node2D>("CanvasLayer/TumorSprites");

        _defaultPosition = Position;
        _defaultGlobalPosition = GlobalPosition;

        _limitLeft = (int)(Position.X + _cameraDiagonal.X / 2 - 25);
        _limitRight = (int)(G.LevelXYSizes[G.CurrentLevel].X + Position.X - _cameraDiagonal.X / 2 + 25);
        _limitTop = (int)(Position.Y + _cameraDiagonal.Y / 2 - 25);
        _limitBottom = (int)(G.LevelXYSizes[G.CurrentLevel].Y + Position.Y - _cameraDiagonal.Y / 2 + 100);

        GetNode<Node2D>("CanvasLayer/TumorSprites").Position = Position + new Vector2(425, 240);
    }

    public override void _PhysicsProcess(double delta)
    {
        {
            if (_player.Position.X < _cameraDiagonal.X / 2 - 25)
                GlobalPosition = new Vector2(_limitLeft, GlobalPosition.Y);
            else if (_player.Position.X > (G.LevelXYSizes[G.CurrentLevel].X - _cameraDiagonal.X / 2 + 25))
                GlobalPosition = new Vector2(_limitRight, GlobalPosition.Y);
            else
                Position = new Vector2(_defaultPosition.X, Position.Y);

            if (_player.Position.Y < _cameraDiagonal.Y / 2 - 25)
                GlobalPosition = new Vector2(GlobalPosition.X, _limitTop);
            else if (_player.Position.Y > (G.LevelXYSizes[G.CurrentLevel].Y - _cameraDiagonal.Y / 2 + 100))
                GlobalPosition = new Vector2(GlobalPosition.X, _limitBottom);
            else
                Position = new Vector2(Position.X, _defaultPosition.Y);
        }

        Scale = new Vector2(Scale.X + 0.005f, Scale.Y + 0.005f);
        _tumorSprites.Scale = new Vector2(Scale.X + 0.005f, Scale.Y + 0.005f);
    }

    public void n2evf7yUH3ZLT3x3N0___()
    {
        UnchangableMeta.SaveRecords();
        UnchangableMeta.SaveToFile();
        GetNode("../../../..").QueueFree();
    }
}
