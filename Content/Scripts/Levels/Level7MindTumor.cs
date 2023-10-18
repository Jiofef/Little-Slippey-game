using Godot;

public partial class Level7MindTumor : Area2D
{
    CharacterBody2D _player;
    Node2D _tumorSprite;
    private Vector2 _defaultPosition, _defaultGlobalPosition, _cameraDiagonal = new Vector2(1024, 576) / (Meta.Instance.CameraZoom / 1.25f);
    private int _limitTop, _limitRight, _limitBottom, _limitLeft;
    public override void _Ready()
    {
        GetNode<Sprite2D>("CanvasLayer/TumorSprite/Sprite2D").Scale = new Vector2(0.04f * Meta.Instance.CameraZoom, 0.04f * Meta.Instance.CameraZoom);
        GD.Print(GetNode<Sprite2D>("CanvasLayer/TumorSprite/Sprite2D").Scale);
        _player = GetNode<CharacterBody2D>("..");
        _tumorSprite = GetNode<Node2D>("CanvasLayer/TumorSprite");

        _defaultPosition = Position;
        _defaultGlobalPosition = GlobalPosition;

        _limitLeft = (int)(Position.X + _cameraDiagonal.X / 2 - 25);
        _limitRight = (int)(G.LevelXYSizes[G.CurrentLevel].X + Position.X - _cameraDiagonal.X / 2 + 25);
        _limitTop = (int)(Position.Y + _cameraDiagonal.Y / 2 - 25);
        _limitBottom = (int)(G.LevelXYSizes[G.CurrentLevel].Y + Position.Y - _cameraDiagonal.Y / 2 + 100);

        _tumorSprite.Position = Position / (Meta.Instance.CameraZoom - (Meta.Instance.CameraZoom - 1.25f) * 2) + new Vector2(430, 230);
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
        _tumorSprite.Scale = new Vector2(Scale.X + 0.005f, Scale.Y + 0.005f);
    }

    public void n2evf7yUH3ZLT3x3N0___()
    {
        UnchangableMeta.SaveRecords();
        UnchangableMeta.SaveToFile();
        GetNode("../../../..").QueueFree();
    }
}
