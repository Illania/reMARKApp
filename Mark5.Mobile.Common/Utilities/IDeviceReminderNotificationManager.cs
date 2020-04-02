using System;
using System.Collections.Generic;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Common.Utilities
{
    public interface IDeviceReminderNotificationManager
    {
        void SetDeviceRemindersNotification(List<CalendarReminder> remindersToSet);
        void CancelDeviceReminderNotifications(List<CalendarReminder> remindersToCancel);
    }

}
