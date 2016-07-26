//
// Project: Mark5.Mobile.Common
// File: CalendarManager.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Mark5.Mobile.Common.DataAccess;
using Mark5.Mobile.Common.Model;
using Mark5.ServiceReference.AppService;

namespace Mark5.Mobile.Common.Managers
{
    class CalendarManager : AbstractManager, ICalendarManager
    {

        readonly IFoldersDataAccess foldersDataAccess;
        readonly ICalendarDataAccess calendarDataAccess;

        public CalendarManager(ConnectionInfo connectionInfo, IAppServiceProxy appServiceProxy, ICalendarDataAccess calendarDataAccess)
            : base(connectionInfo, appServiceProxy)
        {
            this.calendarDataAccess = calendarDataAccess;
        }
    }
}

