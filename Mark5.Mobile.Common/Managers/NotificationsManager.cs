//
// Project: Mark5.Mobile.Common
// File: NotificationsManager.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mark5.Mobile.Common.DataAccess;
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Model.Containers;
using Mark5.Mobile.Common.Model.Converters;
using Mark5.Mobile.Common.Model.Exceptions;
using Mark5.Mobile.Common.Storage;
using Mark5.ServiceReference.AppService;
using DataContract = Mark5.ServiceReference.DataContract;

#pragma warning disable CS1701
namespace Mark5.Mobile.Common.Managers
{

    class NotificationsManager : AbstractManager, INotificationsManager
    {

        public ObjectType[] EnabledObjectTypes { get { return new[] { ObjectType.Document }; } }

        public DocumentBodyTypeRequest DocumentBodyTypeRequest { get; set; } = DocumentBodyTypeRequest.HtmlOnly;

        readonly IFoldersDataAccess foldersDataAccess;
        readonly INotificationsDataAccess notificationsDataAccess;

        public NotificationsManager(ConnectionInfo connectionInfo, IAppServiceProxy appServiceProxy, IFoldersDataAccess foldersDataAccess, INotificationsDataAccess notificationsDataAccess)
            : base(connectionInfo, appServiceProxy)
        {
            this.foldersDataAccess = foldersDataAccess;
            this.notificationsDataAccess = notificationsDataAccess;
        }

        public async Task Subscribe(DeviceType deviceType, string pushToken, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto) sourceType = CommonConfig.ReachabilityService.IsReachable ? SourceType.Remote : SourceType.Local;

            if (sourceType == SourceType.Remote)
            {
                await AppServiceProxy.SetFoldersNotificationsAsync(new DataContract.SetFoldersNotificationsParameters
                {
                    Token = Token,
                    DeviceType = deviceType.ConvertEnum<DataContract.DeviceType>(),
                    PushToken = pushToken,
                    Enabled = true
                });

                return;
            }

            if (sourceType == SourceType.Local)
            {
                throw new InvalidSourceTypeException("This action can only be performed when online.");
            }

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task UnSubscribe(DeviceType deviceType, string pushToken, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto) sourceType = CommonConfig.ReachabilityService.IsReachable ? SourceType.Remote : SourceType.Local;

            if (sourceType == SourceType.Remote)
            {
                await AppServiceProxy.SetFoldersNotificationsAsync(new DataContract.SetFoldersNotificationsParameters
                {
                    Token = Token,
                    DeviceType = deviceType.ConvertEnum<DataContract.DeviceType>(),
                    PushToken = pushToken,
                    Enabled = false
                });

                return;
            }

            if (sourceType == SourceType.Local)
            {
                throw new InvalidSourceTypeException("This action can only be performed when online.");
            }

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task<List<Notification>> GetNotificationsAsync(DeviceType deviceType, string pushToken, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto) sourceType = CommonConfig.ReachabilityService.IsReachable ? SourceType.Remote : SourceType.Local;

            if (sourceType == SourceType.Remote)
            {
                var result = await AppServiceProxy.GetNotificationsAsync(new DataContract.GetNotificationsParameters
                {
                    Token = Token,
                    DeviceType = deviceType.ConvertEnum<DataContract.DeviceType>(),
                    PushToken = pushToken
                });

                var notifications = result.Notifications.WhereNotNull().Select(n => n.Convert()).Where(n => EnabledObjectTypes.Contains(n.ObjectType)).OrderByDescending(n => n.DateTimeTimestamp).ToList();

                await notificationsDataAccess.SaveNotifications(notifications);

                var readGuids = await notificationsDataAccess.GetReadNotificationGuids();
                if (readGuids.Count > 0)
                {
                    notifications.ForEach(n => n.IsRead = readGuids.Contains(n.Guid));
                }

                return notifications;
            }

            if (sourceType == SourceType.Local)
            {
                return await notificationsDataAccess.GetNotifications();
            }

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task<Dictionary<ModuleType, List<Folder>>> GetFoldersNotificationsAsync(DeviceType deviceType, string pushToken, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto) sourceType = CommonConfig.ReachabilityService.IsReachable ? SourceType.Remote : SourceType.Local;

            if (sourceType == SourceType.Remote)
            {
                var result = await AppServiceProxy.GetFoldersNotificationsAsync(new DataContract.GetFoldersNotificationsParameters
                {
                    Token = Token,
                    DeviceType = deviceType.ConvertEnum<DataContract.DeviceType>(),
                    PushToken = pushToken
                });

                return new Dictionary<ModuleType, List<Folder>>
                {
                    [ModuleType.Documents] = result.DocumentFolders?.WhereNotNull().Select(f => f.Convert()).ToList() ?? new List<Folder>(),
                    [ModuleType.Contacts] = result.ContactFolders?.WhereNotNull().Select(f => f.Convert()).ToList() ?? new List<Folder>(),
                    [ModuleType.Shortcodes] = result.ShortcodeFolders?.WhereNotNull().Select(f => f.Convert()).ToList() ?? new List<Folder>()
                };
            }

            if (sourceType == SourceType.Local)
            {
                throw new InvalidSourceTypeException("This action can only be performed when online.");
            }

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task SetFoldersNotificationsAsync(DeviceType deviceType, string pushToken, ModuleType moduleType, List<Folder> folders, bool enabled, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto) sourceType = CommonConfig.ReachabilityService.IsReachable ? SourceType.Remote : SourceType.Local;

            if (sourceType == SourceType.Remote)
            {
                var folderIds = folders.Select(f => f.Id).ToArray();

                await AppServiceProxy.SetFoldersNotificationsAsync(new DataContract.SetFoldersNotificationsParameters
                {
                    Token = Token,
                    DeviceType = deviceType.ConvertEnum<DataContract.DeviceType>(),
                    PushToken = pushToken,
                    ModuleType = moduleType.ConvertEnum<DataContract.ModuleType>(),
                    FolderIds = folderIds,
                    Enabled = enabled
                });

                await foldersDataAccess.SetSubscribed(moduleType, folders, enabled);

                var favoriteFolders = await FileSystemStorage.GetFavoriteFoldersAsync() ?? new Dictionary<ModuleType, List<Folder>>();

                List<Folder> moduleFavoriteFolders;
                if (favoriteFolders.TryGetValue(moduleType, out moduleFavoriteFolders))
                {
                    foreach (var item in moduleFavoriteFolders.Where(f => folderIds.Contains(f.Id)))
                    {
                        item.Subscribed = enabled;
                    }
                }

                await FileSystemStorage.SaveFavoriteFoldersAsync(favoriteFolders);

                return;
            }

            if (sourceType == SourceType.Local)
            {
                throw new InvalidSourceTypeException("This action can only be performed when online.");
            }

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task<bool> GetCalendarNotificationsEnabledAsync(DeviceType deviceType, string pushToken, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto) sourceType = CommonConfig.ReachabilityService.IsReachable ? SourceType.Remote : SourceType.Local;

            if (sourceType == SourceType.Remote)
            {
                var result = await AppServiceProxy.GetCalendarNotificationsEnabledAsync(new DataContract.GetCalendarNotificationsEnabledParameters
                {
                    Token = Token,
                    DeviceType = deviceType.ConvertEnum<DataContract.DeviceType>(),
                    PushToken = pushToken
                });

                var enabled = result.Enabled;

                var notificationSettings = await FileSystemStorage.GetNotificationSettingsAsync() ?? new NotificationSettings();
                notificationSettings.CalendarNotificationsEnabled = enabled;
                await FileSystemStorage.SaveNotificationSettingsAsync(notificationSettings);

                return enabled;
            }

            if (sourceType == SourceType.Local)
            {
                var notificationSettings = await FileSystemStorage.GetNotificationSettingsAsync() ?? new NotificationSettings();
                return notificationSettings.CalendarNotificationsEnabled;
            }

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task SetCalendarNotificationsEnabledAsync(DeviceType deviceType, string pushToken, bool enabled, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto) sourceType = CommonConfig.ReachabilityService.IsReachable ? SourceType.Remote : SourceType.Local;

            if (sourceType == SourceType.Remote)
            {
                await AppServiceProxy.SetCalendarNotificationsEnabledAsync(new DataContract.SetCalendarNotificationsEnabledParameters
                {
                    Token = Token,
                    DeviceType = deviceType.ConvertEnum<DataContract.DeviceType>(),
                    PushToken = pushToken,
                    Enabled = enabled
                });

                var notificationSettings = await FileSystemStorage.GetNotificationSettingsAsync() ?? new NotificationSettings();
                notificationSettings.CalendarNotificationsEnabled = enabled;
                await FileSystemStorage.SaveNotificationSettingsAsync(notificationSettings);

                return;
            }

            if (sourceType == SourceType.Local)
            {
                throw new InvalidSourceTypeException("This action can only be performed when online.");
            }

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task<string> GetNotificationsSoundAsync(DeviceType deviceType, string pushToken, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto) sourceType = CommonConfig.ReachabilityService.IsReachable ? SourceType.Remote : SourceType.Local;

            if (sourceType == SourceType.Remote)
            {
                var result = await AppServiceProxy.GetNotificationsSoundAsync(new DataContract.GetNotificationsSoundParameters
                {
                    Token = Token,
                    DeviceType = deviceType.ConvertEnum<DataContract.DeviceType>(),
                    PushToken = pushToken
                });

                var soundName = result.SoundName;

                var notificationSettings = await FileSystemStorage.GetNotificationSettingsAsync() ?? new NotificationSettings();
                notificationSettings.SoundName = soundName;
                await FileSystemStorage.SaveNotificationSettingsAsync(notificationSettings);

                return soundName;
            }

            if (sourceType == SourceType.Local)
            {
                var notificationSettings = await FileSystemStorage.GetNotificationSettingsAsync() ?? new NotificationSettings();
                return notificationSettings.SoundName;
            }

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task SetNotificationsSoundAsync(DeviceType deviceType, string pushToken, string soundName, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto) sourceType = CommonConfig.ReachabilityService.IsReachable ? SourceType.Remote : SourceType.Local;

            if (sourceType == SourceType.Remote)
            {
                await AppServiceProxy.SetNotificationsSoundAsync(new DataContract.SetNotificationsSoundParameters
                {
                    Token = Token,
                    DeviceType = deviceType.ConvertEnum<DataContract.DeviceType>(),
                    PushToken = pushToken,
                    SoundName = soundName
                });

                var notificationSettings = await FileSystemStorage.GetNotificationSettingsAsync() ?? new NotificationSettings();
                notificationSettings.SoundName = soundName;
                await FileSystemStorage.SaveNotificationSettingsAsync(notificationSettings);

                return;
            }

            if (sourceType == SourceType.Local)
            {
                throw new InvalidSourceTypeException("This action can only be performed when online.");
            }

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task ClearAllNotificationSettingsAsync(DeviceType deviceType, string pushToken, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto) sourceType = CommonConfig.ReachabilityService.IsReachable ? SourceType.Remote : SourceType.Local;

            if (sourceType == SourceType.Remote)
            {
                await AppServiceProxy.ClearAllNotificationsAsync(new DataContract.ClearAllNotificationsParameters
                {
                    Token = Token,
                    DeviceType = deviceType.ConvertEnum<DataContract.DeviceType>(),
                    PushToken = pushToken
                });

                await foldersDataAccess.SetAllSubscribed(false);

                var favoriteFolders = await FileSystemStorage.GetFavoriteFoldersAsync() ?? new Dictionary<ModuleType, List<Folder>>();
                foreach (var favoriteFolder in favoriteFolders.Values.SelectMany(f => f))
                {
                    favoriteFolder.Subscribed = false;
                }
                await FileSystemStorage.SaveFavoriteFoldersAsync(favoriteFolders);
                await FileSystemStorage.SaveNotificationSettingsAsync(new NotificationSettings());

                return;
            }

            if (sourceType == SourceType.Local)
            {
                throw new InvalidSourceTypeException("This action can only be performed when online.");
            }

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task<object> GetRemoteObjectAsync(Notification notification, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto) sourceType = CommonConfig.ReachabilityService.IsReachable ? SourceType.Remote : SourceType.Local;

            if (sourceType == SourceType.Remote)
            {
                var objectId = notification.ObjectId;
                var folderId = notification.FolderId;

                if (notification.ObjectType == ObjectType.Document)
                {
                    var result = await AppServiceProxy.GetDocumentAsync(new DataContract.GetDocumentParameters
                    {
                        Token = Token,
                        FolderId = folderId,
                        DocumentId = objectId,
                        BodyRequest = DocumentBodyTypeRequest.ConvertEnum<DataContract.DocumentBodyTypeRequest>(),
                        IncludePreview = true
                    });

                    return new DocumentContainer(result.DocumentPreview.Convert(), result.Document.Convert());
                }
                if (notification.ObjectType == ObjectType.Contact)
                {
                    var result = await AppServiceProxy.GetContactAsync(new DataContract.GetContactParameters
                    {
                        Token = Token,
                        FolderId = folderId,
                        ContactId = objectId,
                        IncludePreview = true
                    });

                    return new ContactContainer(result.ContactPreview.Convert(), result.Contact.Convert());
                }
                if (notification.ObjectType == ObjectType.Shortcode)
                {
                    var result = await AppServiceProxy.GetShortcodeAsync(new DataContract.GetShortcodeParameters
                    {
                        Token = Token,
                        FolderId = folderId,
                        ShortcodeId = objectId,
                        IncludePreview = true
                    });

                    return new ShortcodeContainer(result.ShortcodePreview.Convert(), result.Shortcode.Convert());
                }
                if (notification.ObjectType == ObjectType.CalendarTask)
                {
                    var result = await AppServiceProxy.GetCalendarTaskAsync(new DataContract.GetCalendarTaskParameters
                    {
                        Token = Token,
                        FolderId = folderId,
                        CalendarTaskId = objectId
                    });

                    return result.CalendarTask.Convert();
                }
                if (notification.ObjectType == ObjectType.CalendarAppointment)
                {
                    var result = await AppServiceProxy.GetCalendarAppointmentAsync(new DataContract.GetCalendarAppointmentParameters
                    {
                        Token = Token,
                        FolderId = folderId,
                        CalendarAppointmentId = objectId
                    });

                    return result.CalendarAppointment.Convert();
                }
            }

            if (sourceType == SourceType.Local)
            {
                throw new InvalidSourceTypeException("This action can only be performed when online.");
            }

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task SaveNotification(Notification notification)
        {
            await notificationsDataAccess.SaveNotifications(new List<Notification> { notification });
        }

        public async Task MarkAsRead(Guid notificationGuid)
        {
            await notificationsDataAccess.MarkAsRead(notificationGuid);
        }

        public async Task MarkAsRead(Notification notification)
        {
            await notificationsDataAccess.MarkAsRead(notification);

            notification.IsRead = true;
        }

        public async Task MarkAsRead(List<Notification> notifications)
        {
            await notificationsDataAccess.MarkAllAsRead(notifications, true);

            notifications.ForEach(n => n.IsRead = true);
        }
    }
}

