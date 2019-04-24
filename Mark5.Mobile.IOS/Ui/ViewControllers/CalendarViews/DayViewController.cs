using UIKit;
using Syncfusion.SfSchedule.iOS;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.CalendarViews
{
    public class DayViewController : UIViewController
    {
        private readonly ReMarkSchedule reMarkSchedule;
        private UIBarButtonItem scheduleSwitchBtn;

        public DayViewController(Foundation.NSDate date)
        {
            reMarkSchedule = new ReMarkSchedule();
            MoveToDate(date);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            View.BackgroundColor = Theme.White;
            View.AddSubview(reMarkSchedule);
            View.AddConstraints(new NSLayoutConstraint[] {
                reMarkSchedule.TopAnchor.ConstraintEqualTo(Integration.IsRunningAtLeast(11) ? View.SafeAreaLayoutGuide.TopAnchor : View.TopAnchor),
                reMarkSchedule.BottomAnchor.ConstraintEqualTo(Integration.IsRunningAtLeast(11) ? View.SafeAreaLayoutGuide.BottomAnchor : View.BottomAnchor),
                reMarkSchedule.RightAnchor.ConstraintEqualTo(Integration.IsRunningAtLeast(11) ? View.SafeAreaLayoutGuide.RightAnchor : View.RightAnchor),
                reMarkSchedule.LeftAnchor.ConstraintEqualTo(Integration.IsRunningAtLeast(11) ? View.SafeAreaLayoutGuide.LeftAnchor : View.LeftAnchor)
            });

            reMarkSchedule.CellLongPressed += ReMarkDayCalendar_CellTapped;
            reMarkSchedule.CellDoubleTapped += ReMarkDayCalendar_CellTapped;

            InitializeNavigationBar();
        }

        public void MoveToDate(Foundation.NSDate date)
        {
            if (reMarkSchedule != null)
            {
                reMarkSchedule.MoveToDate(date);
                reMarkSchedule.SelectedDate = date;
            }
        }

        void ReMarkDayCalendar_CellTapped(object sender, CellTappedEventArgs e)
        {
            if (e != null)
            {
                NavigationController.PushViewController(new AppointmentViewController(), true);
            }
        }

        void InitializeNavigationBar()
        {
            var addBtn = new UIBarButtonItem
            {
                Title = "",
                Image = UIImage.FromBundle("Create")
            };

            addBtn.Clicked += (sender, e) =>
            {
                var newAppointmentVC = new CreateAppointmentViewController();
                NavigationController.PushViewController(newAppointmentVC, true);
            };

            scheduleSwitchBtn = new UIBarButtonItem
            {
                Title = reMarkSchedule.ScheduleView == SFScheduleView.SFScheduleViewWeek ? "Day" : "Week"
            };

            scheduleSwitchBtn.Clicked += (sender, e) =>
            {
                if (reMarkSchedule.ScheduleView == SFScheduleView.SFScheduleViewWorkWeek)
                {
                    scheduleSwitchBtn.Title = "Week";
                    reMarkSchedule.ScheduleView = SFScheduleView.SFScheduleViewDay;
                }
                else
                {
                    scheduleSwitchBtn.Title = "Day";
                    reMarkSchedule.ScheduleView = SFScheduleView.SFScheduleViewWorkWeek;
                }
            };

            NavigationItem.SetRightBarButtonItems(new UIBarButtonItem[] { addBtn, scheduleSwitchBtn }, false);
        }
    }

    class ReMarkSchedule : SFSchedule
    {
        readonly HeaderStyle headerStyle = new HeaderStyle
        {
            BackgroundColor = Theme.DarkerBlue,
            TextColor = UIColor.White,
            TextStyle = Theme.DefaultLightFont,
            TextPosition = UITextAlignment.Center
        };

        readonly SFViewHeaderStyle viewHeaderStyle = new SFViewHeaderStyle
        {
            BackgroundColor = Theme.DarkerBlue,
            DateTextStyle = Theme.DefaultLightFont,
            DateTextColor = UIColor.White,
            CurrentDateTextColor = UIColor.White,
            CurrentDayTextColor = UIColor.White,
            DayTextColor = UIColor.White,
            DayTextStyle = Theme.DefaultLightFont
        };

        readonly DayViewSettings dayViewSettings = new DayViewSettings
        {
            LabelSettings = new DayLabelSettings
            {
                TimeLabelColor = Theme.DarkGray
            }
        };

        readonly SFViewHeaderStyle workWeekDayHeader = new SFViewHeaderStyle()
        {
            DayTextColor = UIColor.White,
            DayTextStyle = Theme.DefaultLightFont,
            DateTextColor = UIColor.White,
            DateTextStyle = Theme.DefaultLightFont,
            BackgroundColor = Theme.DarkerBlue,
            CurrentDayTextColor = UIColor.White
        };

        readonly WeekViewSettings workWeekSettings = new WeekViewSettings()
        {
            LabelSettings = new WeekLabelSettings()
            {
                TimeLabelColor = Theme.DarkGray
            }
        };

        readonly SFAppointmentStyle appointmentStyle = new SFAppointmentStyle()
        {
            TextStyle = UIFont.FromName("Avenir-Light", 14),
            SelectionBorderColor = Theme.DarkerBlue
        };

        public ReMarkSchedule()
        {
            TranslatesAutoresizingMaskIntoConstraints = false;
            ScheduleView = SFScheduleView.SFScheduleViewDay;
            DayHeaderStyle = workWeekDayHeader;
            WeekViewSettings = workWeekSettings;
            HeaderStyle = headerStyle;
            DayViewSettings = dayViewSettings;
            DayHeaderStyle = viewHeaderStyle;
            AppointmentStyle = appointmentStyle;

            AppointmentMapping = CalendarUtils.GetAppointmentMapping();
            ItemsSource = CalendarUtils.GetMeetings();
        }
    }
}
