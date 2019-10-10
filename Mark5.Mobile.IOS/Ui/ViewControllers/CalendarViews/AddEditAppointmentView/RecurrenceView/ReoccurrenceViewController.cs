using System.Threading.Tasks;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.IOS.Ui.Common;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.CalendarViews.RecurrenceView
{
    public class RecurrenceViewController : AbstractViewController
    {
        readonly TaskCompletionSource<RecurrenceInfo> tcs = new TaskCompletionSource<RecurrenceInfo>();
        public Task<RecurrenceInfo> Result => tcs.Task;

        PatternView patternView;
        RangeView rangeView;

        RecurrenceInfo recInfo;

        UIBarButtonItem cancelButton;
        UIBarButtonItem doneButton;

        private RecurrenceViewController(RecurrenceInfo ri)
        {
            recInfo = Serializer.Deserialize<RecurrenceInfo>(Serializer.Serialize(ri));
        }

        public static RecurrenceViewController Create(RecurrenceInfo ri)
        {
            return new RecurrenceViewController(ri);
        }

        public override void LoadView()
        {
            base.LoadView();

            InitializeView();
            InitializeNavigationBar();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            patternView.SetViewModel(recInfo);
            rangeView.SetViewModel(recInfo);

            patternView.Refresh();
            rangeView.Refresh();
        }

        void InitializeNavigationBar()
        {
            cancelButton = new UIBarButtonItem(UIBarButtonSystemItem.Cancel);
            doneButton = new UIBarButtonItem(UIBarButtonSystemItem.Done);

            cancelButton.Clicked += CancelButton_Clicked;
            doneButton.Clicked += DoneButton_Clicked;

            NavigationItem.LeftBarButtonItem = cancelButton;
            NavigationItem.RightBarButtonItem = doneButton;
        }

        void InitializeView()
        {
            NavigationItem.Title = "Custom recurrence";

            UIScrollView scrollView = new UIScrollView
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                BackgroundColor = UIColor.GroupTableViewBackgroundColor,
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
            });

            var gestureRecognizer = new UITapGestureRecognizer(() => View.EndEditing(true));
            gestureRecognizer.CancelsTouchesInView = false;
            View.AddGestureRecognizer(gestureRecognizer);
        }

        void DoneButton_Clicked(object sender, System.EventArgs e)
        {
            tcs.SetResult(recInfo);
            NavigationController?.PopViewController(true);

        }

        void CancelButton_Clicked(object sender, System.EventArgs e)
        {
            tcs.SetResult(null);
            NavigationController?.PopViewController(true);
        }
    }
}
