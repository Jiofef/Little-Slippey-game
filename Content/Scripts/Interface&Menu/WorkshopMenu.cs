using Godot;
using Godot.Collections;
using System;
using System.IO;

public partial class WorkshopMenu : Control
{
    private readonly string _defaultPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + @"\Godot\app_userdata\Little Slippey\mods\";
    private Dictionary[] _modsInfo;
    private int _selectedMod = -1;
    private string _selectedModFolder;
    private TextureButton _selectedModButton;
    private Control _lastFocusOwner;
    public override void _Ready()
    {
        string[] Directories = Directory.GetDirectories(_defaultPath);
        _modsInfo = new Dictionary[Directories.Length];
        _lastFocusOwner = GetViewport().GuiGetFocusOwner();
        _selectedModButton = GetNode<TextureButton>("ModsScrollContainer/ModsScrollVBoxContainer/DefaultMod");

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
                
                int crutch = i; //For some fucking reason, if you put i in the signal, then it will send the PRESENT value of i from the cycle (for example 3 if there is 2 mods). From a cycle that has long passed at the time of sending the signal. I'm in a awe.
                ModButton.FocusEntered += () => ShowModInfo(crutch);
                ModButton.Pressed += () =>
                {
                    SelectMod(crutch, ModButton);
                    _selectedModFolder = Directories[crutch];
                };
                GetNode("ModsScrollContainer/ModsScrollVBoxContainer").AddChild(ModButton);
            }
        }
        var defaultModButton = GetNode<TextureButton>("ModsScrollContainer/ModsScrollVBoxContainer/DefaultMod");
        defaultModButton.FocusEntered += () => ShowDefaultModInfo();
        defaultModButton.Pressed += () => SelectMod(-1, defaultModButton);
    }
    public override void _PhysicsProcess(double delta)
    {
        if (GetViewport().GuiGetFocusOwner() != _lastFocusOwner)
        {
            _lastFocusOwner = GetViewport().GuiGetFocusOwner();
            WhenFocusChanged(_lastFocusOwner);
        }
    }
    private void ShowDefaultModInfo()
    {
        GetNode<RichTextLabel>("ModDescription/Description").Text = "Core. DON'T DISABLE IT.";
    }
    private void ShowModInfo(int index)
    {
        GetNode<RichTextLabel>("ModDescription/Description").Text = _modsInfo[index]["description"].ToString();
    }
    private void SelectMod(int index, TextureButton button)
    {
        if (_selectedModButton != null)
            _selectedModButton.GetNode<ColorRect>("SelectRect").Visible = false;
        _selectedModButton = button;
        _selectedMod = index;
        button.GetNode<ColorRect>("SelectRect").Visible = true;
    }
    private void WhenFocusChanged(Control node)
    {
        if (node.GetParentOrNull<Node>() != null && node.GetParent().Name != "ModsScrollVBoxContainer")
        {
            if (_selectedMod == -1)
                ShowDefaultModInfo();
            else
                ShowModInfo(_selectedMod);
        }
    }

    public void OpenInEditor()
    {
        G.InGameTransitiveValue = _selectedModFolder + @"\MainScene.tscn";
        GetTree().ChangeSceneToFile("res://Content/Scenes/Interface&Menu/LevelEditor.tscn");
    }
}
