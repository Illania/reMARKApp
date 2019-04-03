using UIKit;
using Syncfusion.SfSchedule.iOS;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.CalendarViews
{
    public class WeekViewViewController : UIViewController
    {
        public WeekViewViewController()
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            ReMarkWeekViewController reMarkDayCalendar = new ReMarkWeekViewController()
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
        }

        class ReMarkWeekViewController : SFSchedule
        {
            public ReMarkWeekViewController()
            {
                ScheduleView = SFScheduleView.SFScheduleViewWeek;

                HeaderStyle = new HeaderStyle()
                {
                    BackgroundColor = Theme.DarkerBlue,
                    TextStyle = Theme.DefaultLightFont,
                    TextColor = Theme.White
                };

                DayHeaderStyle = new SFViewHeaderStyle()
                {
                    DayTextColor = UIColor.White,
                    DayTextStyle = Theme.DefaultLightFont,
                    DateTextColor = UIColor.White,
                    DateTextStyle = Theme.DefaultLightFont,
                    BackgroundColor = Theme.DarkerBlue,
                    CurrentDayTextColor = UIColor.White
                };

                WeekViewSettings = new WeekViewSettings()
                {
                    LabelSettings = new WeekLabelSettings()
                    {
                        TimeLabelColor = Theme.DarkGray
                    }
                };

                AppointmentMapping = CalendarUtils.GetAppointmentMapping();

                ItemsSource = CalendarUtils.GetMeetings();
            }
        }
    }
}
