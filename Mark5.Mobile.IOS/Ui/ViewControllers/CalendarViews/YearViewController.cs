using Syncfusion.SfCalendar.iOS;
using UIKit;
using Mark5.Mobile.IOS.Ui.Common;
using CoreAnimation;
using System;
using Mark5.Mobile.IOS.Utilities;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.CalendarViews
{
    public class YearViewController : UIViewController
    {
        ReMarkYearCalendar reMarkYearCalendar;
        public bool transitioning;

        Action<Foundation.NSDate> MoveToDate;

        public YearViewController(Action<Foundation.NSDate> moveToDateAction)
        {
            this.MoveToDate = moveToDateAction;
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
        }

        void ReMarkYearCalendar_ViewModeChanged(object sender, ViewModeChangedEventArgs e)
        {
            if (reMarkYearCalendar.ViewMode == SFCalendarViewMode.SFCalendarViewModeMonth)
            {
                if (transitioning)
                    return;

                CATransition transition = new CATransition
                {
                    Duration = 0.30,
                    TimingFunction = CAMediaTimingFunction.FromName(CAMediaTimingFunction.Default),
                    Type = CAAnimation.TransitionPush,
                    Subtype = CAAnimation.TransitionFromRight,
                    Delegate = new AnimationDelegate(this)
                };

                transitioning = true;
                NavigationController.View.Layer.AddAnimation(transition, null);
                MoveToDate.Invoke(e.Date);
                NavigationController.PopViewController(false);
            }
        }

        class AnimationDelegate : CAAnimationDelegate
        {
            readonly YearViewController yearViewController;

            public AnimationDelegate(YearViewController yearViewController)
            {
                this.yearViewController = yearViewController;
            }

            public override void AnimationStopped(CAAnimation anim, bool finished)
            {
                yearViewController.transitioning = false;
            }
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
