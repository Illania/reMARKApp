using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CoreAnimation;
using Foundation;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Presenters.CalendarModule;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.CalendarViews
{
    public class CalendarCoordinator : ICalendarView, ICalendarCoordinator
    {
        CalendarPresenter presenter;
        MonthViewController monthViewController;
        DayWeekViewController dayWeekViewController;

        public NavigationController NavigationController { get; }

        public ObservableCollection<Appointment> Items => UICache.Instance.Items;

        public CalendarCoordinator()
        {
            presenter = new CalendarPresenter();
            presenter.AttachView(this);
            presenter.Start();  //TODO shouldn't be done here (it gets initialized too early...)

            monthViewController = new MonthViewController(this);
            NavigationController = new NavigationController(monthViewController) //TODO this could be done in a better way 
            {
                RestorationIdentifier = "NavigationController_" + nameof(MonthViewController) + "_" + nameof(ModuleType.Calendar)
            };
        }

        public void UpdateAppointments(IEnumerable<AppointmentPreviewViewModel> caViewModels, DateTime start, DateTime end)
        {
            UICache.Instance.CacheAppointments(caViewModels, start, end);
        }

        public void SetCalendars(List<CalendarViewModel> calendars)
        {
            UICache.Instance.SetCalendars(calendars);
        }

        public void ShowLoading()
        {

        }

        public void StopLoading()
        {

        }

        public Task ShowError(Exception ex)
        {
            return Task.CompletedTask;
        }

        public void ShowAppointment(int appointmentId)
        {

        }

        void ICalendarCoordinator.VisibleDatesChanged(NSDate startDate, NSDate endDate)
        {
            var start = ((DateTime)startDate).ToLocalTime();
            var end = ((DateTime)endDate).ToLocalTime();

            presenter.LoadAppointments(start, end);
        }

        public void DateDoubleTapped(NSDate date)
        {
            //NSDateComponents components = new NSDateComponents
            //{
            //    Hour = 8
            //};

            //NSDate date = NSCalendar.CurrentCalendar.DateByAddingComponents(components, e.Date, NSCalendarOptions.None);
            monthViewController.NavigationController.PushViewController(new DayWeekViewController(this, date), true);  //TODO need to understand why the view controller isn't the same
        }

        public void CreateAppointmentClicked()
        {
            NavigationController.PushViewController(new CreateAppointmentViewController(), true);
        }

        public void HeaderTapped()
        {
            NavigationController.PushViewController(new YearViewController(this), true);
        }

        public void MonthTapped(NSDate date)
        {
            CATransition transition = new CATransition
            {
                Duration = 0.30,
                TimingFunction = CAMediaTimingFunction.FromName(CAMediaTimingFunction.Default),
                Type = CAAnimation.TransitionPush,
                Subtype = CAAnimation.TransitionFromRight,
                //Delegate = new AnimationDelegate(this) //TODO let's try it without...
            };

            NavigationController.View.Layer.AddAnimation(transition, null);
            NavigationController.PopViewController(false);
        }

        class UICache  //TODO this doesn't need to be a singleton
        {
            public static UICache Instance { get; } = new UICache();

            public ObservableCollection<Appointment> Items { get; } = new ObservableCollection<Appointment>();

            List<AppointmentPreviewViewModel> AppointmentViewModels { get; } = new List<AppointmentPreviewViewModel>();
            List<CalendarViewModel> CalendarViewModels { get; } = new List<CalendarViewModel>();

            private UICache() { }
            static UICache() { }

            public void CacheAppointments(IEnumerable<AppointmentPreviewViewModel> appointmentViewModels, DateTime start, DateTime end)
            {
                AppointmentViewModels.Where(i => AppointmentIsInPeriod(i, start, end)).ToList().ForEach((obj) => AppointmentViewModels.Remove(obj));  //TODO need to test
                AppointmentViewModels.AddRange(appointmentViewModels);

                new NSString().BeginInvokeOnMainThread(() =>  //TODO this is so bad, but we need to run it on the UI thread
                {
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

    public class Appointment
    {
        public int CalendarId { get; set; }
        public NSString Id { get; set; }
        public NSString Subject { get; set; }
        public NSDate Start { get; set; }
        public NSDate End { get; set; }
        public UIColor Color { get; set; }
    }

    public interface ICalendarCoordinator
    {
        void VisibleDatesChanged(NSDate startDate, NSDate endDate);

        ObservableCollection<Appointment> Items { get; }

        void DateDoubleTapped(NSDate date);
        void MonthTapped(NSDate date);
        void CreateAppointmentClicked();
        void HeaderTapped();
    }
}
