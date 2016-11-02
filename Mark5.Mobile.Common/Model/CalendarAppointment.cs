//
// Project: Mark5.Mobile.Common
// File: CalendarAppointment.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Collections.Generic;
using Mark5.Mobile.Common.Utilities;
using SQLite;

namespace Mark5.Mobile.Common.Model
{

    public class CalendarAppointment : BusinessEntity
    {
        [Ignore]
        public override ObjectType ObjectType
        {
            get
            {
                return ObjectType.CalendarAppointment;
            }
        }

        [Ignore]
        public override ModuleType ModuleType
        {
            get
            {
                return ModuleType.Calendar;
            }
        }

        [Column("Subject")]
        public string Subject { get; set; }

        [Column("Location")]
        public string Location { get; set; }

        [Column("StartDateTimestamp")]
        public long StartDateTimestamp { get; set; } = -1;

        [Column("EndDateTimestamp")]
        public long EndDateTimestamp { get; set; } = -1;

        [Column("AllDay")]
        public bool AllDay { get; set; }

        [Column("Private")]
        public bool Private { get; set; }

        [Ignore]
        public CalendarCategory Category { get; set; }

        [Column("Status")]
        public AppointmentStatus Status { get; set; }

        [Column("CreatorId")]
        public int CreatorId { get; set; } = -1;

        [Column("Creator")]
        public string Creator { get; set; }

        [Column("Priority")]
        public Priority Priority { get; set; }

        [Column("Type")]
        public CalendarOccurenceType Type { get; set; }

        List<CalendarResource> resources;

        [Ignore]
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

        [Column("ReminderDateTimestamp")]
        public long ReminderDateTimestamp { get; set; } = -1;

        [Column("SnoozeDelay")]
        public long SnoozeDelay { get; set; } = -1;

        List<Participant> participants;

        [Ignore]
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

        #region Serialization

        [Column("CategoryBytes")]
        public byte[] CategoryBytes
        {
            get
            {
                return SerializationUtils.SerializeToByteArray(Category);
            }
            set
            {
                Category = SerializationUtils.DeserializeFromByteArray<CalendarCategory>(value);
            }
        }

        [Column("ResourcesBytes")]
        public byte[] ResourcesBytes
        {
            get
            {
                return SerializationUtils.SerializeToByteArray(Resources);
            }
            set
            {
                Resources = SerializationUtils.DeserializeFromByteArray<List<CalendarResource>>(value);
            }
        }

        [Column("ParticipantsBytes")]
        public byte[] ParticipantsBytes
        {
            get
            {
                return SerializationUtils.SerializeToByteArray(Participants);
            }
            set
            {
                Participants = SerializationUtils.DeserializeFromByteArray<List<Participant>>(value);
            }
        }

        #endregion

        public override string ToString()
        {
            return $"[CalendarAppointment: Subject={Subject}, Location={Location}, StartDateTimestamp={StartDateTimestamp}, Status={Status}]";
        }
    }
}

