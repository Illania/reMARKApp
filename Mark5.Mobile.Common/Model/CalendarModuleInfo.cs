using System.Collections.Generic;

namespace Mark5.Mobile.Common.Model
{
    public class CalendarModuleInfo
    {
        List<Calendar> calendars;

        public List<Calendar> Calendars
        {
            get
            {
                if (calendars == null)
                    calendars = new List<Calendar>();
                return calendars;
            }
            set => calendars = value;
        }

        public Permissions Permissions { get; set; }
    }
}