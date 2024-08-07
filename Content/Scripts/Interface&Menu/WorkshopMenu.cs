using Godot;
using Godot.Collections;
using System.IO;
using System.Linq;

public partial class WorkshopMenu : Control
{
    private readonly string _defaultPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + @"\Godot\app_userdata\Little Slippey\mods\";
    private Dictionary[] _modsInfo;
    private int _selectedMod = -1;
    private string _selectedModFolder;
    private string[] _directories;
    private TextureButton _selectedModButton;
    private Control _lastFocusOwner;
    public override void _Ready()
    {
        _directories = Directory.GetDirectories(_defaultPath);
        _modsInfo = new Dictionary[_directories.Length];
        _lastFocusOwner = GetViewport().GuiGetFocusOwner();
        _selectedModButton = GetNode<TextureButton>("ModsScrollContainer/ModsScrollVBoxContainer/DefaultMod");


        for (int i = 0; i < _directories.Length; i++)
        {
            try
            {
                if (File.Exists(_directories[i] + @"\mod_info.json"))
                {
                    AddModButton(_directories[i], i);
                }
            }
            catch { }
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
    public TextureButton AddModButton(string path, int modIndex)
    {
        Dictionary ModTypeIcons = new Dictionary
        {
            {"localization", ResourceLoader.Load<Texture2D>("res://Content/Sprites/Interface/LocalizationMod.png")},
            {"map", ResourceLoader.Load<Texture2D>("res://Content/Sprites/Interface/MapMod.png")}
        };
        var ModButton = (TextureButton)ResourceLoader.Load<PackedScene>("res://Content/Scenes/Interface&Menu/ModButton.tscn").Instantiate();
        using Godot.FileAccess ModInfo = Godot.FileAccess.Open(path + @"\mod_info.json", Godot.FileAccess.ModeFlags.Read);
        var text = ModInfo.GetAsText();
        var model = Json.ParseString(ModInfo.GetAsText()).Obj as Dictionary;
        _modsInfo[modIndex] = model;
        ModButton.GetNode<RichTextLabel>("Name").Text = model["name"].ToString();
        if (ModTypeIcons.TryGetValue(model["mod_type"], out Variant value))
            ModButton.GetNode<Sprite2D>("ModType").Texture = (Texture2D)value;

        ModButton.FocusEntered += () => ShowModInfo(modIndex);
        ModButton.Pressed += () =>
        {
            SelectMod(modIndex, ModButton);
            _selectedModFolder = _directories[modIndex];
        };
        GetNode("ModsScrollContainer/ModsScrollVBoxContainer").AddChild(ModButton);
        return ModButton;
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

    public void CreateANewMap()
    {
        for (int i = 0; ; i++)
        {
            var SuggestedPath = _defaultPath + "CustomMap" + i;
            if (!Directory.Exists(SuggestedPath))
            {
                _directories.Append(SuggestedPath);
                Directory.CreateDirectory(SuggestedPath);
                DirAccess.CopyAbsolute("res://Content/Scenes/Other/UserLevelLayout.tscn", SuggestedPath + @"\MainScene.tscn");
                using Godot.FileAccess file = Godot.FileAccess.Open(SuggestedPath + @"\mod_info.json", Godot.FileAccess.ModeFlags.Write);
                file.StoreString("{\r\n  \"name\": \"CustomMap\",\r\n  \"mod_type\":  \"map\",\r\n  \"description\": \"\"\r\n}");
                file.Close();

                var button = AddModButton(SuggestedPath, _directories.Length - 1);
                GetNode("ModsScrollContainer/ModsScrollVBoxContainer").MoveChild(button, 1);
                break;
            }
        }

    }

    //Mod Info Editing Section

    public void EditModInfo()
    {
        var editModInfo = GetNode<Control>("EditModInfo");
        editModInfo.Visible = true;
        GetNode<AnimationPlayer>("EditModInfo/AnimationPlayer").Play("Appearance");
        GetNode("ActionButtons").ProcessMode = ProcessModeEnum.Disabled;
        GetNode("ModsScrollContainer").ProcessMode = ProcessModeEnum.Disabled;
    }
    public void Cancel()
    {
        GetNode("ActionButtons").ProcessMode = ProcessModeEnum.Inherit;
        GetNode("ModsScrollContainer").ProcessMode = ProcessModeEnum.Inherit;
        GetNode<Control>("EditModInfo").Visible = false;
    }
    public void Accept()
    {
        GetNode("ActionButtons").ProcessMode = ProcessModeEnum.Inherit;
        GetNode("ModsScrollContainer").ProcessMode = ProcessModeEnum.Inherit;
        GetNode<Control>("EditModInfo").Visible = false;
    }
}
