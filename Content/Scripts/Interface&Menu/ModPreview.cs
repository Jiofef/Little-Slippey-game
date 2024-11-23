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
                previewPicture.Texture = FileSystemExtension.LoadNonResourceImage(_resourcePath);
			else
                previewPicture.Texture = (Texture2D)GD.Load(_resourcePath);
        }
	}
}
