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
using Mark5.Mobile.Common.Model;

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

                await systemDatabase.RunInConnectionAsync(c =>
                {
                    notifications = c.Table<Notification>().OrderByDescending(n => n.DateTimeTimestamp).ToList();
                });

                return notifications;
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error getting notifications.", ex);
            }
        }

        public async Task<HashSet<Guid>> GetReadNotificationGuids()
        {
            try
            {
                HashSet<Guid> guids = null;

                await systemDatabase.RunInConnectionAsync(c =>
                {
                    guids = new HashSet<Guid>(c.Table<ReadNotificationInfo>().Select(rni => rni.NotificationGuid));
                });

                return guids;
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

        public async Task MarkAllAsRead()
        {
            try
            {
                await systemDatabase.RunInConnectionAsync(c =>
                {
                    var cmd = c.CreateCommand($"delete from \"{nameof(ReadNotificationInfo)}\"");
                    cmd.ExecuteNonQuery();

                    cmd = c.CreateCommand($"insert into \"{nameof(ReadNotificationInfo)}\" (\"{nameof(ReadNotificationInfo.NotificationGuid)}\") " +
                                          $"select \"{nameof(Notification.Guid)}\" " +
                                          $"from \"{nameof(Notification)}\"");
                    cmd.ExecuteNonQuery();
                });
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error getting notifications.", ex);
            }
        }
    }
}

