using System;
using Foundation;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities.Extensions;
using Syncfusion.SfCalendar.iOS;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.CalendarViews
{
    public class YearViewController : UIViewController
    {
        ReMarkYearCalendar reMarkYearCalendar;
        ICalendarCoordinator coordinator;
        NSDate initialDate;

        public YearViewController(CalendarModuleCoordinator calendarCoordinator, NSDate date)
        {
            coordinator = calendarCoordinator;
            initialDate = date;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            reMarkYearCalendar = new ReMarkYearCalendar
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            View.BackgroundColor = UIColor.White;

            reMarkYearCalendar.NavigateToMonthOnInActiveDatesSelection = false;

            reMarkYearCalendar.ViewModeChanged += ReMarkYearCalendar_ViewModeChanged;

            View.AddSubview(reMarkYearCalendar);

            View.AddConstraints(new NSLayoutConstraint[] {
                reMarkYearCalendar.TopAnchor.ConstraintEqualTo( View.SafeAreaLayoutGuide.TopAnchor),
                reMarkYearCalendar.BottomAnchor.ConstraintEqualTo( View.SafeAreaLayoutGuide.BottomAnchor),
                reMarkYearCalendar.RightAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.RightAnchor, -10),
                reMarkYearCalendar.LeftAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.LeftAnchor, 10)
            });

            NavigationController.NavigationBarHidden = true;
        }

        public override void ViewDidAppear(bool animated)
        {
            reMarkYearCalendar.MoveToDate(initialDate);
        }

        void ReMarkYearCalendar_ViewModeChanged(object sender, ViewModeChangedEventArgs e)
        {
            if (reMarkYearCalendar.ViewMode == SFCalendarViewMode.SFCalendarViewModeMonth)
                coordinator.MonthTapped(e.Date.AddSeconds(12 * 60 * 60)); //Adding 12 hours to be sure we get into the first day of the month;
        }
    }

    public class ReMarkYearCalendar : SFCalendar
    {

        readonly SFYearViewSettings yearViewSettings = new SFYearViewSettings
        {
            HeaderLabelAlignment = NSTextAlignment.NSTextAlignmentCenter,
            YearHeaderTextColor = Theme.DarkerBlue,
            MonthHeaderTextColor = Theme.DarkerBlue,
            DateTextColor = Theme.DarkerBlue,
            MonthLayoutPadding = 15
        };

        readonly SFMonthViewSettings monthViewSettings = new SFMonthViewSettings()
        {
            DayHeight = 0,
            TodaySelectionTextColor = Theme.DarkGray,
            CurrentMonthTextColor = Theme.DarkGray,
            CurrentMonthBackgroundColor = Theme.DarkGray
        };

        public ReMarkYearCalendar()
        {
            ViewMode = SFCalendarViewMode.SFCalendarViewModeYear;
            YearViewMode = YearViewMode.Date;
            ShowYearView = true;
            MonthViewSettings = monthViewSettings;
            YearViewSettings = yearViewSettings;
            DrawYearCell += ReMarkYearCalendar_DrawYearCell;
            MinDate = new DateTime(2010, 1, 1).ToNSDate(DateTimeKind.Local);
        }

        void ReMarkYearCalendar_DrawYearCell(object sender, DrawYearCellEventArgs e)
        {
            e.YearCell = new SFYearCell
            {
                FontAttribute = Theme.DefaultLightFont,
                MonthBackgroundColor = UIColor.White,
                DateTextColor = UIColor.White
            };
        }
    }
}
