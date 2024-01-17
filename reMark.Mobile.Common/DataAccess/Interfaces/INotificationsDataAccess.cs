using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using reMark.Mobile.Common.Model;

namespace reMark.Mobile.Common.DataAccess
{
    interface INotificationsDataAccess
    {
        Task SaveNotifications(List<Notification> notifications);

        Task<List<Notification>> GetNotifications();

        Task<List<Guid>> GetReadNotificationGuids();

        Task MarkAsRead(Guid notificationGuid);

        Task MarkAsRead(List<Guid> notificationGuids);

        Task MarkAsRead(Notification notification);

        Task MarkAllAsRead();
    }
}