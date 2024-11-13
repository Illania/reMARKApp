using Foundation;
using reMark.Mobile.Common.Model;
using reMark.Mobile.IOS.Ui.Common;
using reMark.Mobile.IOS.Utilities;
using UIKit;

namespace reMark.Mobile.IOS.Ui.ViewControllers
{
    public class DocumentsSegmentListViewController : AbstractMultiViewController, IUIViewControllerRestoration
    {
        public Folder Folder {get; set;}
        public DocumentsSegmentListViewController() => HasToggleBar = true;

        public override void LoadView()
        {
            base.LoadView();

            SegmentedControl.InsertSegment(Localization.GetString("all"), 0, false);
            SegmentedControl.InsertSegment(Localization.GetString("unread"), 1, false);
            SegmentedControl.Hidden = true;
            NavigationItem.Title = this.Folder?.Name;
          
            ViewControllers = new UIViewController[]
            {
                 new DocumentsListViewController(){Folder = this.Folder},
                 new UnreadDocumentsListViewController(){Folder = this.Folder}
            };
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            RestorationIdentifier = nameof(FoldersNotificationsListViewController);
            RestorationClass = Class;
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            if (!Integration.IsRunningAtLeast(11))
                return;

            NSOperationQueue.MainQueue.AddOperation(() =>
            {
                var ni = NavigationItem;

                if (ParentViewController != null
                    && ParentViewController is UIViewController
                    && !(ParentViewController is UINavigationController))
                {
                    ni = ParentViewController?.NavigationItem;
                }

                if (ni != null)
                    ni.SearchController ??= CurrentViewController.NavigationItem?.SearchController;
            });
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
            return new DocumentsSegmentListViewController();
        }

        #endregion
    }
}
