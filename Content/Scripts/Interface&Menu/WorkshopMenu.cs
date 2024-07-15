using Godot;
using Godot.Collections;
using System;
using System.IO;

public partial class WorkshopMenu : Control
{
    private readonly string _defaultPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + @"\Godot\app_userdata\Little Slippey\mods\";
    public override void _Ready()
    {
        string[] Directories = Directory.GetDirectories(_defaultPath);
        for (int i = 0; i < Directories.Length; i++)
        {
            if (File.Exists(Directories[i] + @"\mod_info.json"))
            {
                using Godot.FileAccess ModInfo = Godot.FileAccess.Open(Directories[i] + @"\mod_info.json", Godot.FileAccess.ModeFlags.Read);
                var text = ModInfo.GetAsText();
                var model = Json.ParseString(ModInfo.GetAsText()).Obj as Dictionary;
                GD.Print((string)model["name"]);
            }
        }
    }
}
