using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CoreAnimation;
using Foundation;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Presenters.CalendarModule;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities;
using Syncfusion.SfSchedule.iOS;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.CalendarViews
{
    public class MonthViewController : AbstractViewController, ICalendarView
    {
        readonly ReMarkMonthSchedule monthSchedule;
        private bool transitioning;
        CalendarPresenter presenter;

        ObservableCollection<Meeting> Items = new ObservableCollection<Meeting>();

        public MonthViewController(ModuleType moduleType)
        {
            monthSchedule = new ReMarkMonthSchedule
            {
                AppointmentMapping = GetAppointmentMapping(),
                ItemsSource = Items
            };
            monthSchedule.HeaderTapped += Handle_HeaderTapped;
            monthSchedule.CellDoubleTapped += Schedule_CellDoubleTapped;
            monthSchedule.MonthInlineAppointmentTapped += Schedule_MonthInlineAppointmentTapped;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            View.AddSubview(monthSchedule);
            View.AddConstraints(new NSLayoutConstraint[] {
                monthSchedule.TopAnchor.ConstraintEqualTo(Integration.IsRunningAtLeast(11) ? View.SafeAreaLayoutGuide.TopAnchor : View.TopAnchor),
                monthSchedule.BottomAnchor.ConstraintEqualTo(Integration.IsRunningAtLeast(11) ? View.SafeAreaLayoutGuide.BottomAnchor : View.BottomAnchor),
                monthSchedule.RightAnchor.ConstraintEqualTo(Integration.IsRunningAtLeast(11) ? View.SafeAreaLayoutGuide.RightAnchor : View.RightAnchor),
                monthSchedule.LeftAnchor.ConstraintEqualTo(Integration.IsRunningAtLeast(11) ? View.SafeAreaLayoutGuide.LeftAnchor : View.LeftAnchor)
            });

            View.BackgroundColor = UIColor.White;
            InitializeNavigationBar();
            MoveToDate(NSDate.Now);

            presenter = new CalendarPresenter();
            presenter.AttachView(this);
            presenter.Start();
        }

        public override async void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            NavigationController.NavigationBarHidden = false;

            var start = new DateTime(2019, 4, 1, 0, 0, 0, DateTimeKind.Local);
            var end = start.AddMonths(1).AddDays(-1);
            await presenter.LoadAppointments(start, end);
        }

        void MoveToDate(NSDate date)
        {
            if (monthSchedule != null)
            {
                monthSchedule.MoveToDate(date);
                monthSchedule.SelectedDate = date;
            }
        }

        void InitializeNavigationBar()
        {
            var backBtn = new UIBarButtonItem
            {
                Title = "Year"
            };

            backBtn.Clicked += (sender, e) =>
            {
                if (transitioning)
                    return;

                YearViewController yearSelection = new YearViewController(MoveToDate);
                CATransition transition = new CATransition
                {
                    Duration = 0.35,
                    TimingFunction = CAMediaTimingFunction.FromName(CAMediaTimingFunction.Linear),
                    Type = CAAnimation.TransitionPush,
                    Subtype = CAAnimation.TransitionFromLeft,
                    Delegate = new AnimationDelegate(this)
                };
                transitioning = true;

                NavigationController.View.Layer.AddAnimation(transition, null);
                NavigationController.PushViewController(yearSelection, false);
            };

            NavigationItem.SetLeftBarButtonItem(backBtn, true);

            var createAppointment = new UIBarButtonItem
            {
                Title = "",
                Image = UIImage.FromBundle("Create")
            };

            var selectCalendars = new UIBarButtonItem
            {
                Title = "Calendars"
            };

            createAppointment.Clicked += (sender, e) =>
            {
                var createAppointmentVC = new CreateAppointmentViewController();
                NavigationController.PushViewController(createAppointmentVC, true);
            };

            NavigationItem.SetRightBarButtonItems(new UIBarButtonItem[] { createAppointment, selectCalendars }, true);
        }

        class AnimationDelegate : CAAnimationDelegate
        {
            readonly MonthViewController ctrl;

            public AnimationDelegate(MonthViewController ctrl)
            {
                this.ctrl = ctrl;
            }

            public override void AnimationStopped(CAAnimation anim, bool finished)
            {
                ctrl.transitioning = false;
            }
        }

        #region ICalendar implementation

        void ICalendarView.SetCalendars(List<CalendarViewModel> calendars)
        {
            //TODO
        }

        void ICalendarView.UpdateAppointments(IEnumerable<SimpleCalendarAppointmentViewModel> caViewModels)
        {
            foreach (var caViewModel in caViewModels)
            {
                Items.Add(Convert(caViewModel));
            }
        }

        Meeting Convert(SimpleCalendarAppointmentViewModel cavm)
        {
            return new Meeting
            {
                Subject = new NSString(cavm.Subject),
                Start = (NSDate)cavm.Start,
                End = (NSDate)cavm.End,
                Color = UI.UIColorFromHexString(cavm.HexColor),
            };
        }

        void ICalendarView.ShowLoading()
        {
            //TODO
        }

        void ICalendarView.StopLoading()
        {
            //TODO
        }

        Task ICalendarView.ShowError()
        {
            return Task.CompletedTask;
            //TODO
        }

        #endregion

        #region Event handlers

        void Schedule_MonthInlineAppointmentTapped(object sender, MonthInlineAppointmentTappedEventArgs e)
        {
            NavigationController.PushViewController(new AppointmentViewController(), true);
        }

        void Schedule_CellDoubleTapped(object sender, CellTappedEventArgs e)
        {
            NSDateComponents components = new NSDateComponents
            {
                Hour = 8
            };

            NSDate date = NSCalendar.CurrentCalendar.DateByAddingComponents(components, e.Date, NSCalendarOptions.None);

            NavigationController.PushViewController(new DayViewController(date), true);
        }

        void Handle_HeaderTapped(object sender, HeaderTappedEventArgs e)
        {
            NavigationController.PushViewController(new YearViewController(MoveToDate), true);
        }

        void Schedule_ViewHeaderTapped(object sender, ViewHeaderTappedEventArgs e)
        {
            NavigationController.PushViewController(new YearViewController(MoveToDate), true);
        }

        #endregion

        public class Meeting
        {
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
                AppointmentBackground = "Color"
            };
            return mapping;
        }
    }

    class ReMarkMonthSchedule : SFSchedule
    {
        public ReMarkMonthSchedule()
        {
            TranslatesAutoresizingMaskIntoConstraints = false;

            ScheduleView = SFScheduleView.SFScheduleViewMonth;
            EnableNavigation = true;

            MonthViewSettings = new MonthViewSettings()
            {
                SelectionIndicatorColor = Theme.LightBlue,
                TodayBackgroundColor = Theme.LightBlue,
                SelectionTextColor = Theme.White,
                ShowAgendaView = true,
                ShowAppointmentsInline = false,
                AgendaViewStyle = new AgendaViewStyle
                {
                    SubjectTextColor = Theme.DarkerBlue,
                    SubjectTextStyle = Theme.DefaultLightFont,
                    TimeTextStyle = Theme.CalendarTimeLightFont,
                    TimeTextColor = Theme.DarkGray,
                    DateTextColor = Theme.DarkGray,
                    DateTextStyle = Theme.DefaultLightFont,
                    HeaderHeight = 50f
                }
            };

            SelectionStyle = new SFSelectionStyle
            {
                BackgroundColor = Theme.DarkBlue,
                BorderColor = Theme.LightBlue
            };

            HeaderStyle = new HeaderStyle
            {
                BackgroundColor = Theme.DarkerBlue,
                TextStyle = Theme.DefaultLightFont,
                TextColor = UIColor.White
            };

            DayHeaderStyle = new SFViewHeaderStyle
            {
                BackgroundColor = Theme.DarkerBlue,
                DayTextColor = UIColor.White,
                DayTextStyle = Theme.DefaultLightFont
            };

            MonthCellLoaded += ReMark_MonthCellLoaded;
        }

        void ReMark_MonthCellLoaded(object sender, MonthCellLoadedEventArgs e)
        {
            SFCellStyle style;

            if (e.IsToday)
            {
                style = new SFCellStyle
                {
                    TextStyle = Theme.DefaultLightFont,
                    BackgroundColor = Theme.DarkerBlue,
                    TextColor = Theme.DarkerBlue
                };
            }
            else
            {
                style = new SFCellStyle
                {
                    TextStyle = Theme.DefaultLightFont,
                    BackgroundColor = Theme.DarkerBlue,
                    TextColor = UIColor.White
                };
            }

            e.CellStyle = style;
        }
    }
}
