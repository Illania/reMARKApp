using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Foundation;
using Mark5.Mobile.Common.Presenters.CalendarModule;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities;
using Syncfusion.SfSchedule.iOS;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.CalendarViews
{
    public abstract class CalendarViewController : AbstractViewController, ICalendarView
    {
        protected SFSchedule schedule;
        protected ObservableCollection<Meeting> items = new ObservableCollection<Meeting>();

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            schedule.AppointmentMapping = GetAppointmentMapping();
            schedule.ItemsSource = items;
        }

        protected void MoveToDate(NSDate date)
        {
            if (schedule != null)
            {
                schedule.MoveToDate(date);
                schedule.SelectedDate = date;
            }
        }

        public class Meeting
        {
            public NSString Id { get; set; }
            public NSString Subject { get; set; }
            public NSDate Start { get; set; }
            public NSDate End { get; set; }
            public UIColor Color { get; set; }
        }

        protected Meeting Convert(SimpleCalendarAppointmentViewModel cavm)
        {
            return new Meeting
            {
                Subject = new NSString(cavm.Subject),
                Start = (NSDate)DateTime.SpecifyKind(cavm.Start, DateTimeKind.Local),
                End = (NSDate)DateTime.SpecifyKind(cavm.End, DateTimeKind.Local),
                Color = UI.UIColorFromHexString(cavm.HexColor),
                Id = new NSString(cavm.Id.ToString())
            };
        }

        public static AppointmentMapping GetAppointmentMapping()
        {
            AppointmentMapping mapping = new AppointmentMapping
            {
                Subject = "Subject",
                StartTime = "Start",
                EndTime = "End",
                AppointmentBackground = "Color",
                Notes = "Id",
            };
            return mapping;
        }

        public abstract void SetCalendars(List<CalendarViewModel> calendars);
        public abstract void UpdateAppointments(IEnumerable<SimpleCalendarAppointmentViewModel> caViewModels, DateTime start, DateTime end);
        public abstract void ShowLoading();
        public abstract void StopLoading();
        public abstract Task ShowError(Exception ex);
        public abstract void ShowAppointment(int appointmentId);
    }
}
