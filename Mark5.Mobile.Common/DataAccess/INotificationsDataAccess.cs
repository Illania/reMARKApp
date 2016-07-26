//
// Project: Mark5.Mobile.Common
// File: INotificationsDataAccess.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System.Threading.Tasks;
using System.Collections.Generic;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Common.DataAccess
{

    interface INotificationsDataAccess
    {

        Task SaveNotifications(List<Notification> notifications);

        Task<List<Notification>> GetNotifications();
    }
}

