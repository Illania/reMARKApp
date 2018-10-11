using System;
using System.Collections.Generic;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{
    public class OnBoardingViewController : AbstractPageViewController
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            View.BackgroundColor = Theme.LightBlue;

            var pc = UIPageControl.Appearance;
            pc.BackgroundColor = Theme.LightBlue;
            pc.CurrentPageIndicatorTintColor = Theme.DarkBlue;
            pc.PageIndicatorTintColor = Theme.White;

            var content = new List<OnBoardingPageModel>
            {
                new OnBoardingPageModel("What's new", "We have made a few changes in the MARK5 app. Press next to see what has happened", "2.8_1"),
                new OnBoardingPageModel("Category flow", "We have made it easier to assign categories. " +
                                        "Tap the categories you wish to assign from “select categories” and they will appear in the top under “categories added”. " +
                                        "To remove them from “categories added” just tap them again and they will reappear under “select categories”. " +
                                        "Click save when you are done.", "2.8_2"),
                new OnBoardingPageModel("Swipe between emails", "You can now browse through your emails by swiping to the left or to the right.", "2.8_3"),
                new OnBoardingPageModel("Login details are saved", "From now on MARK5 saves your login details when you log out of the app. " +
                                        "This means that you only need to type in your password when you want to login again.","2.8_4"),
                new OnBoardingPageModel("Compatible with iOS 12", "The MARK5 app is compatible with the new version of iOS.", "2.8_5"),
            };

            if (Integration.IsIPad())
                PreferredContentSize = new CoreGraphics.CGSize(UIImage.FromBundle(content[0].ImageName).Size.Width + 60, 750);

            DataSource = new OnBoardingDataSource(this, content);
        }
    }

    public class OnBoardingPageModel
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public string ImageName { get; set; }

        public OnBoardingPageModel(string title, string content, string imageName)
        {
            Title = title;
            Content = content;
            ImageName = imageName;
        }
    }

    class OnBoardingDataSource : UIPageViewControllerDataSource
    {
        readonly List<OnBoardingPageModel> pageModels;
        readonly UIPageViewController pageViewController;

        public OnBoardingDataSource(UIPageViewController pageViewController, List<OnBoardingPageModel> pageModels)
        {
            this.pageModels = pageModels;
            this.pageViewController = pageViewController;

            Initialize();
        }

        void Initialize()
        {
            pageViewController.SetViewControllers(new[] { GetViewControllerForIndex(0) }, UIPageViewControllerNavigationDirection.Forward, true, (finished) => { });
        }

        public override nint GetPresentationCount(UIPageViewController pageViewController)
        {
            return pageModels.Count;
        }

        public override nint GetPresentationIndex(UIPageViewController pageViewController)
        {
            var currentPage = pageViewController.ViewControllers[0] as OnBoardingPageViewController;
            return currentPage.Index;
        }

        public override UIViewController GetNextViewController(UIPageViewController pageViewController, UIViewController referenceViewController)
        {
            var obvc = (OnBoardingPageViewController)referenceViewController;
            return GetViewControllerForIndex(obvc.Index + 1);
        }

        public override UIViewController GetPreviousViewController(UIPageViewController pageViewController, UIViewController referenceViewController)
        {
            var obvc = (OnBoardingPageViewController)referenceViewController;
            return GetViewControllerForIndex(obvc.Index - 1);
        }

        UIViewController GetViewControllerForIndex(int index)
        {
            if (index < 0 || index >= pageModels.Count)
                return null;

            var viewController = new OnBoardingPageViewController(pageModels[index], index, index == pageModels.Count - 1);
            viewController.Close += OnBoardingPageViewController_Close;
            viewController.GoToNext += OnBoardingPageViewController_GoToNext;

            return viewController;
        }

        void OnBoardingPageViewController_GoToNext(object sender, EventArgs e)
        {
            var obvc = (OnBoardingPageViewController)sender;

            if (obvc.Index + 1 == pageModels.Count)
            {
                pageViewController.DismissViewController(true, null);
                return;
            }

            var nextVc = GetViewControllerForIndex(obvc.Index + 1);

            pageViewController.SetViewControllers(new[] { nextVc }, UIPageViewControllerNavigationDirection.Forward, true, (finished) => { });
        }

        void OnBoardingPageViewController_Close(object sender, EventArgs e)
        {
            pageViewController.DismissViewController(true, null);
        }
    }

    class OnBoardingPageViewController : AbstractViewController
    {
        UILabel titleLabel;
        UITextView descriptionTextView;
        UIImageView headlineImage;

        UIButton nextDoneButton;
        UIButton skipButton;

        readonly OnBoardingPageModel pageModel;
        readonly bool isLast;

        readonly public int Index;

        public event EventHandler GoToNext = delegate { };
        public event EventHandler Close = delegate { };

        public OnBoardingPageViewController(OnBoardingPageModel pageModel, int index, bool isLast)
        {
            this.pageModel = pageModel;
            this.isLast = isLast;

            Index = index;
        }

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
            headlineImage.Image = UIImage.FromBundle(pageModel.ImageName);

            titleLabel = new UILabel
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            titleLabel.TextAlignment = UITextAlignment.Center;
            titleLabel.Text = pageModel.Title;
            titleLabel.TextColor = Theme.DarkBlue;
            titleLabel.Font = Theme.DefaultBoldFont.WithSize(22);

            descriptionTextView = new UITextView
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
            };
            descriptionTextView.Text = pageModel.Content;
            descriptionTextView.Font = Theme.DefaultFont.WithSize(16);
            descriptionTextView.TextColor = Theme.DarkBlue;
            descriptionTextView.TextAlignment = UITextAlignment.Justified;
            descriptionTextView.BackgroundColor = UIColor.Clear;

            nextDoneButton = new UIButton(UIButtonType.System)
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                ContentEdgeInsets = new UIEdgeInsets(5, 25f, 5f, 25f)
            };
            nextDoneButton.SetTitle(!isLast ? "NEXT" : "DONE", UIControlState.Normal);
            nextDoneButton.SetTitleColor(UIColor.White, UIControlState.Normal);
            nextDoneButton.TitleLabel.Font = Theme.DefaultBoldFont.WithSize(18);
            nextDoneButton.TitleLabel.TextAlignment = UITextAlignment.Center;
            nextDoneButton.Layer.CornerRadius = 15.5f;
            nextDoneButton.BackgroundColor = Theme.DarkBlue;
            nextDoneButton.TouchUpInside += NextDoneButton_TouchUpInside;

            View.AddSubview(headlineImage);
            View.AddSubview(titleLabel);
            View.AddSubview(descriptionTextView);
            View.AddSubview(nextDoneButton);

            View.AddConstraints(new NSLayoutConstraint[]
            {
                headlineImage.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor, Integration.IsIPad() ? 30: 0),
                headlineImage.CenterXAnchor.ConstraintEqualTo(View.CenterXAnchor),

                titleLabel.TopAnchor.ConstraintEqualTo(headlineImage.BottomAnchor, 10),
                titleLabel.LeftAnchor.ConstraintEqualTo(View.LeftAnchor, 20),
                titleLabel.RightAnchor.ConstraintEqualTo(View.RightAnchor, -20),

                descriptionTextView.TopAnchor.ConstraintEqualTo(titleLabel.BottomAnchor, 10),
                descriptionTextView.LeftAnchor.ConstraintEqualTo(View.LeftAnchor, 20),
                descriptionTextView.RightAnchor.ConstraintEqualTo(View.RightAnchor, -20),

                nextDoneButton.CenterXAnchor.ConstraintEqualTo(View.CenterXAnchor),
                nextDoneButton.TopAnchor.ConstraintEqualTo(descriptionTextView.BottomAnchor),
                nextDoneButton.BottomAnchor.ConstraintEqualTo(View.BottomAnchor),
            });

            if (!isLast)
            {
                skipButton = new UIButton(UIButtonType.System)
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    ContentEdgeInsets = new UIEdgeInsets(5, 10f, 5f, 10f)
                };
                skipButton.SetTitle("Close", UIControlState.Normal);
                skipButton.TitleLabel.Font = Theme.DefaultFont.WithSize(18);
                skipButton.TitleLabel.TextAlignment = UITextAlignment.Center;
                skipButton.TouchUpInside += SkipButton_TouchUpInside;

                View.AddSubview(skipButton);

                View.AddConstraints(new NSLayoutConstraint[]
                {
                    skipButton.TopAnchor.ConstraintEqualTo(nextDoneButton.TopAnchor),
                    skipButton.LeftAnchor.ConstraintEqualTo(descriptionTextView.LeftAnchor),
                    skipButton.BottomAnchor.ConstraintEqualTo(nextDoneButton.BottomAnchor),

                });
            }
        }

        void NextDoneButton_TouchUpInside(object sender, EventArgs e)
        {
            GoToNext(this, EventArgs.Empty);
        }

        void SkipButton_TouchUpInside(object sender, EventArgs e)
        {
            Close(this, EventArgs.Empty);
        }

    }
}

