using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Common.DataAccess
{
    interface INotificationsDataAccess
    {
        Task SaveNotifications(List<Notification> notifications);

        Task<List<Notification>> GetNotifications();

        Task<List<Guid>> GetReadNotificationGuids();

        Task MarkAsRead(Guid notificationGuid);

        Task MarkAsRead(Notification notification);

        Task MarkAllAsRead();
    }
}