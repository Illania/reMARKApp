using Syncfusion.SfCalendar.iOS;
using UIKit;
using Mark5.Mobile.IOS.Ui.Common;
using CoreAnimation;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.CalendarViews
{
    public class YearViewController : UIViewController
    {
        ReMarkYearCalendar reMarkYearCalendar;
        bool popped = false;
        public bool transitioning;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            View.BackgroundColor = Theme.DarkerBlue;

            reMarkYearCalendar = new ReMarkYearCalendar()
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            reMarkYearCalendar.ViewModeChanged += ReMarkYearCalendar_ViewModeChanged;

            reMarkYearCalendar.NavigateToMonthOnInActiveDatesSelection = false;

            reMarkYearCalendar.ViewModeChanged += ReMarkYearCalendar_ViewModeChanged;

            View.AddSubview(reMarkYearCalendar);

            View.AddConstraints(new NSLayoutConstraint[] {
                reMarkYearCalendar.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                reMarkYearCalendar.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor),
                reMarkYearCalendar.RightAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.RightAnchor),
                reMarkYearCalendar.LeftAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.LeftAnchor)
            });

            NavigationController.NavigationBarHidden = true;
        }

        void ReMarkYearCalendar_ViewModeChanged(object sender, ViewModeChangedEventArgs e)
        {
            if (reMarkYearCalendar.ViewMode == SFCalendarViewMode.SFCalendarViewModeMonth && !popped)
            {
                if (transitioning)
                    return;

                CATransition transition = new CATransition();
                transition.Duration = 0.30;
                transition.TimingFunction = CAMediaTimingFunction.FromName(CAMediaTimingFunction.Default);
                transition.Type = CAAnimation.TransitionPush;
                transition.Subtype = CAAnimation.TransitionFromRight;
                transition.Delegate = new AnimationDelegate(this);
                transitioning = true;
                NavigationController.View.Layer.AddAnimation(transition, null);
                NavigationController.PopViewController(false);
                popped = !popped;
            }
        }

        class AnimationDelegate : CAAnimationDelegate
        {
            YearViewController yearViewController;

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
        public ReMarkYearCalendar()
        {
            ViewMode = SFCalendarViewMode.SFCalendarViewModeYear;
            YearViewSettings.HeaderLabelAlignment = NSTextAlignment.NSTextAlignmentCenter;
            YearViewSettings.MonthHeaderBackground = Theme.DarkerBlue;
            YearViewSettings.YearHeaderBackground = Theme.DarkerBlue;
            YearViewSettings.MonthLayoutBackground = Theme.DarkerBlue;
            YearViewSettings.YearLayoutBackground = Theme.DarkerBlue;
            YearViewSettings.YearHeaderTextColor = UIColor.White;
            YearViewSettings.MonthHeaderTextColor = UIColor.White;
            YearViewSettings.DateTextColor = UIColor.White;
            YearViewSettings.MonthLayoutPadding = 15;
            DrawYearCell += ReMarkYearCalendar_DrawYearCell;
            YearViewSettings.YearLayoutBackground = Theme.DarkerBlue;
            ShowYearView = true;
        }

        void ReMarkYearCalendar_DrawYearCell(object sender, DrawYearCellEventArgs e)
        {
            SFYearCell yearCell = new SFYearCell();
            yearCell.FontAttribute = Theme.DefaultLightFont;
            yearCell.MonthBackgroundColor = Theme.DarkerBlue;
            yearCell.DateTextColor = UIColor.White;
            e.YearCell = yearCell;
        }
    }
}
