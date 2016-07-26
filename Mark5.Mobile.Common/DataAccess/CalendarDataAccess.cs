//
// Project: Mark5.Mobile.Common
// File: CalendarDataAccess.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Mark5.Mobile.Common.Database;

namespace Mark5.Mobile.Common.DataAccess
{

    class CalendarDataAccess : ICalendarDataAccess
    {

        readonly DatabaseConnectionProvider calendarDatabase;

        public CalendarDataAccess(DatabaseConnectionProvider calendarDatabase)
        {
            this.calendarDatabase = calendarDatabase;
        }
    }
}

