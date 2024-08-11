using Godot;

public partial class ModPreview : Control
{
	[Export] public string _resourcePath = "res://Content/Sprites/Interface/LevelIconButton.png";

    public override void _Ready()
	{
		var previewPicture = GetNodeOrNull<TextureRect>("PreviewPicture");
		if (previewPicture != null && FileAccess.FileExists(_resourcePath))
		{
			if (_resourcePath.Substr(0, 6) != "res://")
			{
                Image image = new Image();
                image.Load(_resourcePath);
                ImageTexture texture = new ImageTexture();
                texture.SetImage(image);

                previewPicture.Texture = texture;
            }
			else
			{
                previewPicture.Texture = (Texture2D)GD.Load(_resourcePath);
            }
        }
	}
}
