using Godot;
using Godot.Collections;
using System;
using System.IO;

public partial class WorkshopMenu : Control
{
    private readonly string _defaultPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + @"\Godot\app_userdata\Little Slippey\mods\";
    private Dictionary[] _modsInfo;
    private int _selectedMod;
    public override void _Ready()
    {
        GetViewport().GuiFocusChanged += GuiFocusChanged => WhenFocusChanged(GuiFocusChanged);
        string[] Directories = Directory.GetDirectories(_defaultPath);
        _modsInfo = new Dictionary[Directories.Length];

        Dictionary ModTypeIcons = new Dictionary
        {
            {"localization", ResourceLoader.Load<Texture2D>("res://Content/Sprites/Interface/LocalizationMod.png")},
            {"map", ResourceLoader.Load<Texture2D>("res://Content/Sprites/Interface/MapMod.png")}
        };
        for (int i = 0; i < Directories.Length; i++)
        {
            if (File.Exists(Directories[i] + @"\mod_info.json"))
            {
                var ModButton = (TextureButton)ResourceLoader.Load<PackedScene>("res://Content/Scenes/Interface&Menu/ModButton.tscn").Instantiate();
                using Godot.FileAccess ModInfo = Godot.FileAccess.Open(Directories[i] + @"\mod_info.json", Godot.FileAccess.ModeFlags.Read);
                var text = ModInfo.GetAsText();
                var model = Json.ParseString(ModInfo.GetAsText()).Obj as Dictionary;
                _modsInfo[i] = model;
                ModButton.GetNode<RichTextLabel>("Name").Text = model["name"].ToString();
                if (ModTypeIcons.TryGetValue(model["mod_type"], out Variant value))
                    ModButton.GetNode<Sprite2D>("ModType").Texture = (Texture2D)value;
                
                int crutch = i; //For some fucking reason, if you put i in the function, then it will send the PRESENT value of i from the cycle (for example 3 if there is 2 mods). From a cycle that has long passed at the time of sending the signal. I'm in a awe.
                ModButton.FocusEntered += () => ShowModInfo(crutch);
                ModButton.FocusExited += () => SelectMod(crutch);
                ModButton.Pressed += () => SelectMod(crutch);
                GetNode("ModsScrollContainer/ModsScrollVBoxContainer").AddChild(ModButton);
            }
        }
    }

    public void ShowModInfo(int index)
    {
        GetNode<RichTextLabel>("ModDescription/Description").Text = _modsInfo[index]["description"].ToString();
    }
    public void SelectMod(int index)
    {
        _selectedMod = index;
        GetNode<ColorRect>("ModsScrollContainer/ModsScrollVBoxContainer/Button" + (_selectedMod + 2) + "/SelectRect").Visible = true;
    }
    public void WhenFocusExited(Control node)
    {
        if (node.GetParentOrNull<Node>() != null && node.GetParent().Name == "ModsScrollVBoxContainer")
        {
            node.GetNode<ColorRect>("SelectRect").Visible = false;
        }
    }
    private void WhenFocusChanged(Control node)
    {
        if (node.GetParentOrNull<Node>() != null && node.GetParent().Name != "ModsScrollVBoxContainer")
        {
            ShowModInfo(_selectedMod);
        }
    }
}
