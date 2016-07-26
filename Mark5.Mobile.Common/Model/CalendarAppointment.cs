//
// Project: Mark5.Mobile.Common
// File: CalendarAppointment.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Collections.Generic;

namespace Mark5.Mobile.Common.Model
{

    public class CalendarAppointment : BusinessEntity
    {

        public override ObjectType ObjectType
        {
            get
            {
                return ObjectType.CalendarAppointment;
            }
        }

        public override ModuleType ModuleType
        {
            get
            {
                return ModuleType.Calendar;
            }
        }

        public string Subject { get; set; }

        public string Location { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public bool AllDay { get; set; }

        public bool Private { get; set; }

        public CalendarCategory Category { get; set; }

        public AppointmentStatus Status { get; set; }

        public int CreatorId { get; set; } = -1;

        public string Creator { get; set; }

        public Priority Priority { get; set; }

        public CalendarOccurenceType Type { get; set; }

        List<CalendarResource> resources;

        public List<CalendarResource> Resources
        {
            get
            {
                if (resources == null)
                {
                    resources = new List<CalendarResource>();
                }

                return resources;
            }
            set
            {
                resources = value;
            }
        }

        public DateTime ReminderDate { get; set; }

        public long SnoozeDelay { get; set; } = -1;

        List<Participant> participants;

        public List<Participant> Participants
        {
            get
            {
                if (participants == null)
                {
                    participants = new List<Participant>();
                }

                return participants;
            }
            set
            {
                participants = value;
            }
        }
    }
}

