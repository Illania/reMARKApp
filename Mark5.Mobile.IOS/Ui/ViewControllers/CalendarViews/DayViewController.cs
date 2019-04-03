using UIKit;
using Syncfusion.SfSchedule.iOS;
using Mark5.Mobile.IOS.Ui.Common;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.CalendarViews
{
    public class DayViewController : UIViewController
    {
        readonly HeaderStyle workWeekHeaderStyle = new HeaderStyle()
        {
            BackgroundColor = Theme.DarkerBlue,
            TextStyle = Theme.DefaultLightFont,
            TextColor = Theme.White
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

        ReMarkDayView reMarkDayCalendar;
        UIBarButtonItem scheduleSwitchBtn;

        public DayViewController()
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            reMarkDayCalendar = new ReMarkDayView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            View.BackgroundColor = Theme.White;

            View.AddSubview(reMarkDayCalendar);

            View.AddConstraints(new NSLayoutConstraint[] {
                reMarkDayCalendar.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                reMarkDayCalendar.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor),
                reMarkDayCalendar.RightAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.RightAnchor),
                reMarkDayCalendar.LeftAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.LeftAnchor)
            });

            reMarkDayCalendar.CellLongPressed += ReMarkDayCalendar_CellTapped;
            reMarkDayCalendar.CellDoubleTapped += ReMarkDayCalendar_CellTapped;

            InitializeNavigationBar();
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
                Title = reMarkDayCalendar.ScheduleView == SFScheduleView.SFScheduleViewWeek ? "Day" : "Week"
            };

            scheduleSwitchBtn.Clicked += (sender, e) =>
            {
                reMarkDayCalendar.HeaderStyle = workWeekHeaderStyle;

                reMarkDayCalendar.DayHeaderStyle = workWeekDayHeader;

                reMarkDayCalendar.WeekViewSettings = workWeekSettings;

                if (reMarkDayCalendar.ScheduleView == SFScheduleView.SFScheduleViewWorkWeek)
                {
                    scheduleSwitchBtn.Title = "Week";
                    reMarkDayCalendar.ScheduleView = SFScheduleView.SFScheduleViewDay;
                }
                else
                {
                    scheduleSwitchBtn.Title = "Day";
                    reMarkDayCalendar.ScheduleView = SFScheduleView.SFScheduleViewWorkWeek;
                }
            };

            NavigationItem.SetRightBarButtonItems(new UIBarButtonItem[] { addBtn, scheduleSwitchBtn }, false);
        }
    }

    class ReMarkDayView : SFSchedule
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

        public ReMarkDayView()
        {
            ScheduleView = SFScheduleView.SFScheduleViewDay;
            HeaderStyle = headerStyle;
            DayViewSettings = dayViewSettings;
            DayHeaderStyle = viewHeaderStyle;
            AppointmentMapping = CalendarUtils.GetAppointmentMapping();
            ItemsSource = CalendarUtils.GetMeetings();

            AppointmentStyle = new SFAppointmentStyle()
            {
                TextStyle = UIFont.FromName("Avenir-Light", 14),
                SelectionBorderColor = Theme.DarkerBlue
            };
        }
    }
}
