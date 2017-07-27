using System.IO;
using Mark5.Mobile.IOS.Ui.Common;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{
    public class SplitMainViewController : AbstractMainViewController
    {
        DocumentsSplitViewController documentSplitViewController;
        ContactsSplitViewController contactSplitViewController;
        ShortcodesSplitViewController shortcodeSplitViewController;
        NavigationController settingsNavigationController;

        public override void LoadView()
        {
            base.LoadView();

            documentSplitViewController = new DocumentsSplitViewController();
            documentSplitViewController.TabBarItem.Title = Localization.GetString("documents");
            documentSplitViewController.TabBarItem.Image = UIImage.FromBundle(Path.Combine("icons", "documents.png"));
            documentSplitViewController.TabBarItem.SelectedImage = UIImage.FromBundle(Path.Combine("icons", "documents-filled.png"));
            documentSplitViewController.Tag = DocumentsTag;

            contactSplitViewController = new ContactsSplitViewController();
            contactSplitViewController.TabBarItem.Title = Localization.GetString("contacts");
            contactSplitViewController.TabBarItem.Image = UIImage.FromBundle(Path.Combine("icons", "contacts.png"));
            contactSplitViewController.TabBarItem.SelectedImage = UIImage.FromBundle(Path.Combine("icons", "contacts-filled.png"));
            contactSplitViewController.Tag = ContactsTag;

            shortcodeSplitViewController = new ShortcodesSplitViewController();
            shortcodeSplitViewController.TabBarItem.Title = Localization.GetString("shortcodes");
            shortcodeSplitViewController.TabBarItem.Image = UIImage.FromBundle(Path.Combine("icons", "shortcodes.png"));
            shortcodeSplitViewController.TabBarItem.SelectedImage = UIImage.FromBundle(Path.Combine("icons", "shortcodes-filled.png"));
            shortcodeSplitViewController.Tag = ShortcodesTag;

            settingsNavigationController = new NavigationController(new SettingsViewController());
            settingsNavigationController.TabBarItem.Title = Localization.GetString("settings");
            settingsNavigationController.TabBarItem.Image = UIImage.FromBundle(Path.Combine("icons", "settings.png"));
            settingsNavigationController.TabBarItem.SelectedImage = UIImage.FromBundle(Path.Combine("icons", "settings-filled.png"));
            settingsNavigationController.Tag = SettingsTag;

            ViewControllers = new UIViewController[]
            {
                documentSplitViewController,
                contactSplitViewController,
                Dummy,
                shortcodeSplitViewController,
                settingsNavigationController
            };

            SelectedIndex = 0;
        }
    }
}