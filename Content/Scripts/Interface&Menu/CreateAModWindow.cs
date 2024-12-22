using Godot;
using System;
using static ModLoader;

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
    public override void _Ready()
    {
        _isFolderNameValid = GetNode<RichTextLabel>("MarginContainer/VBoxContainer/VBoxContainer/IsFolderNameValid");
        _acceptButton = GetNode<TextureButton>("MarginContainer/VBoxContainer/HBoxContainer/AcceptButton");
    }


    private void SelectType(int typeID)
    {
        var type = (ModType)typeID;
        _params.modType = type;
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
            _params.FolderName = value;
            _acceptButton.Disabled = false;
        }

        if (!_acceptButton.Disabled)
            _isFolderNameValid.Modulate = _greenColor;
        else
            _isFolderNameValid.Modulate = _redColor;
    }
    private void ModNameChanged(string value)
    {
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
