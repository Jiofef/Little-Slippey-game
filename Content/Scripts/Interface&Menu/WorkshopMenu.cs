using Godot;
using Godot.Collections;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using static ModManager;
using static ModDataManager;

[Tool]
public partial class WorkshopMenu : DraggableWindow
{
    private System.Collections.Generic.Dictionary<string, object>[] _modsInfo;
    private int _selectedMod = -1;
    private string _selectedModFolder;
    private string[] _directories;
    private TextureButton _selectedModButton;
    private Control _lastFocusOwner, _currentModPreview;
    private TabContainer _tabs;

    bool _createAModWindowOpened = false;


    #region InScript methods
    #region Loading methods
    public override void _Ready()
    {
        //Initializing the nodes
        _tabs = GetNode<TabContainer>("MarginContainer/VBoxContainer/Tabs");

        GetTree().Root.FilesDropped += FileDropped; // File reset handling for quick replacement of mod previews
        TreeExiting += () => GetTree().Root.FilesDropped -= FileDropped; // Event disconnecting on node deletion
        _lastFocusOwner = GetViewport().GuiGetFocusOwner(); // I don't remember why it's used.

        UpdateModsList();
    }

    public void UpdateModsList()
    {
        // Deleting current mod buttons
        var modsList = GetNode("MarginContainer/VBoxContainer/Tabs/Mods/MarginC/VBoxC/HBoxC/ModsList");
        foreach (var modButton in modsList.GetChildren())
            modButton.QueueFree();
        // Unselecting the selected mod
        _selectedMod = -1;
        _selectedModFolder = null;
        _selectedModButton = null;
        // Since the selected mod is removed
        SetTabEnabled("Mod Options", false);
        SetTabEnabled("Edit Mod Data", false);

        // Loading mod info
        _directories = GetModDirectories();
        _modsInfo = new System.Collections.Generic.Dictionary<string, object>[_directories.Length];

        // Creating all the mod buttons
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
    public void UpdateModOptions()
    {
        if (_selectedMod == -1) return; // If the mod isn't selected
    }
    public void UpdateEditModData()
    {
        if (_selectedMod == -1) return; // If the mod isn't selected


        const string FIELDS_LINK = "MarginContainer/VBoxContainer/Tabs/Edit Mod Data/MarginC/HBoxC/VBoxC/MarginC/ScrollC/VBoxC/";
        const string EDIT_MOD_DATA_LINK = "MarginContainer/VBoxContainer/Tabs/Edit Mod Data/";

        // Loading fields state
        GetNode<LineEdit>(FIELDS_LINK + "NameEdit").Text = _settedModName = _modsInfo[_selectedMod]["name"].ToString();
        GetNode<LineEdit>(FIELDS_LINK + "FolderNameEdit").Text = _settedModFolderName = Path.GetFileName(_selectedModFolder);
        GetNode<TextEdit>(FIELDS_LINK + "DescriptionText").Text = _settedModDescription = _modsInfo[_selectedMod]["description"].ToString();

        _settedModOptionDatas = GetCloneOfModOptionData(_modsInfo[_selectedMod]["mod_options"]); // NEED FIX HERE

        // Loading the preview
        var previewPicture = _currentModPreview.GetNodeOrNull<TextureRect>("PreviewPicture");
        if (previewPicture != null)
            GetNode<TextureRect>(EDIT_MOD_DATA_LINK + "MarginC/HBoxC/VBoxC2/NPR/PreviewTexture").Texture = previewPicture.Texture;
    }
    #endregion

    #region Private methods
    private void SetTabEnabled(string tabName, bool value)
    {
        var tab = _tabs.GetNode<Control>(tabName);
        int tabId = _tabs.GetTabIdxFromControl(tab);
        _tabs.SetTabDisabled(tabId, !value);
    }
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

        // Enabling tabs in case a mod has not been previously selected
        SetTabEnabled("Mod Options", true);
        SetTabEnabled("Edit Mod Data", true);

        switch (modType)
        {
            case ModType.map:
                SetVisibleActionButtons("PlayMapButton", "OpenInEditorButton");
                break;
            case ModType.localization:
                SetVisibleActionButtons();
                break;
            case ModType.skin:
                SetVisibleActionButtons();
                break;
            case ModType.resource:
                SetVisibleActionButtons();
                break;
            case ModType.content:
                SetVisibleActionButtons();
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

        // Creating a button
        var ModButton = (TextureButton)ResourceLoader.Load<PackedScene>("res://Content/Scenes/Interface&Menu/ModButton.tscn").Instantiate(); 

        var model = FileSystemExtension.GetSystemJsonModel(path + @"\mod_info.json");

        if (modIndex < _modsInfo.Length)
            _modsInfo[modIndex] = model;
        else
            _modsInfo = _modsInfo.Append(model).ToArray();


        // Setting mod name
        ModButton.GetNode<RichTextLabel>("HBoxContainer/Name").Text = model["name"].ToString();


        string modTypeName = model["mod_type"].ToString();
        ModType modType = StringToModType(modTypeName);

        // Setting mod type icon
        if (ModTypeIcons.TryGetValue(modTypeName, out Variant value))
            ModButton.GetNode<TextureRect>("HBoxContainer/ModType").Texture = (Texture2D)value;

        var checkButton = ModButton.GetNode<CheckButton>("HBoxContainer/CheckButton");

        string modFolderName = Path.GetFileName(path);

        if (modTypeName == "map" || modFolderName == "StandartTimerLib") // Removing a switch where it is not needed
            checkButton.QueueFree();
        else // Loading mod status
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

    public void OnTabChanged(int index)
    {
        #region Handling when tab are changed with the expectation that the tab order may change in the future.
        var tabs = GetNode<TabContainer>("MarginContainer/VBoxContainer/Tabs");

        var tab = tabs.GetCurrentTabControl(); // Getting selected tab

        switch (tab.Name)
        {
            case "Mods":
                break;
            case "Mod Options":
                UpdateModOptions();
                break;
            case "Edit Mod Data":
                UpdateEditModData();
                break;
        }
        #endregion
    }

    #region Buttons region
    public void OpenInEditor(bool openAnyway = false)
    {
        if (!openAnyway && G.TypeOfUsedController != "Keyboard")
        {
            var gamepadModWarning = (ConfirmationWindow)GD.Load<PackedScene>("res://Content/Scenes/Interface&Menu/RareScenes/GamepadModWarning.tscn").Instantiate();
            GetParent().AddChild(gamepadModWarning);
            gamepadModWarning.Accepted += () => OpenInEditor(true);
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
    private string _settedModName, _settedModFolderName, _settedModDescription, _settedImagePath;
    private ModOptionData[] _settedModOptionDatas;
    private bool _isFolderNameCorrect = true, _wereChangesMade = false;
    public void ChangeThePicture()
    {
        GetNode<FileDialog>("EditModInfo/ChangeThePictureDialog").Popup();
    }
    public void SetPicture(string path)
    {
        if (!path.EndsWith(".png"))
            return;

        _wereChangesMade = true;
        UpdateSaveChangesButton();

        _settedImagePath = path;
        Image image = new Image();
        image.Load(path);
        ImageTexture texture = new ImageTexture();
        texture.SetImage(image);
        
        GetNode<TextureRect>("MarginContainer/VBoxContainer/Tabs/Edit Mod Data/MarginC/HBoxC/VBoxC2/NPR/PreviewTexture").Texture = texture;
    }
    public void FileDropped(string[] files)
    {
        var previewPicture = GetNode<TextureRect>("MarginContainer/VBoxContainer/Tabs/Edit Mod Data/MarginC/HBoxC/VBoxC2/NPR/PreviewTexture");
        var previewPictureRect = previewPicture.GetGlobalRect();
        var MouseGlobalPos = GetGlobalMousePosition();

        if (previewPicture.Visible && MouseGlobalPos.X > previewPictureRect.Position.X && MouseGlobalPos.Y > previewPictureRect.Position.Y && MouseGlobalPos.X < previewPictureRect.Position.X + previewPictureRect.Size.X && MouseGlobalPos.Y < previewPictureRect.Position.Y + previewPictureRect.Size.Y)
            SetPicture(files[0]);
    }

    public void SaveTheChanges()
    {
        // Disabling the SaveChanges button
        _wereChangesMade = false;
        UpdateSaveChangesButton();

        // Saving the values in mod info
        _modsInfo[_selectedMod]["name"] = _settedModName;
        _modsInfo[_selectedMod]["description"] = _settedModDescription;
        if (_selectedModFolder != DefaultModsPath + _settedModFolderName)
        {
            Directory.Move(_selectedModFolder, DefaultModsPath + _settedModFolderName);
            _selectedModFolder = DefaultModsPath + _settedModFolderName;
            _directories[_selectedMod] = _selectedModFolder;
            ((ModPreview)_currentModPreview)._resourcePath = _selectedModFolder;
        }
        _modsInfo[_selectedMod]["mod_options"] = _settedModOptionDatas;

        // Saving the mod info
        string modsInfoLocation = _selectedModFolder + @"\mod_info.json";
        FileSystemExtension.SaveInJson(_modsInfo[_selectedMod], modsInfoLocation);

        _selectedModButton.GetNode<RichTextLabel>("HBoxContainer/Name").Text = _settedModName;

        // Updating the preview
        if (_settedImagePath != null)
            DirAccess.CopyAbsolute(_settedImagePath, _selectedModFolder + @"\PreviewPicture.png");
        _settedImagePath = null;

        // Updating the showed mod info
        ShowModInfo(_selectedMod);
    }

    public void ModNameChanged(string value)
    {
        _wereChangesMade = true;
        UpdateSaveChangesButton();
        _settedModName = value;
    }
    public void ModFolderNameChanged(string value)
    {
        _wereChangesMade = true;
        CheckFolderNameCorrectness(value);
        UpdateSaveChangesButton();
    }
    // Yes, almost like in CreateModMenu. I haven't figured out how to do it competently without copypasting.
    private bool CheckFolderNameCorrectness(string name)
    {
        Color redColor = new Color(0.788f, 0.141f, 0.392f);
        Color greenColor = new Color(0.357f, 0.702f, 0.38f);

        const string EDIT_MOD_DATA_LINK = "MarginContainer/VBoxContainer/Tabs/Edit Mod Data/MarginC/HBoxC/";

        var isFolderNameValid = GetNode<RichTextLabel>(EDIT_MOD_DATA_LINK + "VBoxC/MarginC/ScrollC/VBoxC/IsFolderNameValid");
        var saveTheChangesButton = GetNode<ButtonWithText>(EDIT_MOD_DATA_LINK + "VBoxC2/NPR2/MarginC/VBoxC/Control/SaveChangesButton");

        void NameIsntCorrectMessage(string messageText)
        {
            isFolderNameValid.Text = messageText;
            _isFolderNameCorrect = false;
            isFolderNameValid.Modulate = redColor;
        }
        if (FileSystemExtension.HasSpecialChars(name))
        {
            NameIsntCorrectMessage("Remove the special characters, please");
            return false;
        }
        else if (name == "")
        {
            NameIsntCorrectMessage("Enter something");
            return false;
        }
        else if (name.All(c => c == ' '))
        {
            NameIsntCorrectMessage("Bro, why the spaces?");
            return false;
        }
        else if (DirAccess.DirExistsAbsolute(DefaultModsPath + name) && name != Path.GetFileName(_selectedModFolder))
        {
            NameIsntCorrectMessage("There's a folder with the same name");
            return false;
        }


        //
        switch (name.ToLower())
        {
            case "upandup" or "up and up" or "up_and_up":
                NameIsntCorrectMessage("Heeey! No");
                return false;
            case "xa":
                NameIsntCorrectMessage("Not a number");
                return false;
            case "jiofef" or "jio yoba fefski" or "jio_yoba_fefski":
                NameIsntCorrectMessage("Pick another name, I don't like it");
                return false;
            case "tomatoes":
                NameIsntCorrectMessage("Cucumbers are better");
                return false;
            case "when's the game coming out":
                NameIsntCorrectMessage("Tomorrow at 3");
                return false;
        }

        // If the name is correct
        isFolderNameValid.Text = "The name is valid";
        if (name == "000000000")
            isFolderNameValid.Text = "000000000, bro";
        _settedModFolderName = name;
        _isFolderNameCorrect = true;
        UpdateSaveChangesButton();
        isFolderNameValid.Modulate = greenColor;

        return true;
    }

    public void ModDescriptionChanged()
    {
        _wereChangesMade = true;
        UpdateSaveChangesButton();
        _settedModDescription = GetNode<TextEdit>("MarginContainer/VBoxContainer/Tabs/Edit Mod Data/MarginC/HBoxC/VBoxC/MarginC/ScrollC/VBoxC/DescriptionText").Text;
    }

    public void UpdateSaveChangesButton()
    {
        bool CanSave = _isFolderNameCorrect && _wereChangesMade;

        var saveChangesButton = GetNode<ButtonWithText>("MarginContainer/VBoxContainer/Tabs/Edit Mod Data/MarginC/HBoxC/VBoxC2/NPR2/MarginC/VBoxC/Control/SaveChangesButton");

        saveChangesButton.Disabled = !CanSave;
    }

    public void ModOptionsMoreInfo()
    {
        var infoWindow = (ConfirmationWindow)GD.Load<PackedScene>("res://Content/Scenes/Interface&Menu/RareScenes/ModOptionsInfo.tscn").Instantiate();
        GetParent().AddChild(infoWindow);
    }
    #endregion
}
