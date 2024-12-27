using Godot;
using Godot.Collections;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using static ModManager;

[Tool]
public partial class WorkshopMenu : DraggableWindow
{
    private Dictionary[] _modsInfo;
    private int _selectedMod = -1;
    private string _selectedModFolder;
    private string[] _directories;
    private TextureButton _selectedModButton;
    private Control _lastFocusOwner, _currentModPreview;

    bool _createAModWindowOpened = false;


    #region InScript methods
    #region Override methods
    public override void _Ready()
    {
        GetTree().Root.FilesDropped += FileDropped;
        TreeExiting += () => GetTree().Root.FilesDropped -= FileDropped;
        _directories = GetModDirectories();
        _modsInfo = new Dictionary[_directories.Length];
        _lastFocusOwner = GetViewport().GuiGetFocusOwner();



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
    }
    #endregion

    #region Private methods
    private void ShowModInfo(int index)
    {
        GetNode<RichTextLabel>("MarginContainer/VBoxContainer/Tabs/Mods/MarginC/VBoxC/HBoxC/ModDescription/MarginC/ScrollContainer/VBoxC/Description").Text = _modsInfo[index]["description"].ToString();
        if (_currentModPreview != null)
            _currentModPreview.QueueFree();
        _currentModPreview = (Control)ResourceLoader.Load<PackedScene>(_directories[index] + "/ModPreview.tscn").Instantiate();

        var modDescriptionVBox = GetNode("MarginContainer/VBoxContainer/Tabs/Mods/MarginC/VBoxC/HBoxC/ModDescription/MarginC/ScrollContainer/VBoxC");
        modDescriptionVBox.AddChild(_currentModPreview);
        modDescriptionVBox.MoveChild(_currentModPreview, 0); // Moving preview to the top
    }
    private void SelectMod(int index, TextureButton button, ModType modType)
    {
        if (_selectedModButton != null)
            _selectedModButton.GetNode<ColorRect>("SelectRect").Visible = false; // Unsellect the last mod

        _selectedModButton = button;
        _selectedMod = index;
        button.GetNode<ColorRect>("SelectRect").Visible = true; // Selecting new mod


        switch (modType)
        {
            case ModType.map:
                SetVisibleActionButtons("PlayMapButton", "OpenInEditorButton");
                break;
            case ModType.localization:
                break;
            case ModType.skin:
                break;
            case ModType.resource:
                break;
            case ModType.content:
                break;
        }
    }
    private void SelectMod(int index, TextureButton button, string modTypeName)
    {
        ModType NewModType = StringToModType(modTypeName);
        SelectMod(index, button, NewModType);
    }

    /// <summary>
    /// All buttons with sent names will be visible and the rest will be hidden. 
    ///<para>There are buttons that are always visible, regardless of the values sent, among them: "MakeAModButton" and "OpenTheDirectoryButton".</para>
    /// </summary>
    private void SetVisibleActionButtons(params string[] visibleNames)
    {
        string[] AlwaysVisibleButtons = ["MakeAModButton", "OpenTheDirectoryButton"];
        var actionButtonsContainer = GetNode<HBoxContainer>("MarginContainer/VBoxContainer/Tabs/Mods/MarginC/VBoxC/ActionButtons");

        foreach (ButtonWithText button in actionButtonsContainer.GetChildren())
        {
            string buttonName = button.Name;
            bool doArrayContainsName = AlwaysVisibleButtons.Contains(buttonName) || visibleNames.Contains(buttonName);
            button.Visible = doArrayContainsName;
        }
    }

    private void WhenFocusChanged(Control node)
    {
        if (node != null && node.GetParentOrNull<Node>() != null && node.GetParent().Name == "ModsList")
        {
             ShowModInfo(_selectedMod);
        }
    }

    public TextureButton AddModButton(string path, int modIndex)
    {
        Dictionary ModTypeIcons = new Dictionary
        {
            {"map", ResourceLoader.Load<Texture2D>("res://Content/Sprites/Interface/MapMod.png")},
            {"localization", ResourceLoader.Load<Texture2D>("res://Content/Sprites/Interface/LocalizationMod.png")},
            {"skin", ResourceLoader.Load<Texture2D>("res://Content/Sprites/Interface/SkinMod.png")},
            {"content", ResourceLoader.Load<Texture2D>("res://Content/Sprites/Interface/ContentMod.png")},
            {"resource", ResourceLoader.Load<Texture2D>("res://Content/Sprites/Interface/ResourceMod.png")},
        };
        var ModButton = (TextureButton)ResourceLoader.Load<PackedScene>("res://Content/Scenes/Interface&Menu/ModButton.tscn").Instantiate();

        var model = FileSystemExtension.GetJsonModel(path + @"\mod_info.json");

        if (modIndex < _modsInfo.Length)
            _modsInfo[modIndex] = model;
        else
            _modsInfo = _modsInfo.Append(model).ToArray();



        ModButton.GetNode<RichTextLabel>("HBoxContainer/Name").Text = model["name"].ToString(); // Setting mod name


        string modTypeName = model["mod_type"].ToString();
        ModType modType = (ModType)Enum.Parse(typeof(ModType), modTypeName);

        if (ModTypeIcons.TryGetValue(modTypeName, out Variant value))
            ModButton.GetNode<TextureRect>("HBoxContainer/ModType").Texture = (Texture2D)value; // Setting mod type icon

        var checkButton = ModButton.GetNode<CheckButton>("HBoxContainer/CheckButton");

        string modFolderName = Path.GetFileName(path);

        if (modTypeName == "map" || modFolderName == "StandartTimerLib")
            checkButton.QueueFree();
        else
        {
            if (!ModDataManager.ModStatuses.ContainsKey(modFolderName))
                ModDataManager.ModStatuses.Add(modFolderName, true);

            bool IsModToggled = ModDataManager.ModStatuses[modFolderName].AsBool();

            checkButton.ButtonPressed = IsModToggled;
            checkButton.Toggled += (bool v) => ToggleMod(v, modFolderName);
        }


        int id = modIndex;
        ModButton.FocusEntered += () => ShowModInfo(id);
        ModButton.Pressed += () =>
        {
            SelectMod(modIndex, ModButton, modType);
            _selectedModFolder = _directories[modIndex];
        };
        GetNode("MarginContainer/VBoxContainer/Tabs/Mods/MarginC/VBoxC/HBoxC/ModsList").AddChild(ModButton);
        return ModButton;
    }
    private void ToggleMod(bool value, string folderName)
    {
        ModDataManager.ModStatuses[folderName] = value;
        ModDataManager.SaveModStatuses();
    }
    #endregion

    #endregion



    #region Buttons region
    public void OpenInEditor()
    {
        if (G.TypeOfUsedController != "Keyboard" && GetNode<Control>("WARNING").Visible == false)
        {
            GetNode<AnimationPlayer>("WARNING/AnimationPlayer").Play("WARNING");
            return;
        }

        G.InGameTransitiveValue = _selectedModFolder + @"\MainScene.tscn";
        G.ModMapPath = _selectedModFolder.Remove(0, DefaultModsPath.Length) + @"\MainScene.tscn";
        G.ModMapFolder = _selectedModFolder;

        CurrentModMapFolderName = Path.GetDirectoryName(_selectedModFolder);
        G.IsLevelVanilla = false;

        GetTree().ChangeSceneToFile("res://Content/Scenes/Interface&Menu/LevelEditor/LevelEditor.tscn");
    }

    public void PlayMap()
    {
        CurrentModMapFolderName = Path.GetDirectoryName(_selectedModFolder);
        G.IsLevelVanilla = false;
    }

    public void OpenModFolder()
    {
        // Determine the folder path based on the selected mod
        string folderPath = _selectedMod == -1 ?
            DefaultModsPath.Replace("/", @"\") :
            _directories[_selectedMod].Replace("/", @"\");

        if (_selectedMod == -1)
            Process.Start("explorer.exe", folderPath); // Open the default mods path directly
        else 
            Process.Start("explorer.exe", $"/select,\"{folderPath}\""); // Open the parent folder and select the specific mod folder
    }


    public void CreateANewMod()
    {
        if (!_createAModWindowOpened)
        {
            var createAModMenu = GD.Load<PackedScene>("res://Content/Scenes/Interface&Menu/CreateAModWindow.tscn").Instantiate() as CreateAModWindow;
            GetParent().AddChild(createAModMenu);

            createAModMenu.CreateMod += ConfirmCreatingMod;

            _createAModWindowOpened = true;
            createAModMenu.TreeExited += () => _createAModWindowOpened = false;
        }

    }
    public void ConfirmCreatingMod(CreateModParams @params)
    {
        var modPath = CreateAMod(@params); // Creating a mod and getting its path

        var button = AddModButton(modPath, _directories.Length); // Creating a button for the mode
        GetNode("MarginContainer/VBoxContainer/Tabs/Mods/MarginC/VBoxC/HBoxC/ModsList").MoveChild(button, 0); // Moving the mod button to the top

        _directories = _directories.Append(modPath).ToArray(); // Adding the path to the array
    }
    #endregion



    #region Mod Info Editing Section
    string _settedModName, _settedModFolderName, _settedModDescription, _settedImagePath;
    public void EditModInfo()
    {
        var editModInfo = GetNode<Control>("EditModInfo");
        editModInfo.Visible = true;
        GetNode<AnimationPlayer>("EditModInfo/AnimationPlayer").Play("Appearance");
        GetNode<TextureButton>("ActionButtons/EditInfoButton").Disabled = true;

        GetNode<TextEdit>("EditModInfo/ScrollContainer/VBoxContainer/CrutchControl/ModNameText").Text = _settedModName = _modsInfo[_selectedMod]["name"].ToString();
        GetNode<LineEdit>("EditModInfo/ScrollContainer/VBoxContainer/CrutchControl/LineEdit").Text = _settedModFolderName = _selectedModFolder.Remove(0, DefaultModsPath.Length);
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
        if (_selectedModFolder != DefaultModsPath + _settedModFolderName)
        {
            Directory.Move(_selectedModFolder, DefaultModsPath + _settedModFolderName);
            _selectedModFolder = DefaultModsPath + _settedModFolderName;
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
    #endregion
}
