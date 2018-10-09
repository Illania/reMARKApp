using System;
using CoreGraphics;
using Foundation;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.ViewControllers.OnBoardingViewControllers;
using Mark5.Mobile.IOS.Utilities;
using UIKit;
using WebKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{
    public class OnBoardingViewController : AbstractPageViewController
    {

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            DataSource = new DataSource();

            var pc = UIPageControl.Appearance;
            pc.BackgroundColor = Theme.LightBlue;

            SetViewControllers(new[] { new OnBoardingModelViewController() }, UIPageViewControllerNavigationDirection.Forward, false, (finished) => { });
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
        }
    }

    class DataSource : UIPageViewControllerDataSource
    {
        public override nint GetPresentationCount(UIPageViewController pageViewController)
        {
            return 5;
        }

        public override nint GetPresentationIndex(UIPageViewController pageViewController)
        {
            return 0;
        }

        public override UIViewController GetNextViewController(UIPageViewController pageViewController, UIViewController referenceViewController)
        {
            return new OnBoardingModelViewController();
        }

        public override UIViewController GetPreviousViewController(UIPageViewController pageViewController, UIViewController referenceViewController)
        {
            return new OnBoardingModelViewController();
        }
    }
}