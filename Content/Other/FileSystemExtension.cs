using Godot;
using System.IO;

public partial class FileSystemExtension : Node
{
	public static void CopyResourceFileTo(string sourcePath, string targetPath)
	{
        ResourceSaver.Save(ResourceLoader.Load(sourcePath), targetPath);
    }

    public static void CopyByteResourceFileTo(string sourcePath, string targetPath)
    {
        if (!Godot.FileAccess.FileExists(sourcePath))
            return;

        var sourceFile = Godot.FileAccess.Open(sourcePath, Godot.FileAccess.ModeFlags.Read);
        if (sourceFile == null)
            return;

        byte[] data = sourceFile.GetBuffer((long)sourceFile.GetLength());
        sourceFile.Close();


        var targetFile = Godot.FileAccess.Open(targetPath, Godot.FileAccess.ModeFlags.Write);
        if (targetFile == null)
            return;

        targetFile.StoreBuffer(data);
        targetFile.Close();
    }

    public static ImageTexture LoadNonResourceImage(string resourcePath)
    {
        Image image = new Image();
        image.Load(resourcePath);
        ImageTexture texture = new ImageTexture();
        texture.SetImage(image);
        return texture;
    }
}