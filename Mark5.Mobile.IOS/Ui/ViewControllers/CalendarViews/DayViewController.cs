using System;
using UIKit;
using Syncfusion.SfSchedule.iOS;
using Mark5.Mobile.IOS.Ui.Common;
using Foundation;
using System.Collections.Generic;
using CoreGraphics;

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
                reMarkDayCalendar.RightAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.RightAnchor, -10),
                reMarkDayCalendar.LeftAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.LeftAnchor, 10)
            });
        }


    }

    class ReMarkDayView : SFSchedule
    {
        public ReMarkDayView()
        {
            ScheduleView = SFScheduleView.SFScheduleViewDay;
            SFViewHeaderStyle viewHeaderStyle = new SFViewHeaderStyle();
            BackgroundColor = UIColor.FromRGB(0, 150, 136);
            viewHeaderStyle.DayTextColor = UIColor.FromRGB(255, 255, 255);
            viewHeaderStyle.DateTextColor = UIColor.FromRGB(255, 255, 255);
            viewHeaderStyle.DayTextStyle = UIFont.FromName("Arial", 15);
            viewHeaderStyle.DateTextStyle = UIFont.FromName("Arial", 15);
            DayHeaderStyle = viewHeaderStyle;
        }
    }
}
