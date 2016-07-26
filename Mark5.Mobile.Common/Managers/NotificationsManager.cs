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
using Mark5.Mobile.Common.Model.Converters;
using Mark5.ServiceReference.AppService;
using DataContract = Mark5.ServiceReference.DataContract;
using Mark5.Mobile.Common.Storage;

namespace Mark5.Mobile.Common.Managers
{

    class NotificationsManager : AbstractManager, INotificationsManager
    {

        readonly IFoldersDataAccess foldersDataAccess;
        readonly INotificationsDataAccess notificationsDataAccess;

        public NotificationsManager(ConnectionInfo connectionInfo, IAppServiceProxy appServiceProxy, IFoldersDataAccess foldersDataAccess, INotificationsDataAccess notificationsDataAccess)
            : base(connectionInfo, appServiceProxy)
        {
            this.foldersDataAccess = foldersDataAccess;
            this.notificationsDataAccess = notificationsDataAccess;
        }

        public async Task<List<Notification>> GetNotificationsAsync(DeviceType deviceType, string pushToken, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto || sourceType == SourceType.Remote)
            {
                var result = await AppServiceProxy.GetNotificationsAsync(new DataContract.GetNotificationsParameters
                {
                    Token = Token,
                    DeviceType = deviceType.ConvertEnum<DataContract.DeviceType>(),
                    PushToken = pushToken
                });

                var notifications = result.Notifications.WhereNotNull().Select(n => n.Convert()).ToList();

                await notificationsDataAccess.SaveNotifications(notifications);

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
            if (sourceType == SourceType.Auto || sourceType == SourceType.Remote)
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

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task SetFoldersNotificationsAsync(DeviceType deviceType, string pushToken, ModuleType moduleType, List<Folder> folders, bool enabled, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto || sourceType == SourceType.Remote)
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
                foreach (var favoriteFolder in favoriteFolders[moduleType].Where(f => folderIds.Contains(f.Id)))
                {
                    favoriteFolder.Subscribed = enabled;
                }
                await FileSystemStorage.SaveFavoriteFoldersAsync(favoriteFolders);
            }

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task<bool> GetCalendarNotificationsEnabledAsync(DeviceType deviceType, string pushToken, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto || sourceType == SourceType.Remote)
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
            if (sourceType == SourceType.Auto || sourceType == SourceType.Remote)
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
            }

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task<string> GetNotificationsSoundAsync(DeviceType deviceType, string pushToken, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto || sourceType == SourceType.Remote)
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
            if (sourceType == SourceType.Auto || sourceType == SourceType.Remote)
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
            }

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task ClearAllNotificationSettingsAsync(DeviceType deviceType, string pushToken, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto || sourceType == SourceType.Remote)
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
            }

            throw new ArgumentException("Invalid sourceType provided.");
        }
    }
}

