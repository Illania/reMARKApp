using System;
namespace Mark5.Mobile.IOS.Model
{
    public class DateTimeChangeEvent
    {
        public DateTime selectedDate;
        public DateRowType rowType;
        public bool allDay;

        public DateTimeChangeEvent(DateRowType rowType, bool allDay)
        {
            this.rowType = rowType;
            this.allDay = allDay;
        }

        public DateTimeChangeEvent(DateTime selectedDate, DateRowType rowType)
        {
            this.selectedDate = selectedDate;
            this.rowType = rowType;
        }

        public enum DateRowType
        {
            Starts,
            Ends,
            AllDay
        }
    }
}
