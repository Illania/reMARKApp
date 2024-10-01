using Foundation;
using reMark.Mobile.Common.Model;
using reMark.Mobile.Common.Utilities;
using reMark.Mobile.IOS.Ui.ViewControllers.ContactsList;
using reMark.Mobile.IOS.Ui.ViewControllers.ShortcodesList;
using UIKit;
using reMark.Mobile.IOS.Ui.Common;
using System.Threading.Tasks;
using reMark.Mobile.IOS.Utilities;

namespace reMark.Mobile.IOS.Ui.ViewControllers.FoldersList
{
    public class BrowseFoldersListViewController : AbstractFoldersListViewController, IUIViewControllerRestoration
    {

        protected UIBarButtonItem NotificationsBarButtonItem;
        public BrowseFoldersListViewController(ModuleType module)
            : base(module, false, false, false)
        {
        }

        protected BrowseFoldersListViewController(Folder folder)
            : base(folder, false, false, false)
        {
        }
        
        public override void LoadView()
        {
            base.LoadView();
            InitializeNavigationBar();
        }

        protected void InitializeNavigationBar()
        {
            if (!Integration.IsIPad())
                return;

            NotificationsBarButtonItem = new UIBarButtonItem
            {
                
                Image = UIImage.FromBundle("Notifications"),
                Width = 16, 
                Enabled = true
            };
            NavigationItem.SetRightBarButtonItem( NotificationsBarButtonItem, false);
            NotificationsBarButtonItem.Clicked+= NotificationsBarButtonItemOnClicked;
        }

        private void NotificationsBarButtonItemOnClicked(object? sender, EventArgs e)
        {
            PresentViewController(new NotificationsSplitViewController(), false,null);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            RestorationIdentifier = nameof(BrowseFoldersListViewController);
            RestorationClass = Class;
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            OutgoingWarningBar.Attach(this);
            ReachabilityBar.Attach(this);
            SendStatusBanner.Attach(this);
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);
            OutgoingWarningBar.Detach(this);
            ReachabilityBar.Detach(this);
            SendStatusBanner.Detach(this);
        }

        protected override void FolderSelected(Folder folder, bool isFromFavorite)
        {
            base.FolderSelected(folder, isFromFavorite);

            if (folder.Module == ModuleType.Documents)
            {
                UIViewController vc;
                if (folder.Local)
                    vc = new DocumentsToUploadListViewController();
                else
                    vc = new DocumentsSegmentListViewController()
                    {
                        Folder = folder
                    };

                NavigationController.PushViewController(vc, true);
            }

            if (folder.Module == ModuleType.Contacts)
            {
                var vc = new ContactsListViewController
                {
                    Folder = folder
                };
                NavigationController.PushViewController(vc, true);
            }

            if (folder.Module == ModuleType.Shortcodes)
            {
                var vc = new ShortcodesListViewController
                {
                    Folder = folder
                };
                NavigationController.PushViewController(vc, true);
            }
        }

        protected async override Task FolderExpand(Folder folder)
        {
            await base.FolderExpand(folder);

            var vc = new BrowseFoldersListViewController(folder);
            NavigationController.PushViewController(vc, true);
        }

        #region State restoration

        public override void EncodeRestorableState(NSCoder coder)
        {
            base.EncodeRestorableState(coder);

            coder.Encode(IsRootOfFoldersList, "isRootOfFoldersList");

            if (IsRootOfFoldersList)
                coder.Encode((int)ParentFolder.Module, "moduleType");
            else
                coder.Encode(Serializer.SerializeToByteArray(ParentFolder.ShallowCopy()), "parentFolder");
        }

        [Export("viewControllerWithRestorationIdentifierPath:coder:")]
        public static UIViewController Restore(string[] identifierComponents, NSCoder coder)
        {
            var isRootOfFoldersList = coder.DecodeBool("isRootOfFoldersList");

            if (isRootOfFoldersList)
            {
                var moduleType = (ModuleType)coder.DecodeInt("moduleType");
                return new BrowseFoldersListViewController(moduleType);
            }
            else
            {
                var folder = Serializer.DeserializeFromByteArray<Folder>(coder.DecodeBytes("parentFolder"));
                return new BrowseFoldersListViewController(folder);
            }
        }

        #endregion
    }
}
