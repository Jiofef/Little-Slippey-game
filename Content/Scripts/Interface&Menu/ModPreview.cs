using Godot;

public partial class ModPreview : Control
{
	[Export] public string _resourcePath = "res://Content/Sprites/Interface/LevelIconButton.png";

    public override void _Ready()
	{
		var previewPicture = GetNodeOrNull<TextureRect>("PreviewPicture");
		if (previewPicture != null)
		{
			Image image = new Image();
            image.Load(_resourcePath);
            previewPicture.Texture = GD.Load(_resourcePath) as ImageTexture;
        }
	}
}
