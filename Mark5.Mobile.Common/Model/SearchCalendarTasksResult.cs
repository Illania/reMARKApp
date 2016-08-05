using System.Collections.Generic;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Common
{
    public class SearchCalendarTasksResult
    {
        public int SearchId { get; set; } = -1;

        public List<CalendarTask> CalendarTasks { get; set; } = new List<CalendarTask>();
    }
}

