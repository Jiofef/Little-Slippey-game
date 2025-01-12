using Godot;
using System;
using static ModDataManager;
using static ModDataManager.ModOptionData;

/// <summary>
/// For correct operation, set the editable data via SetModOptionData
/// </summary>
public partial class ModOptionEdit : MarginContainer
{
    // There is no need to emit any signals, etc. to WorkshopMenu, as the object is the same, and if you change the value here, the values there change.
    public ModOptionData ModOptionData = new ModOptionData();
    private Control _optionMethod;

    /// <summary>
    /// Needed for correct work
    /// </summary>
    public void SetModOptionData(ModOptionData data)
    {
        ModOptionData = data;

        // Upper options
        GetNode<LineEdit>("VBoxC/OptionNameEdit").Text = data.Name;
        GetNode<LineEdit>("VBoxC/HBoxC/KeyEdit").Text = data.Key;


        // Hidden options
        GetNode<TextEdit>("VBoxC/ExpandableBox/DescriptionEdit").Text = data.Description;
        GetNode<OptionButton>("VBoxC/ExpandableBox/ValueTypeOption").Selected = (int)data.ValueType;
        GetNode<OptionButton>("VBoxC/ExpandableBox/OptionMethod").Selected = (int)data.OptionMethod;

        // Setting the default value option
        var defaultValueCheckBox = GetNode<CheckBox>("VBoxC/ExpandableBox/DefaultValueField");
        switch(data.OptionMethod)
        {
            case OptionMethodEnum.CheckBox:
                defaultValueCheckBox.ButtonPressed = data.DefaultValue.AsBool();
                break;
            case OptionMethodEnum.CheckButton:
                var checkButton = new CheckButton();
                checkButton.ButtonPressed = data.DefaultValue.AsBool();

                ReplaceCheckBoxBy(checkButton);
                break;

            case OptionMethodEnum.SpinBox:
                var spinBox = new SpinBox();
                spinBox.Value = data.DefaultValue.AsDouble();
                spinBox.MinValue = int.MinValue;
                spinBox.MaxValue = int.MaxValue;
                spinBox.AllowGreater = true;
                spinBox.AllowGreater = true;

                ReplaceCheckBoxBy(spinBox);
                break;
            case OptionMethodEnum.HSlider:
                var hSlider = new HSlider();
                hSlider.Value = data.DefaultValue.AsDouble();

                hSlider.MinValue = ((ModNumberOptionData)data.modOptionTypeData).MinNumValue;
                hSlider.MaxValue = ((ModNumberOptionData)data.modOptionTypeData).MaxNumValue;
                hSlider.AllowGreater = true;
                hSlider.AllowGreater = true;

                ReplaceCheckBoxBy(hSlider);
                break;

            case OptionMethodEnum.LineEdit:
                var lineEdit = new LineEdit();
                lineEdit.Text = data.DefaultValue.AsString();

                lineEdit.PlaceholderText = ((ModStringOptionData)data.modOptionTypeData).PlaceholderText;

                ReplaceCheckBoxBy(lineEdit);
                break;
            case OptionMethodEnum.TextEdit:
                var textEdit = new TextEdit();
                textEdit.Text = data.DefaultValue.AsString();

                textEdit.PlaceholderText = ((ModStringOptionData)data.modOptionTypeData).PlaceholderText;

                ReplaceCheckBoxBy(textEdit);
                break;

            case OptionMethodEnum.ColorPickerButton:
                var colorPickerButton = new ColorPickerButton();
                colorPickerButton.Color = data.DefaultValue.AsColor();

                ReplaceCheckBoxBy(colorPickerButton);
                break;

            case OptionMethodEnum.OptionButton:
                var optionButton = new OptionButton();
                optionButton.Selected = data.DefaultValue.AsInt32();

                ReplaceCheckBoxBy(optionButton);
                break;
        }
        void ReplaceCheckBoxBy(Control node)
        {
            defaultValueCheckBox.ReplaceBy(node);
            defaultValueCheckBox.QueueFree();
            _optionMethod = node;
        }


        // String options
        GetNode<SpinBox>("VBoxC/ExpandableBox/StringOptions/MaxLengthBox").Value = ((ModStringOptionData)data.modOptionTypeData).MaxStringLength;
        GetNode<TextEdit>("VBoxC/ExpandableBox/StringOptions/PlaceholderTextEdit").Text = ((ModStringOptionData)data.modOptionTypeData).PlaceholderText;

        // Number options
        GetNode<SpinBox>("VBoxC/ExpandableBox/NubmerOptions/MinValueBox").Value = ((ModNumberOptionData)data.modOptionTypeData).MinNumValue;
        GetNode<SpinBox>("VBoxC/ExpandableBox/NubmerOptions/MaxValueBox").Value = ((ModNumberOptionData)data.modOptionTypeData).MaxNumValue;
        GetNode<CheckBox>("VBoxC/ExpandableBox/NubmerOptions/AllowGreaterBox").ButtonPressed = ((ModNumberOptionData)data.modOptionTypeData).AllowGreater;
        GetNode<CheckBox>("VBoxC/ExpandableBox/NubmerOptions/AllowLesserBox").ButtonPressed = ((ModNumberOptionData)data.modOptionTypeData).AllowLesser;
        GetNode<SpinBox>("VBoxC/ExpandableBox/NubmerOptions/RoundToBox").Value = ((ModNumberOptionData)data.modOptionTypeData).RoundTo;

        // Enum options



    }

    /*
        All the functions here perform qualitatively the same thing, which is why I've compressed them so ugly.
    */

    public void OptionNameChanged(string value) { ModOptionData.Name = value; }

    public void KeyChanged(string value){ ModOptionData.Key = value; }

    public void DescriptionChanged() { ModOptionData.Description = GetNode<TextEdit>("VBoxC/ExpandableBox/DescriptionEdit").Text; }

    public void SetDefaultValue(Variant value){ ModOptionData.DefaultValue = value; }

    public void SetValueType(int index) { ModOptionData.ValueType = (ValueTypeEnum)index; }

    // By "method" is meant the object with which the value is entered. For example, CheckBox or TextEdit
    public void SetOptionMethod(int index) { ModOptionData.OptionMethod = (OptionMethodEnum)index; }




    // String options
    public void SetMaxStringLength(int value) { ((ModStringOptionData)ModOptionData.modOptionTypeData).MaxStringLength = value; }

    public void PlaceholderTextChanged() {
        string value = GetNode<TextEdit>("VBoxC/ExpandableBox/StringOptions/PlaceholderTextEdit").Text;
        ((ModStringOptionData)ModOptionData.modOptionTypeData).PlaceholderText = value;

        if (_optionMethod is TextEdit textEdit)
            textEdit.PlaceholderText = value;
        else if (_optionMethod is LineEdit lineEdit)
            lineEdit.PlaceholderText = value;
    }




    // Number options
    public void SetMinValue(double value) { ((ModNumberOptionData)ModOptionData.modOptionTypeData).MinNumValue = value;
    if (_optionMethod is HSlider hSlider)
            hSlider.MinValue = value;
    }

    public void SetMaxValue(double value) { ((ModNumberOptionData)ModOptionData.modOptionTypeData).MaxNumValue = value;
        if (_optionMethod is HSlider hSlider)
            hSlider.MaxValue = value;
    }

    public void SetAllowGreater(bool value) { ((ModNumberOptionData)ModOptionData.modOptionTypeData).AllowGreater = value; }

    public void SetAllowLesser(bool value) { ((ModNumberOptionData)ModOptionData.modOptionTypeData).AllowLesser = value; }

    public void SetRoundTo(double value) { ((ModNumberOptionData)ModOptionData.modOptionTypeData).RoundTo = value; }




    // Enum options
}
