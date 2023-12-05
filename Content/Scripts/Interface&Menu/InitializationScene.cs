using Godot;

public partial class InitializationScene : Control
{
	public override void _Ready()
	{
        Meta.Instance.LoadOptions();
        Meta.Instance.ApplyOptions();
        UnchangableMeta.LoadSave();
        if (!UnchangableMeta.IsLanguageSetted)
        {
            GetNode<Control>("ChooseYourLanguage").Visible = true;
            GetNode<TextureButton>("ChooseYourLanguage/ChooseYourLanguageEng").GrabFocus();
        }
        else
            GetTree().ChangeSceneToFile("res://Content/Scenes/Interface&Menu/WelcomeToGOS.tscn");
    }

    public void SetLanguage(int languageNumber)
    {
        Meta.Instance.language = (Meta.Language)languageNumber;
        UnchangableMeta.IsLanguageSetted = true;
        Meta.Instance.ApplyOptions();
        Meta.Instance.SaveToFile();
        UnchangableMeta.SaveToFile();
        GetTree().ChangeSceneToFile("res://Content/Scenes/Interface&Menu/WelcomeToGOS.tscn");
    }
}
