using Foundation;
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.ViewControllers.FoldersList;
using Mark5.Mobile.IOS.Utilities;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{
    public class FoldersNotificationsListViewController : AbstractMultiViewController, IUIViewControllerRestoration
    {
        readonly ModuleType moduleType;

        public FoldersNotificationsListViewController(ModuleType moduleType)
        {
            this.moduleType = moduleType;
        }

        public override void LoadView()
        {
            base.LoadView();

            NavigationItem.Title = GetTitleForModule(moduleType);

            SegmentedControl.InsertSegment(Localization.GetString("folders"), 0, false);
            SegmentedControl.InsertSegment(Localization.GetString("notifications"), 1, false);

            ViewControllers = new UIViewController[]
            {
                 new BrowseFoldersListViewController(moduleType),
                 new NotificationsListViewController(moduleType.ObjectTypes())
            };
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            RestorationIdentifier = nameof(FoldersNotificationsListViewController);
            RestorationClass = Class;
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            if (Integration.IsRunningAtLeast(11))
            {
                if (NavigationController != null)
                    NavigationController.NavigationBar.PrefersLargeTitles = true;
                NavigationItem.LargeTitleDisplayMode = UINavigationItemLargeTitleDisplayMode.Automatic;
            }
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            if (Integration.IsRunningAtLeast(11))
            {
                NSOperationQueue.MainQueue.AddOperation(() =>
                {
                    var ni = NavigationItem;

                    if (ParentViewController != null && ParentViewController is UIViewController && !(ParentViewController is UINavigationController))
                        ni = ParentViewController?.NavigationItem;

                    if (ni.SearchController == null)
                        ni.SearchController = CurrentViewController?.NavigationItem?.SearchController;
                });
            }
        }

        static string GetTitleForModule(ModuleType moduleType)
        {
            switch (moduleType)
            {
                case ModuleType.Documents:
                    return Localization.GetString("documents");
                case ModuleType.Contacts:
                    return Localization.GetString("contacts");
                case ModuleType.Shortcodes:
                    return Localization.GetString("shortcodes");
                case ModuleType.Calendar:
                    return Localization.GetString("contacts");
                default:
                    return string.Empty;
            }
        }

        #region State restoration

        public override void EncodeRestorableState(NSCoder coder)
        {
            base.EncodeRestorableState(coder);
            coder.Encode((int)moduleType, "moduleType");
            coder.Encode(SegmentedControl.SelectedSegment, "selectedSegment");
        }

        public override void DecodeRestorableState(NSCoder coder)
        {
            base.DecodeRestorableState(coder);
            SegmentedControl.SelectedSegment = coder.DecodeInt("selectedSegment");
        }

        [Export("viewControllerWithRestorationIdentifierPath:coder:")]
        public static UIViewController Restore(string[] identifierComponents, NSCoder coder)
        {
            var moduleType = (ModuleType)coder.DecodeInt("moduleType");
            return new FoldersNotificationsListViewController(moduleType);
        }

        #endregion

    }
}