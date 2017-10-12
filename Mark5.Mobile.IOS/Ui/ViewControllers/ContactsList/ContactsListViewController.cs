using System;
using System.IO;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.IOS.Ui.Common;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.ContactsList
{
    public class ContactsListViewController : AbstractContactsListViewController, IUIViewControllerRestoration
    {
        protected UIBarButtonItem CreateContactItem;

        public ContactsListViewController()
            : base(false)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            RestorationIdentifier = nameof(ContactsListViewController);
            RestorationClass = Class;
        }

        protected override void InitializeNavigationBar()
        {
            base.InitializeNavigationBar();

            if (ServerConfig.SystemSettings.ContactsModuleInfo.Permissions.CreateAllowed)
            {
                CreateContactItem = new UIBarButtonItem();
                CreateContactItem.Image = UIImage.FromBundle(Path.Combine("icons", "add_action.png"));
                NavigationItem.SetRightBarButtonItem(CreateContactItem, false);
                RightButton = CreateContactItem;
            }
        }

        protected override void InitializeHandlers()
        {
            base.InitializeHandlers();

            if (CreateContactItem != null)
                CreateContactItem.Clicked += CreateContactItem_Clicked;
        }

        protected override void DeinitializeHandlers()
        {
            base.DeinitializeHandlers();

            if (CreateContactItem != null)
                CreateContactItem.Clicked -= CreateContactItem_Clicked;
        }

        async void CreateContactItem_Clicked(object sender, EventArgs e)
        {
            var choice = await Dialogs.ShowListDialogAsync(this,
                                                           new[] {Localization.GetString("add_company"), Localization.GetString("add_department"), Localization.GetString("add_person") },
                                                           CreateContactItem);
            if (choice < 0)
                return;

            ContactType type = ContactType.None;
            switch (choice)
            {
                case 0:
                    type = ContactType.Company;
                    break;
                case 1:
                    type = ContactType.Department;
                    break;
                case 2:
                    type = ContactType.Person;
                    break;
            }

            var vc = new AddEditContactViewController
            {
                CreationModeFlag = ContactCreationModeFlag.New,
                ContactType = type,
            };
            PresentViewController(new NavigationController(vc), true, null);
        }

        protected override void ContactSelected(UITableView tableView, NSIndexPath indexPath, ContactPreview contactPreview)
        {
            if (SplitViewController != null && !SplitViewController.Collapsed)
            {
                var nc = (UINavigationController)SplitViewController.ViewControllers[1];
                nc.PopToRootViewController(false);

                var vc = (ContactViewController)nc.ViewControllers[0];

                if (vc.IsShowingContactWithId(contactPreview.Id))
                    return;

                vc.ClearData();
                vc.SetData(Folder, contactPreview);
                vc.RefreshData();
            }
            else
            {
                var vc = new ContactViewController();
                vc.SetData(Folder, contactPreview);
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
            return new ContactsListViewController();
        }

        #endregion

    }
}