using Godot;
using Godot.Collections;
using System;
using System.Diagnostics;
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
    private Control _lastFocusOwner, _currentModPreview;

    public override void _Ready()
    {
        GetTree().Root.FilesDropped += FileDropped;
        TreeExiting += () => GetTree().Root.FilesDropped -= FileDropped;
        _directories = Directory.GetDirectories(_defaultPath);
        _modsInfo = new Dictionary[_directories.Length];
        _lastFocusOwner = GetViewport().GuiGetFocusOwner();
        _selectedModButton = GetNode<TextureButton>("ModsScrollContainer/ModsScrollVBoxContainer/DefaultMod");


        for (int i = 0; i < _directories.Length; i++)
        {
            GD.Print(_directories[i]);
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

        _currentModPreview = GetNode<Control>("ModDescription/ModPreview");
    }
    public override void _PhysicsProcess(double delta)
    {
        if (GetViewport().GuiGetFocusOwner() != _lastFocusOwner)
        {
            _lastFocusOwner = GetViewport().GuiGetFocusOwner();
            WhenFocusChanged(_lastFocusOwner);
        }
        if (Input.IsActionJustPressed("Cancel") && GetNode<Control>("EditModInfo").Visible)
            GetNode<TextureButton>("EditModInfo/DeclineButton").GrabFocus();
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
        if (modIndex < _modsInfo.Length)
            _modsInfo[modIndex] = model;
        else
            _modsInfo = _modsInfo.Append(model).ToArray();

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
        if (_currentModPreview != null)
            _currentModPreview.QueueFree();
        _currentModPreview = (Control)ResourceLoader.Load<PackedScene>(_directories[index] + "/ModPreview.tscn").Instantiate();
        GetNode("ModDescription").AddChild(_currentModPreview);
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
        if (node != null && node.GetParentOrNull<Node>() != null && node.GetParent().Name != "ModsScrollVBoxContainer")
        {
            if (_selectedMod == -1)
                ShowDefaultModInfo();
            else
                ShowModInfo(_selectedMod);
        }
    }

    public void OpenInEditor()
    {
        if (G.TypeOfUsedController != "Keyboard" && GetNode<Control>("WARNING").Visible == false)
        {
            GetNode<AnimationPlayer>("WARNING/AnimationPlayer").Play("WARNING");
            return;
        }

        G.InGameTransitiveValue = _selectedModFolder + @"\MainScene.tscn";
        G.ModMapPath = _selectedModFolder.Remove(0, _defaultPath.Length) + @"\MainScene.tscn";
        G.ModMapFolder = _selectedModFolder;
        GetTree().ChangeSceneToFile("res://Content/Scenes/Interface&Menu/LevelEditor.tscn");
    }

    public void OpenModFolder()
    {
        Process.Start("explorer.exe", OS.GetUserDataDir().Replace("/", @"\") + @"\mods");
    }

    public void CreateANewMap()
    {
        for (int i = 0; ; i++)
        {
            var SuggestedPath = _defaultPath + "CustomMap" + i;
            if (!Directory.Exists(SuggestedPath))
            {
                _directories = _directories.Append(SuggestedPath).ToArray();
                Directory.CreateDirectory(SuggestedPath);
                DirAccess.CopyAbsolute("res://Content/Scenes/Other/UserLevelLayout.tscn", SuggestedPath + @"\MainScene.tscn"); 
                DirAccess.CopyAbsolute("res://Content/Sprites/Interface/CustomMapDefaultPreview.png", SuggestedPath + @"\PreviewPicture.png");
                foreach (string value in new string[]{ ""})
                {

                }
                Directory.CreateDirectory(SuggestedPath + @"\Other");
                Directory.CreateDirectory(SuggestedPath + @"\Scenes");
                Directory.CreateDirectory(SuggestedPath + @"\Scripts");
                Directory.CreateDirectory(SuggestedPath + @"\Scounds");
                Directory.CreateDirectory(SuggestedPath + @"\Sprites");

                var modPreview = (ModPreview)ResourceLoader.Load<PackedScene>("res://Content/Scenes/Interface&Menu/ModPreviewLayout.tscn").Instantiate();
                modPreview._resourcePath = SuggestedPath + @"\PreviewPicture.png";
                var ToSave = new PackedScene();
                ToSave.Pack(modPreview);
                ResourceSaver.Save(ToSave, SuggestedPath + @"\ModPreview.tscn");

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
    string _settedModName, _settedModFolderName, _settedModDescription, _settedImagePath;
    public void EditModInfo()
    {
        var editModInfo = GetNode<Control>("EditModInfo");
        editModInfo.Visible = true;
        GetNode<AnimationPlayer>("EditModInfo/AnimationPlayer").Play("Appearance");
        GetNode<TextureButton>("EditModInfo/DeclineButton").GrabFocus();
        GetNode<TextureButton>("ActionButtons/EditInfoButton").Disabled = true;

        GetNode<TextEdit>("EditModInfo/ScrollContainer/VBoxContainer/CrutchControl/ModNameText").Text = _settedModName = _modsInfo[_selectedMod]["name"].ToString();
        GetNode<LineEdit>("EditModInfo/ScrollContainer/VBoxContainer/CrutchControl/LineEdit").Text = _settedModFolderName = _selectedModFolder.Remove(0, _defaultPath.Length);
        GetNode<TextEdit>("EditModInfo/ScrollContainer/VBoxContainer/CrutchControl/DescriptionText").Text = _settedModDescription = _modsInfo[_selectedMod]["description"].ToString();

        var previewPicture = _currentModPreview.GetNodeOrNull<TextureRect>("PreviewPicture");
        if (previewPicture != null)
            GetNode<TextureRect>("EditModInfo/PreviewPicture").Texture = previewPicture.Texture;
    }

    public void ChangeThePicture()
    {
        GetNode<FileDialog>("EditModInfo/ChangeThePictureDialog").Popup();
    }
    public void SetPicture(string path)
    {
        if (!path.EndsWith(".png"))
            return;

        _settedImagePath = path;
        Image image = new Image();
        image.Load(path);
        ImageTexture texture = new ImageTexture();
        texture.SetImage(image);
        
        GetNode<TextureRect>("EditModInfo/PreviewPicture").Texture = texture;
    }
    public void FileDropped(string[] files)
    {
        var previewPictureRect = GetNode<TextureRect>("EditModInfo/PreviewPicture").GetRect();
        var MouseGlobalPos = GetGlobalMousePosition();
        if (GetNode<TextureRect>("EditModInfo/PreviewPicture").Visible && MouseGlobalPos.X > previewPictureRect.Position.X && MouseGlobalPos.Y > previewPictureRect.Position.Y && MouseGlobalPos.X < previewPictureRect.Position.X + previewPictureRect.Size.X && MouseGlobalPos.Y < previewPictureRect.Position.Y + previewPictureRect.Size.Y)
            SetPicture(files[0]);
    }

    public void Cancel()
    {
        GetNode<Control>("EditModInfo").Visible = false;
        var editInfoButton = GetNode<TextureButton>("ActionButtons/EditInfoButton");
        editInfoButton.GrabFocus();
        editInfoButton.Disabled = false;
    }
    public void Accept()
    {
        GetNode<Control>("EditModInfo").Visible = false;
        var editInfoButton = GetNode<TextureButton>("ActionButtons/EditInfoButton");
        editInfoButton.GrabFocus();
        editInfoButton.Disabled = false;

        _modsInfo[_selectedMod]["name"] = _settedModName;
        _modsInfo[_selectedMod]["description"] = _settedModDescription;
        if (_selectedModFolder != _defaultPath + _settedModFolderName)
        {
            Directory.Move(_selectedModFolder, _defaultPath + _settedModFolderName);
            _selectedModFolder = _defaultPath + _settedModFolderName;
            _directories[_selectedMod] = _selectedModFolder;
            ((ModPreview)_currentModPreview)._resourcePath = _selectedModFolder;
        }

        using Godot.FileAccess file = Godot.FileAccess.Open(_selectedModFolder + @"\mod_info.json", Godot.FileAccess.ModeFlags.Write);
        file.StoreString(_modsInfo[_selectedMod].ToString());
        file.Close();

        _selectedModButton.GetNode<RichTextLabel>("Name").Text = _settedModName;

        if (_settedImagePath != null)
            DirAccess.CopyAbsolute(_settedImagePath, _selectedModFolder + @"\PreviewPicture.png");
        _settedImagePath = null;
        ShowModInfo(_selectedMod);
    }
    public void ModNameChanged()
    {
        _settedModName = GetNode<TextEdit>("EditModInfo/ScrollContainer/VBoxContainer/CrutchControl/ModNameText").Text;
    }
    public void ModFolderNameChanged(string value)
    {
        _settedModFolderName = value;
    }
    public void ModDescriptionChanged()
    {
        _settedModDescription = GetNode<TextEdit>("EditModInfo/ScrollContainer/VBoxContainer/CrutchControl/DescriptionText").Text;
    }
}
