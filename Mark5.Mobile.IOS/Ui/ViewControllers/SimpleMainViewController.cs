using System.IO;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.ViewControllers.FoldersList;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{
    public class SimpleMainViewController : AbstractMainViewController
    {
        NavigationController documentsNavigationController;
        NavigationController contactsNavigationController;
        NavigationController shortcodesNavigationController;
        NavigationController settingsNavigationController;

        public override void LoadView()
        {
            base.LoadView();

            documentsNavigationController = new NavigationController(new FoldersNotificationsListViewController(ModuleType.Documents));
            documentsNavigationController.TabBarItem.Title = Localization.GetString("documents");
            documentsNavigationController.TabBarItem.Image = UIImage.FromBundle(Path.Combine("icons", "documents.png"));
            documentsNavigationController.TabBarItem.SelectedImage = UIImage.FromBundle(Path.Combine("icons", "documents-filled.png"));
            documentsNavigationController.Tag = DocumentsTag;
            documentsNavigationController.RestorationIdentifier = "NavigationController_" + nameof(FoldersNotificationsListViewController) + "_" + nameof(ModuleType.Documents);

            contactsNavigationController = new NavigationController(new BrowseFoldersListViewController(ModuleType.Contacts));
            contactsNavigationController.TabBarItem.Title = Localization.GetString("contacts");
            contactsNavigationController.TabBarItem.Image = UIImage.FromBundle(Path.Combine("icons", "contacts.png"));
            contactsNavigationController.TabBarItem.SelectedImage = UIImage.FromBundle(Path.Combine("icons", "contacts-filled.png"));
            contactsNavigationController.Tag = ContactsTag;
            contactsNavigationController.RestorationIdentifier = "NavigationController_" + nameof(BrowseFoldersListViewController) + "_" + nameof(ModuleType.Contacts);

            shortcodesNavigationController = new NavigationController(new BrowseFoldersListViewController(ModuleType.Shortcodes));
            shortcodesNavigationController.TabBarItem.Title = Localization.GetString("shortcodes");
            shortcodesNavigationController.TabBarItem.Image = UIImage.FromBundle(Path.Combine("icons", "shortcodes.png"));
            shortcodesNavigationController.TabBarItem.SelectedImage = UIImage.FromBundle(Path.Combine("icons", "shortcodes-filled.png"));
            shortcodesNavigationController.Tag = ShortcodesTag;
            shortcodesNavigationController.RestorationIdentifier = "NavigationController_" + nameof(BrowseFoldersListViewController) + "_" + nameof(ModuleType.Shortcodes);

            settingsNavigationController = new NavigationController(new SettingsViewController());
            settingsNavigationController.TabBarItem.Title = Localization.GetString("settings");
            settingsNavigationController.TabBarItem.Image = UIImage.FromBundle(Path.Combine("icons", "settings.png"));
            settingsNavigationController.TabBarItem.SelectedImage = UIImage.FromBundle(Path.Combine("icons", "settings-filled.png"));
            settingsNavigationController.Tag = SettingsTag;
            settingsNavigationController.RestorationIdentifier = "NavigationController_" + nameof(SettingsViewController);

            ViewControllers = new UIViewController[]
            {
                documentsNavigationController,
                contactsNavigationController,
                Dummy,
                shortcodesNavigationController,
                settingsNavigationController
            };

            SelectedIndex = 0;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            /*var numbers = new System.Collections.Generic.List<ContactPhoneNumber>();
            System.Threading.Tasks.Task.Run(async () =>
            {
                numbers = await Mobile.Common.Manager.Managers.ContactsManager.GetContactPhoneNumbers();
                Mobile.Common.CommonConfig.Logger.Info("AMOUNT OF NUMBERS: " + numbers.Count);
            });*/
            RestorationIdentifier = nameof(SimpleMainViewController);

        }
    }
}