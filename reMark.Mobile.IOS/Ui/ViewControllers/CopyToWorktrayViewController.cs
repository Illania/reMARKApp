using System.Collections.Generic;
using Foundation;
using reMark.Mobile.Common.Model;
using reMark.Mobile.IOS.Ui.Common;
using reMark.Mobile.IOS.Utilities;
using UIKit;

namespace reMark.Mobile.IOS.Ui.ViewControllers
{
    public class CopyToWorktrayViewController : AbstractMultiViewController, IUIViewControllerRestoration
    {

        public List<IBusinessEntity> BusinessEntities { get; set; }

        public CopyToWorktrayViewController()
        {

        }

        public override void LoadView()
        {
            base.LoadView();

            NavigationItem.Title = Localization.GetString("copy_to_worktray");

            SegmentedControl.InsertSegment(Localization.GetString("users"), 0, false);
            SegmentedControl.InsertSegment(Localization.GetString("departments"), 1, false);

            SegmentedControl.SetTitleTextAttributes(new UIStringAttributes { ForegroundColor = Theme.White }, UIControlState.Selected);
            SegmentedControl.SetTitleTextAttributes(new UIStringAttributes { ForegroundColor = Theme.DarkerBlue }, UIControlState.Normal);

            ViewControllers = new UIViewController[]
            {
                 new CopyToUserWorktrayViewController(){ BusinessEntities = BusinessEntities },
                 new CopyToDepartmentWorktrayViewController() { BusinessEntities = BusinessEntities }
            };
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            RestorationIdentifier = nameof(CopyToUserWorktrayViewController);
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


        #region State restoration

        public override void EncodeRestorableState(NSCoder coder)
        {
            base.EncodeRestorableState(coder);
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