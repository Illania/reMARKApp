//
// Project: Mark5.Mobile.Common
// File: INotificationsDataAccess.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Model;

#pragma warning disable CS1701
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

