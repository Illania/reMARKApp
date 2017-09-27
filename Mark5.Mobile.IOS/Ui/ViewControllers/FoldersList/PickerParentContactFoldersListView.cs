using System;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.ViewControllers.ContactsList;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.FoldersList
{
    public class PickerParentContactFoldersListView : AbstractFoldersListViewController
    {
        readonly TaskCompletionSource<ContactPreview> tcs = new TaskCompletionSource<ContactPreview>();
        public Task<ContactPreview> Task => tcs.Task;

        readonly ContactType childrenType;
        
        UIBarButtonItem cancelModeItem;

        public PickerParentContactFoldersListView(ContactType type)
            : base(ModuleType.Contacts, true, true, true)
        {
            childrenType = type;
        }

        protected PickerParentContactFoldersListView(Folder folder, ContactType type)
            : base(folder, true, true, true)
        {
            childrenType = type;
        }

        protected async override void FolderSelected(Folder folder)
        {
            var vc = new PickerParentContactListViewController()
            {
                Folder = folder,
                ChildrenType = childrenType,
            };
            NavigationController.PushViewController(vc, true);

            var result = await vc.Task;
            if (result != null)
                tcs.SetResult(result);
        }

        protected override void InitializeNavigationBar()
        {
            if (IsRootOfFoldersList)
                switch (ParentFolder.Module)
                {
                    case ModuleType.Documents:
                        NavigationItem.Title = Localization.GetString("documents");
                        break;
                    case ModuleType.Contacts:
                        NavigationItem.Title = Localization.GetString("contacts");
                        break;
                    case ModuleType.Shortcodes:
                        NavigationItem.Title = Localization.GetString("shortcodes");
                        break;
                    default:
                        NavigationItem.Title = " ";
                        break;
                }
            else
                NavigationItem.Title = ParentFolder.Name;
            
            if (IsRootOfFoldersList)
            {
                cancelModeItem = new UIBarButtonItem();
                cancelModeItem.Title = Localization.GetString("cancel");
                NavigationItem.SetLeftBarButtonItem(cancelModeItem, false);
            }
        }

        protected override void InitializeHandlers()
        {
            base.InitializeHandlers();

            if (cancelModeItem != null)
                cancelModeItem.Clicked += CancelModeItem_Clicked;
        }

        protected override void DeinitializeHandlers()
        {
            base.DeinitializeHandlers();

            if (cancelModeItem != null)
                cancelModeItem.Clicked -= CancelModeItem_Clicked;
        }

        void CancelModeItem_Clicked(object sender, EventArgs e)
        {
            tcs.SetResult(null);
        }

        protected override async void FolderExpand(Folder folder)
        {
            base.FolderExpand(folder);

            var vc = new PickerParentContactFoldersListView(folder, childrenType);
            NavigationController.PushViewController(vc, true);

            var result = await vc.Task;
            if (result != null)
                tcs.SetResult(result);
        }
    }
}
