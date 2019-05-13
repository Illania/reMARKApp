using Foundation;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities;
using Syncfusion.SfCalendar.iOS;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.CalendarViews
{
    public class YearViewController : UIViewController
    {
        ReMarkYearCalendar reMarkYearCalendar;
        ICalendarCoordinator coordinator;
        private NSDate initialDate;

        public YearViewController(CalendarModuleCoordinator calendarCoordinator, NSDate date)
        {
            coordinator = calendarCoordinator;
            initialDate = date;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            reMarkYearCalendar = new ReMarkYearCalendar()
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            View.BackgroundColor = Theme.DarkerBlue;

            reMarkYearCalendar.NavigateToMonthOnInActiveDatesSelection = false;

            reMarkYearCalendar.ViewModeChanged += ReMarkYearCalendar_ViewModeChanged;

            View.AddSubview(reMarkYearCalendar);

            View.AddConstraints(new NSLayoutConstraint[] {
                reMarkYearCalendar.TopAnchor.ConstraintEqualTo(Integration.IsRunningAtLeast(11) ? View.SafeAreaLayoutGuide.TopAnchor : View.TopAnchor),
                reMarkYearCalendar.BottomAnchor.ConstraintEqualTo(Integration.IsRunningAtLeast(11) ? View.SafeAreaLayoutGuide.BottomAnchor : View.BottomAnchor),
                reMarkYearCalendar.RightAnchor.ConstraintEqualTo(Integration.IsRunningAtLeast(11) ? View.SafeAreaLayoutGuide.RightAnchor : View.RightAnchor),
                reMarkYearCalendar.LeftAnchor.ConstraintEqualTo(Integration.IsRunningAtLeast(11) ? View.SafeAreaLayoutGuide.LeftAnchor : View.LeftAnchor)
            });

            NavigationController.NavigationBarHidden = true;

            reMarkYearCalendar.MoveToDate(initialDate);
        }

        void ReMarkYearCalendar_ViewModeChanged(object sender, ViewModeChangedEventArgs e)
        {
            if (reMarkYearCalendar.ViewMode == SFCalendarViewMode.SFCalendarViewModeMonth)
                coordinator.MonthTapped(e.Date);
        }
    }

    public class ReMarkYearCalendar : SFCalendar
    {
        readonly SFYearViewSettings yearViewSettings = new SFYearViewSettings
        {
            HeaderLabelAlignment = NSTextAlignment.NSTextAlignmentCenter,
            MonthHeaderBackground = Theme.DarkerBlue,
            YearHeaderBackground = Theme.DarkerBlue,
            MonthLayoutBackground = Theme.DarkerBlue,
            YearLayoutBackground = Theme.DarkerBlue,
            YearHeaderTextColor = UIColor.White,
            MonthHeaderTextColor = UIColor.White,
            DateTextColor = UIColor.White,
            MonthLayoutPadding = 15
        };

        readonly SFMonthViewSettings monthViewSettings = new SFMonthViewSettings()
        {
            HeaderBackgroundColor = Theme.DarkerBlue,
            CurrentMonthBackgroundColor = Theme.DarkBlue,
            PreviousMonthBackgroundColor = Theme.DarkBlue,
            WeekEndBackgroundColor = Theme.DarkBlue,
            DayLabelBackgroundColor = Theme.DarkBlue,
            DayHeight = 0
        };

        public ReMarkYearCalendar()
        {
            ViewMode = SFCalendarViewMode.SFCalendarViewModeYear;
            BackgroundColor = Theme.DarkerBlue;
            ShowYearView = true;
            MonthViewSettings = monthViewSettings;
            YearViewSettings = yearViewSettings;
            DrawYearCell += ReMarkYearCalendar_DrawYearCell;
        }

        void ReMarkYearCalendar_DrawYearCell(object sender, DrawYearCellEventArgs e)
        {
            e.YearCell = new SFYearCell
            {
                FontAttribute = Theme.DefaultLightFont,
                MonthBackgroundColor = Theme.DarkerBlue,
                DateTextColor = UIColor.White
            };
        }
    }
}
