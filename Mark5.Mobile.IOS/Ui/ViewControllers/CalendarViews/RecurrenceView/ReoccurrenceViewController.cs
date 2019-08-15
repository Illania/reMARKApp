using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Presenters.CalendarModule;
using Mark5.Mobile.IOS.Ui.Common;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.CalendarViews.RecurrenceView
{
    public class RecurrenceViewController : AbstractViewController
    {
        //TODO after the merge, the addEditAppointmentPresenter, needs to give default values to all parameters of recurring info
        //And need to find a way to remove the recurring part

        PatternView patternView;
        RangeView rangeView;

        EditAppointmentViewModel ap = new EditAppointmentViewModel();

        public override void LoadView()
        {
            base.LoadView();

            InitializeView();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            patternView.SetViewModel(ap);
            patternView.Refresh();

            rangeView.SetViewModel(ap);
            rangeView.Refresh();
        }

        void InitializeView()
        {
            ap.RecurrenceInfo = new RecurrenceInfo();
            ap.RecurrenceInfo.Type = RecurrenceType.Weekly;
            ap.RecurrenceInfo.WeekDays = WeekDays.WorkDays;

            NavigationItem.Title = "Custom recurrence"; //TODO remove support for iOS 10
            View.BackgroundColor = UIColor.GroupTableViewBackgroundColor;

            UIScrollView scrollView = new UIScrollView
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                BackgroundColor = UIColor.White,
                ShowsVerticalScrollIndicator = false,
                ShowsHorizontalScrollIndicator = false,
            };

            View.AddSubview(scrollView);

            View.AddConstraints(new[]
            {
                scrollView.TopAnchor.ConstraintEqualTo(View.TopAnchor),
                scrollView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor),
                scrollView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                scrollView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor)
            });

            patternView = new PatternView();
            patternView.BackgroundColor = UIColor.Blue;
            rangeView = new RangeView();

            scrollView.AddSubview(patternView);
            scrollView.AddSubview(rangeView);

            var paddingValue = 20f;

            scrollView.AddConstraints(new[]
            {
                    patternView.LeadingAnchor.ConstraintEqualTo(scrollView.ReadableContentGuide.LeadingAnchor, paddingValue),
                    patternView.TopAnchor.ConstraintEqualTo(scrollView.ReadableContentGuide.TopAnchor, paddingValue),
                    patternView.RightAnchor.ConstraintEqualTo(scrollView.ReadableContentGuide.RightAnchor, -paddingValue),

                    rangeView.TopAnchor.ConstraintEqualTo(patternView.BottomAnchor, 10f),
                    rangeView.LeftAnchor.ConstraintEqualTo(patternView.LeftAnchor),
                    rangeView.RightAnchor.ConstraintEqualTo(patternView.RightAnchor),
                    //rangeView.BottomAnchor.ConstraintEqualTo(scrollView.ReadableContentGuide.BottomAnchor, -paddingValue),
            });

            var gestureRecognizer = new UITapGestureRecognizer(() => View.EndEditing(true));
            //View.AddGestureRecognizer(gestureRecognizer);  //TODO testing
        }
    }
}
