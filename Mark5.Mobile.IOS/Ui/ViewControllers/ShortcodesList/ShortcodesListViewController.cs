using Foundation;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.ShortcodesList
{
    public class ShortcodesListViewController : AbstractShortcodesListViewController, IUIViewControllerRestoration
    {
        public ShortcodesListViewController()
            : base(false)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            RestorationIdentifier = nameof(ShortcodesListViewController);
            RestorationClass = Class;
        }

        public override void ShortcodeSelected(UITableView tableView, ShortcodePreview shortcodePreview)
        {
            if (tableView == SearchResultsController.TableView)
            {
                var ds = (DataSource) tableView.Source;
                var indexPath = ds.FindItemIndexPath(shortcodePreview);
                if (indexPath != null)
                    tableView.SelectRow(indexPath, false, UITableViewScrollPosition.Middle);
            }

            if (SplitViewController != null && !SplitViewController.Collapsed)
            {
                var nc = (UINavigationController) SplitViewController.ViewControllers[1];
                nc.PopToRootViewController(false);

                var vc = (ShortcodeViewController) nc.ViewControllers[0];

                if (vc.IsShowingShortcodeWithId(shortcodePreview.Id))
                    return;

                vc.ClearData();
                vc.SetData(Folder, shortcodePreview);
                vc.RefreshData();
            }
            else
            {
                var vc = new ShortcodeViewController();
                vc.SetData(Folder, shortcodePreview);
                vc.SetRefreshDataOnAppear();
                NavigationController.PushViewController(vc, true);
            }
        }

        #region State restoration

        public override void EncodeRestorableState(NSCoder coder)
        {
            base.EncodeRestorableState(coder);
            coder.Encode(Serializer.SerializeToByteArray(Folder.ShallowCopy()), "folder");
        }

        public override void DecodeRestorableState(NSCoder coder)
        {
            base.DecodeRestorableState(coder);
            Folder = Serializer.DeserializeFromByteArray<Folder>(coder.DecodeBytes("folder"));
        }

        [Export("viewControllerWithRestorationIdentifierPath:coder:")]
        public static UIViewController Restore(string[] identifierComponents, NSCoder coder)
        {
            return new ShortcodesListViewController();
        }

        #endregion
    }
}
