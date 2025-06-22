using Godot;
using OtherExtension;
using System;
using static ModDataManager;
using static ModDataManager.ModOptionData;

/// <summary>
/// For correct operation, set the editable data via SetModOptionData
/// </summary>
public partial class ModOptionEdit : MarginContainer
{
    // There is no need to emit any signals, etc. to WorkshopMenu, as the object is the same, and if you change the value here, the values there change.\

    public TreeNode<Object> DataTreeItem;
    public ModOptionData Data = new ModOptionData();
    private Control _optionMethod;

    /// <summary>
    /// Needed for correct work
    /// </summary>
    public void SetModOptionDataTreeItem(TreeNode<Object> dataItem)
    {
        DataTreeItem = dataItem;
        Data = (ModOptionData)dataItem.Value;

        // Upper options
        GetNode<LineEdit>("VBoxC/OptionNameEdit").Text = Data.Name;
        GetNode<LineEdit>("VBoxC/HBoxC/KeyEdit").Text = Data.Key;


        // Hidden options
        GetNode<TextEdit>("VBoxC/ExpandableBox/DescriptionEdit").Text = Data.Description;
        GetNode<OptionButton>("VBoxC/ExpandableBox/ValueTypeOption").Selected = (int)Data.ValueType;
        GetNode<OptionButton>("VBoxC/ExpandableBox/OptionMethod").Selected = (int)Data.OptionMethod;

        // Setting the default value option
        var defaultValueCheckBox = GetNode<CheckBox>("VBoxC/ExpandableBox/DefaultValueField");
        switch(Data.OptionMethod)
        {
            case OptionMethodEnum.CheckBox:
                defaultValueCheckBox.ButtonPressed = Data.DefaultValue.AsBool();
                break;
            case OptionMethodEnum.CheckButton:
                var checkButton = new CheckButton();
                checkButton.ButtonPressed = Data.DefaultValue.AsBool();

                ReplaceCheckBoxBy(checkButton);
                break;

            case OptionMethodEnum.SpinBox:
                var spinBox = new SpinBox();
                spinBox.Value = Data.DefaultValue.AsDouble();
                spinBox.MinValue = int.MinValue;
                spinBox.MaxValue = int.MaxValue;
                spinBox.AllowGreater = true;
                spinBox.AllowGreater = true;

                ReplaceCheckBoxBy(spinBox);
                break;
            case OptionMethodEnum.HSlider:
                var hSlider = new HSlider();
                hSlider.Value = Data.DefaultValue.AsDouble();

                hSlider.MinValue = ((ModNumberOptionData)Data.modOptionTypeData).MinNumValue;
                hSlider.MaxValue = ((ModNumberOptionData)Data.modOptionTypeData).MaxNumValue;
                hSlider.AllowGreater = true;
                hSlider.AllowGreater = true;

                ReplaceCheckBoxBy(hSlider);
                break;

            case OptionMethodEnum.LineEdit:
                var lineEdit = new LineEdit();
                lineEdit.Text = Data.DefaultValue.AsString();

                lineEdit.PlaceholderText = ((ModStringOptionData)Data.modOptionTypeData).PlaceholderText;

                ReplaceCheckBoxBy(lineEdit);
                break;
            case OptionMethodEnum.TextEdit:
                var textEdit = new TextEdit();
                textEdit.Text = Data.DefaultValue.AsString();

                textEdit.PlaceholderText = ((ModStringOptionData)Data.modOptionTypeData).PlaceholderText;

                ReplaceCheckBoxBy(textEdit);
                break;

            case OptionMethodEnum.ColorPickerButton:
                var colorPickerButton = new ColorPickerButton();
                colorPickerButton.Color = Data.DefaultValue.AsColor();

                ReplaceCheckBoxBy(colorPickerButton);
                break;

            case OptionMethodEnum.OptionButton:
                var optionButton = new OptionButton();
                optionButton.Selected = Data.DefaultValue.AsInt32();

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
        GetNode<SpinBox>("VBoxC/ExpandableBox/StringOptions/MaxLengthBox").Value = ((ModStringOptionData)Data.modOptionTypeData).MaxStringLength;
        GetNode<TextEdit>("VBoxC/ExpandableBox/StringOptions/PlaceholderTextEdit").Text = ((ModStringOptionData)Data.modOptionTypeData).PlaceholderText;

        // Number options
        GetNode<SpinBox>("VBoxC/ExpandableBox/NubmerOptions/MinValueBox").Value = ((ModNumberOptionData)Data.modOptionTypeData).MinNumValue;
        GetNode<SpinBox>("VBoxC/ExpandableBox/NubmerOptions/MaxValueBox").Value = ((ModNumberOptionData)Data.modOptionTypeData).MaxNumValue;
        GetNode<CheckBox>("VBoxC/ExpandableBox/NubmerOptions/AllowGreaterBox").ButtonPressed = ((ModNumberOptionData)Data.modOptionTypeData).AllowGreater;
        GetNode<CheckBox>("VBoxC/ExpandableBox/NubmerOptions/AllowLesserBox").ButtonPressed = ((ModNumberOptionData)Data.modOptionTypeData).AllowLesser;
        GetNode<SpinBox>("VBoxC/ExpandableBox/NubmerOptions/RoundToBox").Value = ((ModNumberOptionData)Data.modOptionTypeData).RoundTo;

        // Enum options



    }

    public void RemoveOption()
    {
        DataTreeItem.Free();
        QueueFree();
    }

    /*
        All the functions here perform qualitatively the same thing, which is why I've compressed them so ugly.
    */

    public void OptionNameChanged(string value) { Data.Name = value; }

    public void KeyChanged(string value){ Data.Key = value; }

    public void DescriptionChanged() { Data.Description = GetNode<TextEdit>("VBoxC/ExpandableBox/DescriptionEdit").Text; }

    public void SetDefaultValue(Variant value){ Data.DefaultValue = value; }

    public void SetValueType(int index) { Data.ValueType = (ValueTypeEnum)index; }

    // By "method" is meant the object with which the value is entered. For example, CheckBox or TextEdit
    public void SetOptionMethod(int index) { Data.OptionMethod = (OptionMethodEnum)index; }




    // String options
    public void SetMaxStringLength(int value) { ((ModStringOptionData)Data.modOptionTypeData).MaxStringLength = value; }

    public void PlaceholderTextChanged() {
        string value = GetNode<TextEdit>("VBoxC/ExpandableBox/StringOptions/PlaceholderTextEdit").Text;
        ((ModStringOptionData)Data.modOptionTypeData).PlaceholderText = value;

        if (_optionMethod is TextEdit textEdit)
            textEdit.PlaceholderText = value;
        else if (_optionMethod is LineEdit lineEdit)
            lineEdit.PlaceholderText = value;
    }




    // Number options
    public void SetMinValue(double value) { ((ModNumberOptionData)Data.modOptionTypeData).MinNumValue = value;
    if (_optionMethod is HSlider hSlider)
            hSlider.MinValue = value;
    }

    public void SetMaxValue(double value) { ((ModNumberOptionData)Data.modOptionTypeData).MaxNumValue = value;
        if (_optionMethod is HSlider hSlider)
            hSlider.MaxValue = value;
    }

    public void SetAllowGreater(bool value) { ((ModNumberOptionData)Data.modOptionTypeData).AllowGreater = value; }

    public void SetAllowLesser(bool value) { ((ModNumberOptionData)Data.modOptionTypeData).AllowLesser = value; }

    public void SetRoundTo(double value) { ((ModNumberOptionData)Data.modOptionTypeData).RoundTo = value; }




    // Enum options
}
