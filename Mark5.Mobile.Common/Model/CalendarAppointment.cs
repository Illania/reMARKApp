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

        [Column("Description")]
        public string Description { get; set; }

        [Column("Location")]
        public string Location { get; set; }

        [Ignore]
        public List<CalendarAppointmentOccurrence> Occurrences { get; set; } = new List<CalendarAppointmentOccurrence>();

        [Column("AllDay")]
        public bool AllDay { get; set; }

        [Column("CreatorId")]
        public int CreatorId { get; set; } = -1;

        [Column("Creator")]
        public string Creator { get; set; }

        [Column("Priority")]
        public Priority Priority { get; set; }  //This is kept for compatibility, later we need to check if it can be removed

        [Column("Type")]
        public CalendarOccurenceType Type { get; set; }

        [Column("CalendarId")]
        public int CalendarId { get; set; } = -1;

        [Column("ReminderAlertTime")]
        public long ReminderAlertTime { get; set; } = -1;

        [Ignore]
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

        [Column("RecurrenceInfoString")]
        public string RecurrenceInfoString { get => Serializer.Serialize(RecurrenceInfo); set => RecurrenceInfo = Serializer.Deserialize<RecurrenceInfo>(value); }

        #endregion

        public override string ToString()
        {
            return $"[CalendarAppointment: Subject={Subject}, Location={Location}]";
        }
    }
}

