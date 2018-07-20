using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Common.Manager
{
    public interface INotificationsManager
    {
        ObjectType[] EnabledObjectTypes { get; }
        DocumentBodyTypeRequest DocumentBodyTypeRequest { get; set; }

        Task Subscribe(DeviceType deviceType, string pushToken, string fcmToken, SourceType sourceType = SourceType.Auto);

        Task UnSubscribe(DeviceType deviceType, string pushToken, SourceType sourceType = SourceType.Auto);

        Task<List<Notification>> GetNotificationsAsync(DeviceType deviceType, string pushToken, SourceType sourceType = default(SourceType));

        Task<Dictionary<ModuleType, List<Folder>>> GetFoldersNotificationsAsync(DeviceType deviceType, string pushToken, SourceType sourceType = default(SourceType));

        Task SetFoldersNotificationsAsync(DeviceType deviceType, string pushToken, ModuleType moduleType, List<Folder> folderIds, bool enabled, SourceType sourceType = default(SourceType));

        Task<bool> GetCalendarNotificationsEnabledAsync(DeviceType deviceType, string pushToken, SourceType sourceType = default(SourceType));

        Task SetCalendarNotificationsEnabledAsync(DeviceType deviceType, string pushToken, bool enabled, SourceType sourceType = default(SourceType));

        Task<string> GetNotificationsSoundAsync(DeviceType deviceType, string pushToken, SourceType sourceType = default(SourceType));

        Task SetNotificationsSoundAsync(DeviceType deviceType, string pushToken, string soundName, SourceType sourceType = default(SourceType));

        Task ClearAllNotificationSettingsAsync(DeviceType deviceType, string pushToken, SourceType sourceType = default(SourceType));

        Task<object> GetRemoteObjectAsync(Notification notification, SourceType sourceType = default(SourceType));

        Task SaveNotification(Notification notification);

        Task MarkAsRead(Guid notificationGuid);

        Task MarkAsRead(Notification notification);

        Task MarkAllAsRead();
    }
}