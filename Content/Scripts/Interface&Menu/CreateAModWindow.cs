using Godot;
using System;
using static ModLoader;

[Tool]
public partial class CreateAModWindow : DraggableWindow
{
    [Signal] public delegate void CreateModEventHandler(ModType type, string folderName, string modName);
    private ModType _selectedType = ModType.map;
    private string _folderName, _modName;

    Color _redColor = new Color(0.788f, 0.141f, 0.392f);
    Color _greenColor = new Color(0.357f, 0.702f, 0.38f);

    RichTextLabel _isFolderNameValid;
    TextureButton _acceptButton;
    public override void _Ready()
    {
        _isFolderNameValid = GetNode<RichTextLabel>("MarginContainer/VBoxContainer/VBoxContainer/IsFolderNameValid");
        _acceptButton = GetNode<TextureButton>("MarginContainer/VBoxContainer/HBoxContainer/AcceptButton");
    }


    private void SelectType(int typeID)
    {
        var type = (ModType)typeID;
        _selectedType = type;
    }

    private void FolderNameChanged(string value) 
    {
        if (FileSystemExtension.HasSpecialChars(value))
        {
            _isFolderNameValid.Text = "Remove the special characters, please";
            _acceptButton.Disabled = true;
        }
        else if (value == "")
        {
            _isFolderNameValid.Text = "Enter something";
            _acceptButton.Disabled = true;
        }
        else if (DirAccess.DirExistsAbsolute(DefaultModsPath + value))
        {
            _isFolderNameValid.Text = "There's a folder with the same name";
            _acceptButton.Disabled = true;
        }
        else
        {
            _isFolderNameValid.Text = "The name is valid";
            _folderName = value;
            _acceptButton.Disabled = false;
        }

        if (!_acceptButton.Disabled)
            _isFolderNameValid.Modulate = _greenColor;
        else
            _isFolderNameValid.Modulate = _redColor;
    }
    private void SetModName(string value)
    {
        _modName = value;
    }
    private void Accept()
    {
        EmitSignal("CreateMod", (int)_selectedType, _folderName, _modName);
        QueueFree();
    }
}
