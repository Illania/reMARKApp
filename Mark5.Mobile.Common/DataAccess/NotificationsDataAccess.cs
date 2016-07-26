//
// Project: Mark5.Mobile.Common
// File: NotificationsDataAccess.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Database;
using Mark5.Mobile.Common.Model;
using System.Linq;

namespace Mark5.Mobile.Common.DataAccess
{

    class NotificationsDataAccess : INotificationsDataAccess
    {

        readonly DatabaseConnectionProvider commonDatabase;

        public NotificationsDataAccess(DatabaseConnectionProvider commonDatabase)
        {
            this.commonDatabase = commonDatabase;
        }

        public async Task SaveNotifications(List<Notification> notifications)
        {
            await commonDatabase.RunInConnectionAsync(c =>
            {
                c.DeleteAll<Notification>();
                c.InsertAll(notifications);
            });
        }

        public async Task<List<Notification>> GetNotifications()
        {
            List<Notification> notifications = null;

            await commonDatabase.RunInConnectionAsync(c =>
            {
                notifications = c.Table<Notification>().ToList();
            });

            return notifications;
        }
    }
}

