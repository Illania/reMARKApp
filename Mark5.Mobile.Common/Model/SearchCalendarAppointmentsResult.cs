using System.Collections.Generic;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Common
{
    public class SearchCalendarAppointmentsResult
    {
        public int SearchId { get; set; } = -1;

        public List<CalendarAppointment> CalendarAppointments { get; set; } = new List<CalendarAppointment>();
    }
}

