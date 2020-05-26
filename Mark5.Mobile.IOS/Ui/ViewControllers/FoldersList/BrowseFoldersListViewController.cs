using Foundation;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.IOS.Ui.ViewControllers.ContactsList;
using Mark5.Mobile.IOS.Ui.ViewControllers.ShortcodesList;
using UIKit;
using Mark5.Mobile.IOS.Ui.Common;


namespace Mark5.Mobile.IOS.Ui.ViewControllers.FoldersList
{
    public class BrowseFoldersListViewController : AbstractFoldersListViewController, IUIViewControllerRestoration
    {
        public BrowseFoldersListViewController(ModuleType module)
            : base(module, false, false, false)
        {
        }

        protected BrowseFoldersListViewController(Folder folder)
            : base(folder, false, false, false)
        {
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
                    vc = new DocumentsListViewController
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

        protected override void FolderExpand(Folder folder)
        {
            base.FolderExpand(folder);

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