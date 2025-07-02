using System;

public class InGameNotifierComponent
{
	string NotifyKey { get; set; }
	int NotificationMultiplier { get; set; }

	public InGameNotifierComponent(string notifyKey, int notificationMultiplier = 1)
	{
		NotifyKey = notifyKey;
		NotificationMultiplier = notificationMultiplier;
	}

	public void CallNotify()
	{
		UnchangableMeta.NotificationsAmount[NotifyKey] += NotificationMultiplier;
	}
}