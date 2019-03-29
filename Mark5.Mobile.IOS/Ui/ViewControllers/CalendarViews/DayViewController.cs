using System;
using UIKit;
using Syncfusion.SfSchedule.iOS;
using Mark5.Mobile.IOS.Ui.Common;
using Foundation;
using System.Collections.Generic;
using CoreGraphics;
using Syncfusion.SfCalendar.iOS;
using Xamarin.Forms.Internals;
using Mono;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.CalendarViews
{
    public class DayViewController : UIViewController
    {
        public DayViewController()
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            ReMarkDayView reMarkDayCalendar = new ReMarkDayView()
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


            InitializeNavigationBar();
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

            NavigationItem.SetRightBarButtonItem(addBtn, true);
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
        }
    }
}
