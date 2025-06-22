using Godot;
using System;

public partial class NotificationCounter : Control
{
    private int _notificationsAmount;
    [Export] public int NotificationsAmount 
    { 
        get => _notificationsAmount;
        set 
        {
            var amount = GetNode<Label>("Amount");
            amount.Text = value.ToString();
            amount.Visible = value > 0;

            _notificationsAmount = value;
        }
    }
    [Export] public bool EnableAutoSetting = false;
    [Export] public string AutoSettingDicKey;

    public override void _Ready()
    {
        Update();
    }

    public void Update()
    {
        if (EnableAutoSetting)
        {
            NotificationsAmount = UnchangableMeta.NotificationsAmount[AutoSettingDicKey];
        }
    }

    public void Nullify()
    {
        NotificationsAmount = 0;

        if (EnableAutoSetting)
        {
            UnchangableMeta.NotificationsAmount[AutoSettingDicKey] = 0;
        }
    }
}
