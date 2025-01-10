using Godot;
using Godot.Collections;
using System.IO;
using System.Linq;
using System.Text.Json;

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
        return value.IndexOfAny(Path.GetInvalidFileNameChars()) != -1;
    }

    public static string MakeUniqueFilePath(string path)
    {
        if (!Directory.Exists(path))
            return path;

        string newPath = null;
        for (int i = 2; ; i++)
        {
            newPath = path + i;

            if (!Directory.Exists(newPath))
                return newPath;
        }
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
    public static void SaveInJson(Variant what, string where)
    {
        SaveInJson(Json.Stringify(what), where);
    }
}