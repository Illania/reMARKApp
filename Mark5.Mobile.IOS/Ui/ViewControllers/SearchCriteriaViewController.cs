using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.ViewControllers.SearchCriteriaView;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{
    public class SearchCriteriaViewController : AbstractMultiViewController, IUIViewControllerRestoration
    {
        public override void LoadView()
        {
            base.LoadView();

            SegmentedControl.InsertSegment(Localization.GetString("documents"), 0, false);
            SegmentedControl.InsertSegment(Localization.GetString("contacts"), 1, false);
            SegmentedControl.InsertSegment(Localization.GetString("shortcodes"), 2, false);

            ViewControllers = new UIViewController[]
            {
                new DocumentsSearchCriteriaViewController(),
                new ContactsSearchCriteriaViewController(),
                new ShortcodesSearchCriteriaViewController()
            };
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            RestorationIdentifier = nameof(SearchCriteriaViewController);
            RestorationClass = Class;
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            if (NavigationController != null)
                NavigationController.NavigationBar.PrefersLargeTitles = true;
            NavigationItem.LargeTitleDisplayMode = UINavigationItemLargeTitleDisplayMode.Never;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (CommonConfig.Logger.IsDebugEnabled())
                CommonConfig.Logger.Debug("Disposed");
        }

        #region State restoration

        public override void EncodeRestorableState(NSCoder coder)
        {
            base.EncodeRestorableState(coder);
            coder.Encode(SegmentedControl.SelectedSegment, "selectedSegment");
            coder.Encode(ViewControllers[0], "vc_0");
            coder.Encode(ViewControllers[1], "vc_1");
            coder.Encode(ViewControllers[2], "vc_2");
        }

        public override void DecodeRestorableState(NSCoder coder)
        {
            base.DecodeRestorableState(coder);
            SegmentedControl.SelectedSegment = coder.DecodeInt("selectedSegment");
        }

        [Export("viewControllerWithRestorationIdentifierPath:coder:")]
        public static UIViewController Restore(string[] identifierComponents, NSCoder coder)
        {
            return new SearchCriteriaViewController();
        }

        #endregion

    }
}