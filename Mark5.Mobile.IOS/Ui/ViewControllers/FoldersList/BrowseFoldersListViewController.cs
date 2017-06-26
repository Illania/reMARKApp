using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.ViewControllers.ContactsList;
using Mark5.Mobile.IOS.Ui.ViewControllers.ShortcodesList;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.FoldersList
{
    public class BrowseFoldersListViewController : AbstractFoldersListViewController
    {
        public BrowseFoldersListViewController(ModuleType module)
            : base(module, false, false, false)
        {
        }

        protected BrowseFoldersListViewController(Folder folder)
            : base(folder, false, false, false)
        {
        }

        protected override void FolderSelected(Folder folder)
        {
            base.FolderSelected(folder);

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
    }
}