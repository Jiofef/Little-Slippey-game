using Godot;

public partial class OptionsMenu : Control
{
    public override void _Ready()
    {
        Meta.OptionsReserve = Meta.Instance.Clone();
    }
}
