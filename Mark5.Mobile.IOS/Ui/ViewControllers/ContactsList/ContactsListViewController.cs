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

            if (ServerConfig.SystemSettings.ContactsModuleInfo.Permissions.CreateAllowed)
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

        async void RightButton_Clicked(object sender, EventArgs e)
        {
            var choice = await Dialogs.ShowListActionSheetAsync(this, new[] { Localization.GetString("add_company"), Localization.GetString("add_department"), Localization.GetString("add_person") }, RightButton);
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