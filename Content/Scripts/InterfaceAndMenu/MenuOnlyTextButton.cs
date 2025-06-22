using Godot;
using System;

[Tool]
public partial class MenuOnlyTextButton : EnhancedButton
{
    public RichTextLabel GetLabel()
    {
        return GetNode<RichTextLabel>("MarginC/Text");
    }

    protected const string PREFIX = "[center]";
    private string _text = "Text";
    [Export(PropertyHint.MultilineText)] public string Text
    {
        get => _text;
        set {
            _text = value;
            CallDeferred("UpdateText");
        }
    }
    public void UpdateText()
    {
        SetLabelText(_text);
    }

    /// <summary>
    /// Sets the text without changing the text property (calling UpdateText will return the old text)
    /// </summary>
    public void SetLabelText(string text)
    {
        GetLabel().Text = PREFIX + Tr(text);
    }



    //private int _fontSize = 22;
    //[Export] public int FontSize
    //{
    //    get => _fontSize;
    //    set
    //    {
    //        _fontSize = value;
    //        CallDeferred("UpdateFontSize");
    //    }
    //}
    //public void UpdateFontSize()
    //{
    //    GetLabel().AddThemeFontSizeOverride("font_size", _fontSize);
    //}



    //private Color _fontColor = new Color("323232");
    //[Export]
    //public Color FontColor
    //{
    //    get => _fontColor;
    //    set
    //    {
    //        _fontColor = value;
    //        CallDeferred("UpdateFontColor");
    //    }
    //}
    //public void UpdateFontColor()
    //{
    //    GetLabel().AddThemeColorOverride("font_color", _fontColor);
    //}
}
