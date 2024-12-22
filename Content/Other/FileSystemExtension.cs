using Godot;
using Godot.Collections;
using System.IO;
using System.Linq;

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

    public static bool HasSpecialChars(string value)
    {
        return value.Any(ch => !char.IsLetterOrDigit(ch));
    }

    public static Dictionary GetJsonModel(string path)
    {
        using Godot.FileAccess file = Godot.FileAccess.Open(path, Godot.FileAccess.ModeFlags.Read);
        var model = Json.ParseString(file.GetAsText()).Obj as Dictionary;
        file.Close();
        return model;
    }

    public static void SaveInJson(string what, string where)
    {
        using Godot.FileAccess file = Godot.FileAccess.Open(where, Godot.FileAccess.ModeFlags.Write);
        file.StoreString(what);
        file.Close();
    }
}