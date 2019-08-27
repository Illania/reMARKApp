using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.ViewControllers.SearchCriteriaView;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{
    public class SearchCriteriaViewController : AbstractMultiViewController, IUIViewControllerRestoration
    {
        DocumentsSearchCriteriaViewController documentsSearchCriteriaViewController;
        ContactsSearchCriteriaViewController contactsSearchViewController;
        ShortcodesSearchCriteriaViewController shortcodesSearchCriteriaViewController;

        public override void LoadView()
        {
            base.LoadView();

            SegmentedControl.InsertSegment(Localization.GetString("documents"), 0, false);
            SegmentedControl.InsertSegment(Localization.GetString("contacts"), 1, false);
            SegmentedControl.InsertSegment(Localization.GetString("shortcodes"), 2, false);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            RestorationIdentifier = nameof(SearchCriteriaViewController);
            RestorationClass = Class;
        }

        public override void ViewWillAppear(bool animated)
        {
            if (ViewControllers == null || ViewControllers.Length == 0)
            {
                ViewControllers = new UIViewController[]
                {
                    documentsSearchCriteriaViewController ?? new DocumentsSearchCriteriaViewController(),
                    contactsSearchViewController ?? new ContactsSearchCriteriaViewController(),
                    shortcodesSearchCriteriaViewController ?? new ShortcodesSearchCriteriaViewController()
                };
            }

            base.ViewWillAppear(animated);

            if (Integration.IsRunningAtLeast(11))
            {
                if (NavigationController != null)
                    NavigationController.NavigationBar.PrefersLargeTitles = false;
                NavigationItem.LargeTitleDisplayMode = UINavigationItemLargeTitleDisplayMode.Automatic;
            }

            documentsSearchCriteriaViewController = null;
            contactsSearchViewController = null;
            shortcodesSearchCriteriaViewController = null;
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

            if (SegmentedControl != null)
                coder.Encode(SegmentedControl.SelectedSegment, "selectedSegment");

            if (ViewControllers != null && ViewControllers.Length == 3)
            {
                coder.Encode(ViewControllers[0], "vc_0");
                coder.Encode(ViewControllers[1], "vc_1");
                coder.Encode(ViewControllers[2], "vc_2");
            }
        }

        public override void DecodeRestorableState(NSCoder coder)
        {
            base.DecodeRestorableState(coder);

            if (coder.ContainsKey("selectedSegment"))
                SegmentedControl.SelectedSegment = coder.DecodeInt("selectedSegment");

            if (coder.ContainsKey("vc_0"))
                documentsSearchCriteriaViewController = (DocumentsSearchCriteriaViewController)coder.DecodeObject("vc_0");

            if (coder.ContainsKey("vc_1"))
                contactsSearchViewController = (ContactsSearchCriteriaViewController)coder.DecodeObject("vc_1");

            if (coder.ContainsKey("vc_2"))
                shortcodesSearchCriteriaViewController = (ShortcodesSearchCriteriaViewController)coder.DecodeObject("vc_2");
        }

        [Export("viewControllerWithRestorationIdentifierPath:coder:")]
        public static UIViewController Restore(string[] identifierComponents, NSCoder coder)
        {
            return new SearchCriteriaViewController();
        }

        #endregion

    }
}