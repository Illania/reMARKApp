using System;
using Syncfusion.SfCalendar.iOS;
using UIKit;
using Mark5.Mobile.IOS.Ui.Common;
using TelerikUI;
using Syncfusion.SfSchedule.iOS;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.CalendarViews
{
    public class YearViewController : UIViewController
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            ReMarkYearCalendar reMarkYearCalendar = new ReMarkYearCalendar()
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            reMarkYearCalendar.SelectionChanged += ReMarkYearCalendar_SelectionChanged;

            reMarkYearCalendar.CalendarTapped += ReMarkYearCalendar_CalendarTapped;

            reMarkYearCalendar.SelectionChanged += ReMarkYearCalendar_SelectionChanged;

            reMarkYearCalendar.ViewModeChanged += ReMarkYearCalendar_ViewModeChanged;

            reMarkYearCalendar.NavigateToMonthOnInActiveDatesSelection = false;

            reMarkYearCalendar.DateCellHolding += ReMarkYearCalendar_DateCellHolding;

            reMarkYearCalendar.ViewModeChanged += ReMarkYearCalendar_ViewModeChanged;

            reMarkYearCalendar.YearViewSettings.PropertyChanged += YearViewSettings_PropertyChanged;

            View.BackgroundColor = Theme.White;

            View.AddSubview(reMarkYearCalendar);

            View.AddConstraints(new NSLayoutConstraint[] {
                reMarkYearCalendar.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                reMarkYearCalendar.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor),
                reMarkYearCalendar.RightAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.RightAnchor, -10),
                reMarkYearCalendar.LeftAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.LeftAnchor, 10)
            });
        }

        void YearViewSettings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var x = 10;
        }

        void ReMarkYearCalendar_ViewModeChanged1(object sender, ViewModeChangedEventArgs e)
        {
            NavigationController.DismissViewController(true, null);
        }


        void ReMarkYearCalendar_DateCellHolding(object sender, DateCellHoldingEventArgs e)
        {
            var x = 10;
            NavigationController.DismissViewController(true, null);
        }


        void ReMarkYearCalendar_ViewModeChanged(object sender, ViewModeChangedEventArgs e)
        {
            var x = 10;
            NavigationController.DismissViewController(true, null);
            NavigationController.DismissViewController(true, null);
        }


        void ReMarkYearCalendar_SelectionChanged1(object sender, SelectionChangedEventArgs e)
        {
            var x = 10;
            NavigationController.DismissViewController(true, null);
        }


        void ReMarkYearCalendar_CalendarTapped(object sender, CalendarTappedEventArgs e)
        {
            var x = 10;
            NavigationController.DismissViewController(true, null);
        }

        void ReMarkYearCalendar_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var x = 10;
            NavigationController.DismissViewController(true, null);
        }

    }

    public class ReMarkYearCalendar : SFCalendar
    {
        public ReMarkYearCalendar()
        {
            ViewMode = SFCalendarViewMode.SFCalendarViewModeYear;
            YearViewSettings.HeaderLabelAlignment = NSTextAlignment.NSTextAlignmentCenter;
            YearViewSettings.DateTextColor = Theme.DarkerBlue;
            YearViewSettings.YearHeaderTextColor = Theme.DarkerBlue;
            YearViewSettings.MonthLayoutPadding = 20;
            MonthViewSettings.HeaderTextColor = Theme.DarkerBlue;
            YearViewSettings.MonthHeaderTextColor = Theme.DarkerBlue;
            ShowNavigationButtons = true;
            ShowYearView = true;
            DrawMonthCell += YearCalendar_DrawMonthCell;

            this.CalendarTapped += Handle_CalendarTapped;

        }

        void Handle_CalendarTapped(object sender, CalendarTappedEventArgs e)
        {
            var x = 10;
        }

        void YearCalendar_DrawMonthCell(object sender, DrawMonthCellEventArgs e)
        {
            if (e.MonthCell.IsCurrentMonth)
            {
                e.MonthCell.FontAttribute = Theme.DefaultLightFont;
                e.MonthCell.BackgroundColor = Theme.DarkerBlue;
                e.MonthCell.TextColor = UIColor.White;
            }
        }
    }
}
