using Mark5.Mobile.Common.Model;
using Syncfusion.SfSchedule.iOS;
using UIKit;
using Mark5.Mobile.IOS.Ui.Common;
using CoreAnimation;
using Mark5.Mobile.IOS.Utilities;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.CalendarViews
{
    public class MonthViewController : AbstractViewController
    {
        readonly ReMarkMonthSchedule monthSchedule;
        private bool transitioning;

        public MonthViewController(ModuleType moduleType)
        {
            monthSchedule = new ReMarkMonthSchedule();
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
            MoveToDate(Foundation.NSDate.Now);
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            NavigationController.NavigationBarHidden = false;
        }

        void MoveToDate(Foundation.NSDate date)
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

        void Schedule_MonthInlineAppointmentTapped(object sender, MonthInlineAppointmentTappedEventArgs e)
        {
            NavigationController.PushViewController(new AppointmentViewController(), true);
        }

        void Schedule_CellDoubleTapped(object sender, CellTappedEventArgs e)
        {
            Foundation.NSDateComponents components = new Foundation.NSDateComponents
            {
                Hour = 8
            };

            Foundation.NSDate date = Foundation.NSCalendar.CurrentCalendar.DateByAddingComponents(components, e.Date, Foundation.NSCalendarOptions.None);

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
    }

    class ReMarkMonthSchedule : SFSchedule
    {
        readonly SFViewHeaderStyle dayHeaderStyle = new SFViewHeaderStyle
        {
            BackgroundColor = Theme.DarkerBlue,
            DayTextColor = UIColor.White,
            DayTextStyle = Theme.DefaultLightFont
        };

        readonly HeaderStyle calendarHeaderStyle = new HeaderStyle
        {
            BackgroundColor = Theme.DarkerBlue,
            TextStyle = Theme.DefaultLightFont,
            TextColor = UIColor.White
        };

        readonly MonthViewSettings monthViewSettings = new MonthViewSettings()
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

        readonly SFSelectionStyle selectionStyle = new SFSelectionStyle
        {
            BackgroundColor = Theme.DarkBlue,
            BorderColor = Theme.LightBlue
        };

        public ReMarkMonthSchedule()
        {
            TranslatesAutoresizingMaskIntoConstraints = false;

            ScheduleView = SFScheduleView.SFScheduleViewMonth;
            EnableNavigation = true;
            MonthViewSettings = monthViewSettings;
            SelectionStyle = selectionStyle;
            HeaderStyle = calendarHeaderStyle;
            DayHeaderStyle = dayHeaderStyle;

            MonthCellLoaded += ReMark_MonthCellLoaded;

            AppointmentMapping = CalendarUtils.GetAppointmentMapping();
            ItemsSource = CalendarUtils.GetMeetings();
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
