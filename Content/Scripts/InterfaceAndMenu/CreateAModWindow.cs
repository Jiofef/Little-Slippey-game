using Godot;
using System;
using System.IO;
using System.Linq;
using static ModManager;

[Tool]
public partial class CreateAModWindow : DraggableWindow
{
    public delegate void CreateModEventHandler(CreateModParams @params);
    public event CreateModEventHandler CreateMod;

    private CreateModParams _params = new CreateModParams();

    Color _redColor = new Color(0.788f, 0.141f, 0.392f);
    Color _greenColor = new Color(0.357f, 0.702f, 0.38f);

    RichTextLabel _isFolderNameValid;
    TextureButton _acceptButton;

    LineEdit _nameEdit, _folderNameEdit;

    public override void _Ready()
    {
        // Initializing nodes
        _isFolderNameValid = GetNode<RichTextLabel>("MarginContainer/VBoxContainer/VBoxContainer/IsFolderNameValid");
        _acceptButton = GetNode<TextureButton>("MarginContainer/VBoxContainer/HBoxContainer/AcceptButton");
        _nameEdit = GetNode<LineEdit>("MarginContainer/VBoxContainer/VBoxContainer/NameEdit");
        _folderNameEdit = GetNode<LineEdit>("MarginContainer/VBoxContainer/VBoxContainer/FolderNameEdit");

        SelectType((int)DefaultModType);
    }


    private void SelectType(int typeID)
    {
        var type = (ModType)typeID;
        _params.modType = type;


        // Setting the default name for a mod by its type
        if (!_wasModNameChanged)
        {
            _nameEdit.Text = DefaultModTypeNames[type];
            _params.ModName = _nameEdit.Text;
        }

        // Same but for folder name
        if (!_wasModFolderNameChanged)
        {
            string path = DefaultModsPath + DefaultModTypeNames[type];
            _folderNameEdit.Text = Path.GetFileName(FileSystemExtension.MakeUniqueFilePath(path));
            CheckFolderNameCorrectness(_folderNameEdit.Text);
        }
    }

    private bool _wasModFolderNameChanged = false;
    private void FolderNameChanged(string value) 
    {
        _wasModFolderNameChanged = true;

        CheckFolderNameCorrectness(value);
    }

    private bool CheckFolderNameCorrectness(string name)
    {
        void NameIsntCorrectMessage(string messageText)
        {
            _isFolderNameValid.Text = messageText;
            _acceptButton.Disabled = true;
            _isFolderNameValid.Modulate = _redColor;
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
        else if (DirAccess.DirExistsAbsolute(DefaultModsPath + name))
        {
            NameIsntCorrectMessage("There's a folder with the same name");
            return false;
        }


        //
        switch(name.ToLower())
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
        _isFolderNameValid.Text = "The name is valid";
        if (name == "000000000")
            _isFolderNameValid.Text = "000000000, bro";
        _params.FolderName = name;
        _acceptButton.Disabled = false;
        _isFolderNameValid.Modulate = _greenColor;

        return true;
    }

    private bool _wasModNameChanged = false;
    private void ModNameChanged(string value)
    {
        _wasModNameChanged = true;
        _params.ModName = value;
    }
    private void SetCreateAdditionalFolders(bool value)
    {
        _params.CreateAdditionalFolders = value;
    }
    private void Accept()
    {
        CreateMod(_params);
        QueueFree();
    }
}
