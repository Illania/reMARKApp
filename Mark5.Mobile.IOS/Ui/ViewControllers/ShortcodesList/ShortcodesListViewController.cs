using System.IO;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.IOS.Ui.Common;
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

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            
            ReachabilityBar.Attach(this);
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            ReachabilityBar.Detach(this);
        }

        protected override void InitializeNavigationBar()
        {
            base.InitializeNavigationBar();

            if (ServerConfig.SystemSettings.ShortcodesModuleInfo.Permissions.CreateAllowed)
            {
                RightButton = new UIBarButtonItem
                {
                    Image = UIImage.FromBundle(Path.Combine("icons", "add_action.png"))
                };
                NavigationItem.SetRightBarButtonItem(RightButton, false);
            }
        }

        protected override void InitializeHandlers()
        {
            base.InitializeHandlers();

            if (RightButton != null)
                RightButton.Clicked += RightButton_Clicked;
        }

        protected override void DeinitializeHandlers()
        {
            base.DeinitializeHandlers();

            if (RightButton != null)
                RightButton.Clicked -= RightButton_Clicked;
        }

        void RightButton_Clicked(object sender, System.EventArgs e)
        {
            var vc = new AddEditShortcodeViewController
            {
                CreationModeFlag = ShortcodeCreationModeFlag.New,
            };

            PresentViewController(new NavigationController(vc), true, null);
        }

        public override void ShortcodeSelected(UITableView tableView, NSIndexPath indexPath, ShortcodePreview shortcodePreview)
        {
            if (SplitViewController != null && !SplitViewController.Collapsed)
            {
                var nc = (UINavigationController)SplitViewController.ViewControllers[1];
                nc.PopToRootViewController(false);

                var vc = (ShortcodeViewController)nc.ViewControllers[0];

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
