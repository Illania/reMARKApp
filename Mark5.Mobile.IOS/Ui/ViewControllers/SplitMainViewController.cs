using System.IO;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
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
            documentSplitViewController.RestorationIdentifier = nameof(DocumentsSplitViewController);

            contactSplitViewController = new ContactsSplitViewController();
            contactSplitViewController.TabBarItem.Title = Localization.GetString("contacts");
            contactSplitViewController.TabBarItem.Image = UIImage.FromBundle(Path.Combine("icons", "contacts.png"));
            contactSplitViewController.TabBarItem.SelectedImage = UIImage.FromBundle(Path.Combine("icons", "contacts-filled.png"));
            contactSplitViewController.Tag = ContactsTag;
            contactSplitViewController.RestorationIdentifier = nameof(ContactsSplitViewController);

            shortcodeSplitViewController = new ShortcodesSplitViewController();
            shortcodeSplitViewController.TabBarItem.Title = Localization.GetString("shortcodes");
            shortcodeSplitViewController.TabBarItem.Image = UIImage.FromBundle(Path.Combine("icons", "shortcodes.png"));
            shortcodeSplitViewController.TabBarItem.SelectedImage = UIImage.FromBundle(Path.Combine("icons", "shortcodes-filled.png"));
            shortcodeSplitViewController.Tag = ShortcodesTag;
            shortcodeSplitViewController.RestorationIdentifier = nameof(ShortcodesSplitViewController);

            settingsNavigationController = new NavigationController(new SettingsViewController());
            settingsNavigationController.TabBarItem.Title = Localization.GetString("settings");
            settingsNavigationController.TabBarItem.Image = UIImage.FromBundle(Path.Combine("icons", "settings.png"));
            settingsNavigationController.TabBarItem.SelectedImage = UIImage.FromBundle(Path.Combine("icons", "settings-filled.png"));
            settingsNavigationController.Tag = SettingsTag;
            settingsNavigationController.RestorationIdentifier = "NavigationController_" + nameof(SettingsViewController);

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

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            ViewControllerSelected += ViewControllerSelected1;
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            ViewControllerSelected -= ViewControllerSelected1;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            RestorationIdentifier = nameof(SplitViewController);
        }

        void ViewControllerSelected1(object sender, UITabBarSelectionEventArgs e)
        {
            ModuleType module = ModuleType.None;
            if (e.ViewController == documentSplitViewController)
                module = ModuleType.Documents;
            if (e.ViewController == contactSplitViewController)
                module = ModuleType.Contacts;
            if (e.ViewController == shortcodeSplitViewController)
                module = ModuleType.Shortcodes;

            if (module != ModuleType.None)
                CommonConfig.UsageAnalytics.LogEvent(new OpenModuleEvent(module));
        }
    }
}