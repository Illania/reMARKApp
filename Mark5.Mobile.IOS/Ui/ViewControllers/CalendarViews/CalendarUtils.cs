using System;
using System.Collections.ObjectModel;
using Foundation;
using Mark5.Mobile.IOS.Ui.Common;
using Syncfusion.SfSchedule.iOS;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.CalendarViews
{
    public static class CalendarUtils
    {
        public class Meeting
        {
            public NSString EventName { get; set; }
            public NSDate From { get; set; }
            public NSDate To { get; set; }
            public UIColor Color { get; set; }
        }

        public static AppointmentMapping GetAppointmentMapping()
        {
            AppointmentMapping mapping = new AppointmentMapping
            {
                Subject = "EventName",
                StartTime = "From",
                EndTime = "To",
                AppointmentBackground = "Color"
            };
            return mapping;
        }

        public static ObservableCollection<Meeting> GetMeetings()
        {
            ObservableCollection<Meeting> meetings = new ObservableCollection<Meeting>();
            NSCalendar calendar = new NSCalendar(NSCalendarType.Gregorian);
            NSDate today = new NSDate();
            NSDateComponents startDateComponents = calendar.Components(NSCalendarUnit.Year |
                                                                       NSCalendarUnit.Month |
                                                                       NSCalendarUnit.Day, today);
            startDateComponents.Hour = 09;
            startDateComponents.Minute = 0;
            startDateComponents.Second = 0;
            NSDateComponents endDateComponents = calendar.Components(NSCalendarUnit.Year |
                                                                     NSCalendarUnit.Month |
                                                                     NSCalendarUnit.Day, today);

            endDateComponents.Hour = 10;
            endDateComponents.Minute = 0;
            endDateComponents.Second = 0;
            NSDate startDate = calendar.DateFromComponents(startDateComponents);
            NSDate endDate = calendar.DateFromComponents(endDateComponents);
            Meeting meeting = new Meeting
            {
                From = startDate,
                To = endDate,
                EventName = (NSString)"Anniversary",
                Color = Theme.Blue
            };
            meetings.Add(meeting);
            Meeting meeting2 = new Meeting
            {
                From = startDate.AddSeconds(3600),
                To = endDate.AddSeconds(3600),
                EventName = (NSString)"Meetings with Tester",
                Color = Theme.LightBrown
            };
            meetings.Add(meeting2);

            Meeting meeting3 = new Meeting
            {
                From = startDate.AddSeconds(7200),
                To = endDate.AddSeconds(7200),
                EventName = (NSString)"Meetings with Tester 2",
                Color = Theme.Brown
            };
            meetings.Add(meeting3);

            Meeting meeting4 = new Meeting
            {
                From = startDate.AddSeconds(7200),
                To = endDate.AddSeconds(7200),
                EventName = (NSString)"Meetings with Tester 3",
                Color = Theme.LightBlue
            };
            meetings.Add(meeting4);

            Meeting meeting5 = new Meeting
            {
                From = startDate.AddSeconds(9200),
                To = endDate.AddSeconds(9200),
                EventName = (NSString)"Meetings with Tester 3",
                Color = Theme.LightGray
            };

            meetings.Add(meeting5);

            return meetings;
        }
    }
}
