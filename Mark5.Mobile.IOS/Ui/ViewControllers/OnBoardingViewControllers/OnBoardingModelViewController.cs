using System;
using Mark5.Mobile.IOS.Ui.Common;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.OnBoardingViewControllers
{
    public class OnBoardingModelViewController : AbstractViewController
    {
        UILabel titleLabel;
        UITextView descriptionTextView;
        UIImageView headlineImage;

        UIButton nextDoneButton;
        UIButton skipButton;

        public int index;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            InitializeView();
        }

        void InitializeView()
        {
            View.BackgroundColor = Theme.LightBlue;

            headlineImage = new UIImageView
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            headlineImage.Image = UIImage.FromBundle("OnBoardingOne");

            titleLabel = new UILabel
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            titleLabel.TextAlignment = UITextAlignment.Center;
            titleLabel.Text = "What's new";
            titleLabel.Font = Theme.DefaultBoldFont.WithSize(24);

            descriptionTextView = new UITextView
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
            };
            descriptionTextView.Text = "Wee have made a few changes in the MARK5 App. Press next to see what has happened.";
            descriptionTextView.Font = Theme.DefaultFont.WithSize(18);
            descriptionTextView.TextAlignment = UITextAlignment.Center;
            descriptionTextView.BackgroundColor = UIColor.Clear;

            skipButton = new UIButton(UIButtonType.System)
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                ContentEdgeInsets = new UIEdgeInsets(5, 10f, 5f, 10f)
            };
            skipButton.SetTitle("Skip", UIControlState.Normal);
            skipButton.TitleLabel.Font = Theme.DefaultFont.WithSize(18);
            skipButton.TitleLabel.TextAlignment = UITextAlignment.Center;
            skipButton.SetContentCompressionResistancePriority((int)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);

            nextDoneButton = new UIButton(UIButtonType.System)
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                ContentEdgeInsets = new UIEdgeInsets(5, 25f, 5f, 25f)
            };
            nextDoneButton.SetTitle("NEXT", UIControlState.Normal);
            nextDoneButton.SetTitleColor(UIColor.White, UIControlState.Normal);
            nextDoneButton.TitleLabel.Font = Theme.DefaultBoldFont.WithSize(18);
            nextDoneButton.TitleLabel.TextAlignment = UITextAlignment.Center;
            nextDoneButton.Layer.CornerRadius = 15.5f;
            nextDoneButton.BackgroundColor = Theme.DarkBlue;

            View.AddSubview(headlineImage);
            View.AddSubview(titleLabel);
            View.AddSubview(descriptionTextView);
            View.AddSubview(skipButton);
            View.AddSubview(nextDoneButton);

            View.AddConstraints(new NSLayoutConstraint[]
            {
                headlineImage.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                headlineImage.CenterXAnchor.ConstraintEqualTo(View.CenterXAnchor),

                titleLabel.TopAnchor.ConstraintEqualTo(headlineImage.BottomAnchor, 10),
                titleLabel.LeftAnchor.ConstraintEqualTo(View.LeftAnchor, 20),
                titleLabel.RightAnchor.ConstraintEqualTo(View.RightAnchor, -20),

                descriptionTextView.TopAnchor.ConstraintEqualTo(titleLabel.BottomAnchor, 10),
                descriptionTextView.LeftAnchor.ConstraintEqualTo(View.LeftAnchor, 20),
                descriptionTextView.RightAnchor.ConstraintEqualTo(View.RightAnchor, -20),

                skipButton.TopAnchor.ConstraintEqualTo(descriptionTextView.BottomAnchor),
                skipButton.LeftAnchor.ConstraintEqualTo(descriptionTextView.LeftAnchor),
                skipButton.BottomAnchor.ConstraintEqualTo(View.BottomAnchor),

                nextDoneButton.CenterXAnchor.ConstraintEqualTo(View.CenterXAnchor),
                nextDoneButton.TopAnchor.ConstraintEqualTo(skipButton.TopAnchor),
                nextDoneButton.BottomAnchor.ConstraintEqualTo(skipButton.BottomAnchor),

            });


        }
    }
}
