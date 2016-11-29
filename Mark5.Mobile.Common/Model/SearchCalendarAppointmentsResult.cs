//
// Project: Mark5.Mobile.Common
// File: SearchCalendarAppointmentsResult.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System.Collections.Generic;
using Mark5.Mobile.Common.Model;

#pragma warning disable CS1701
namespace Mark5.Mobile.Common
{

    public class SearchCalendarAppointmentsResult
    {

        public int SearchId { get; set; } = -1;

        public List<CalendarAppointment> CalendarAppointments { get; set; } = new List<CalendarAppointment>();
    }
}

