using System;
using System.Collections.Generic;
using Mark5.Mobile.Common.Utilities;
using SQLite;

namespace Mark5.Mobile.Common.Model
{
    public class CalendarAppointment : BusinessEntity
    {
        [Ignore]
        public override ObjectType ObjectType => ObjectType.CalendarAppointment;

        [Ignore]
        public override ModuleType ModuleType => ModuleType.Calendar;

        [Column("Subject")]
        public string Subject { get; set; }

        [Column("Location")]
        public string Location { get; set; }

        [Ignore] //TODO to fix
        public List<CalendarAppointmentOccurrence> Occurrences { get; set; } = new List<CalendarAppointmentOccurrence>();

        [Column("AllDay")]
        public bool AllDay { get; set; }

        [Column("Private")]
        public bool Private { get; set; }

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

        [Column("CalendarId")]
        public int CalendarId { get; set; } = -1;

        [Column("ReminderAlertTime")]
        public long ReminderAlertTime { get; set; } = -1;

        [Column("ReminderTimeBefore")]
        public long ReminderTimeBefore { get; set; } = -1;

        [Ignore] //TODO testing
        public RecurrenceInfo RecurrenceInfo { get; set; }

        List<Participant> participants;

        [Ignore]
        public List<Participant> Participants
        {
            get
            {
                if (participants == null)
                    participants = new List<Participant>();
                return participants;
            }
            set => participants = value;
        }

        #region Serialization

        [Column("ParticipantsString")]
        public string ParticipantsString { get => Serializer.Serialize(Participants); set => Participants = Serializer.Deserialize<List<Participant>>(value); }

        #endregion

        public override string ToString()
        {
            return $"[CalendarAppointment: Subject={Subject}, Location={Location}, Status={Status}]";
        }
    }

    public class CalendarAppointmentOccurrence
    {
        public long StartDateTimestamp { get; set; } = -1;

        public long EndDateTimestamp { get; set; } = -1;

        public int RecurrenceIndex { get; set; } = -1;
    }
}

