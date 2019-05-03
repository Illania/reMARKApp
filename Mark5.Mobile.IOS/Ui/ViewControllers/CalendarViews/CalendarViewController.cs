using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            schedule.AppointmentMapping = GetAppointmentMapping();
            schedule.ItemsSource = UICache.Instance.Items;
        }

        protected void MoveToDate(NSDate date)
        {
            if (schedule != null)
            {
                schedule.MoveToDate(date);
                schedule.SelectedDate = date;
            }
        }

        public class Appointment
        {
            public int CalendarId { get; set; }
            public NSString Id { get; set; }
            public NSString Subject { get; set; }
            public NSDate Start { get; set; }
            public NSDate End { get; set; }
            public UIColor Color { get; set; }
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

        public void UpdateAppointments(IEnumerable<AppointmentPreviewViewModel> caViewModels, DateTime start, DateTime end)
        {
            UICache.Instance.CacheAppointments(caViewModels, start, end);
        }

        public void SetCalendars(List<CalendarViewModel> calendars)
        {
            UICache.Instance.SetCalendars(calendars);
        }

        public abstract void ShowLoading();
        public abstract void StopLoading();
        public abstract Task ShowError(Exception ex);
        public abstract void ShowAppointment(int appointmentId);

        class UICache
        {
            public static UICache Instance { get; } = new UICache();

            public ObservableCollection<Appointment> Items { get; } = new ObservableCollection<Appointment>();

            List<AppointmentPreviewViewModel> AppointmentViewModels { get; } = new List<AppointmentPreviewViewModel>();
            List<CalendarViewModel> CalendarViewModels { get; } = new List<CalendarViewModel>();

            private UICache()
            {
            }

            static UICache()
            {
            }

            public void CacheAppointments(IEnumerable<AppointmentPreviewViewModel> appointmentViewModels, DateTime start, DateTime end)
            {
                AppointmentViewModels.Where(i => AppointmentIsInPeriod(i, start, end)).ToList().ForEach((obj) => AppointmentViewModels.Remove(obj));  //TODO need to test
                AppointmentViewModels.AddRange(appointmentViewModels);

                new NSString().BeginInvokeOnMainThread(() =>  //TODO this is so bad, but we need to run it on the UI thread
                {
                    //TODO in this part here we'd need to take care also of the calendar that have been selected right now
                    Items.Where(i => AppointmentIsInPeriod(i, start, end)).ToList().ForEach((obj) => Items.Remove(obj));  //TODO this can probably be done in a more clever way...
                    foreach (var caViewModel in AppointmentsInSelectedCalendars(appointmentViewModels))
                    {
                        Items.Add(Convert(caViewModel));
                    }
                });
            }

            public void UpdateCalendar()  //TODO need to connect it
            {
                new NSString().BeginInvokeOnMainThread(() =>  //TODO this is so bad, but we need to run it on the UI thread
                {
                    Items.Clear();
                    foreach (var item in AppointmentsInSelectedCalendars(AppointmentViewModels))
                    {
                        Items.Add(Convert(item));
                    }
                });
            }

            public void SetCalendars(List<CalendarViewModel> calendars)
            {
                CalendarViewModels.Clear();
                CalendarViewModels.AddRange(calendars);
            }

            #region Utilities

            IEnumerable<AppointmentPreviewViewModel> AppointmentsInSelectedCalendars(IEnumerable<AppointmentPreviewViewModel> appointmentViewModels)
            {
                var calendarIds = new HashSet<int>(CalendarViewModels.Select(c => c.Id));
                return appointmentViewModels.Where(apvm => calendarIds.Contains(apvm.CalendarId));
            }

            bool AppointmentIsInPeriod(AppointmentPreviewViewModel appointment, DateTime start, DateTime end)
            {
                var appStart = appointment.Start;
                var appEnd = appointment.End;

                return appEnd > start && appStart < end;
            }

            bool AppointmentIsInPeriod(Appointment appointment, DateTime start, DateTime end)  //TODO almost duplication
            {
                var appStart = (DateTime)appointment.Start;
                var appEnd = (DateTime)appointment.End;

                return appEnd > start && appStart < end;
            }

            protected Appointment Convert(AppointmentPreviewViewModel cavm)
            {
                return new Appointment
                {
                    Subject = new NSString(cavm.Subject),
                    Start = (NSDate)DateTime.SpecifyKind(cavm.Start, DateTimeKind.Local),
                    End = (NSDate)DateTime.SpecifyKind(cavm.End, DateTimeKind.Local),
                    Color = UI.UIColorFromHexString(cavm.HexColor),
                    Id = new NSString(cavm.Id.ToString())
                };
            }

            #endregion

        }
    }
}
