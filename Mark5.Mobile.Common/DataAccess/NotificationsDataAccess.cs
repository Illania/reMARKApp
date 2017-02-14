//
// Project: Mark5.Mobile.Common
// File: NotificationsDataAccess.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mark5.Mobile.Common.DataAccess.Exceptions;
using Mark5.Mobile.Common.Database;
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.Common.Model;

#pragma warning disable CS1701
namespace Mark5.Mobile.Common.DataAccess
{

    class NotificationsDataAccess : INotificationsDataAccess
    {

        readonly DatabaseConnectionProvider systemDatabase;

        public NotificationsDataAccess(DatabaseConnectionProvider systemDatabase)
        {
            this.systemDatabase = systemDatabase;
        }

        public async Task SaveNotifications(List<Notification> notifications)
        {
            try
            {
                await systemDatabase.RunInConnectionAsync(c =>
                {
                    c.DeleteAll<Notification>();
                    c.InsertAll(notifications);
                });
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error saving notifications.", ex);
            }
        }

        public async Task<List<Notification>> GetNotifications()
        {
            try
            {
                List<Notification> notifications = null;
                List<ReadNotificationInfo> readNotificationInfos = null;

                await systemDatabase.RunInConnectionAsync(c =>
                {
                    notifications = c.Table<Notification>().OrderByDescending(n => n.DateTimeTimestamp).ToList();
                    readNotificationInfos = c.Table<ReadNotificationInfo>().ToList();
                });

                if (notifications != null && readNotificationInfos != null && readNotificationInfos.Count > 0)
                {
                    var guids = readNotificationInfos.Select(rni => rni.NotificationGuid);
                    notifications.ForEach(n => n.IsRead = guids.Contains(n.Guid));
                }

                return notifications;
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error getting notifications.", ex);
            }
        }

        public async Task<List<Guid>> GetReadNotificationGuids()
        {
            try
            {
                List<ReadNotificationInfo> rnis = null;

                await systemDatabase.RunInConnectionAsync(c =>
                {
                    rnis = c.Table<ReadNotificationInfo>().ToList();
                });

                return rnis.Select(rni => rni.NotificationGuid).ToList();
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error getting notifications.", ex);
            }
        }

        public async Task MarkAsRead(Guid notificationGuid)
        {
            try
            {
                await systemDatabase.RunInConnectionAsync(c =>
                {
                    c.InsertOrReplace(new ReadNotificationInfo { NotificationGuid = notificationGuid });
                });
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error getting notifications.", ex);
            }
        }

        public async Task MarkAsRead(Notification notification)
        {
            try
            {
                await systemDatabase.RunInConnectionAsync(c =>
                {
                    c.InsertOrReplace(new ReadNotificationInfo { NotificationGuid = notification.Guid });
                });
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error getting notifications.", ex);
            }
        }

        public async Task MarkAllAsRead(List<Notification> notifications, bool clear)
        {
            try
            {
                await systemDatabase.RunInConnectionAsync(c =>
                {
                    if (clear)
                    {
                        c.DeleteAll<ReadNotificationInfo>();
                    }

                    c.InsertOrReplaceAll(notifications.Select(n => new ReadNotificationInfo { NotificationGuid = n.Guid }));
                });
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error getting notifications.", ex);
            }
        }
    }
}

