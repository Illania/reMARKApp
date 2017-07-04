using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using CoreGraphics;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.IOS.Model.HubMessages;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.TableViewCells;
using Mark5.Mobile.IOS.Ui.ViewControllers.ComposeDocumentView;
using Mark5.Mobile.IOS.Ui.ViewControllers.FoldersList;
using Mark5.Mobile.IOS.Utilities;
using Mark5.Mobile.IOS.Utilities.Extensions;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{
    public class ContactViewController : AbstractViewController, ISecondaryViewController
    {
        public bool Modal { get; set; }

        public bool Empty => folderId == null && folder == null && contactId == null && contactPreview == null && contact == null;

        int? folderId;
        Folder folder;

        int? contactId;
        ContactPreview contactPreview;
        Contact contact;

        bool refreshDataOnAppear;

        UIView headerView;
        UIButton button1;
        UIButton button2;
        UIButton button3;
        UIButton button4;
        UILabel nameLabel;
        UILabel nameSubLabel;
        UITableView tableView;
        UIToolbar toolbar;
        UIBarButtonItem assignCategoryButton;
        UIBarButtonItem fileToButton;
        UIBarButtonItem actionsLinksButton;
        UIBarButtonItem doneButtonItem;
        NSLayoutConstraint headerViewOffset;

        CancellationTokenSource cts;

        public override void LoadView()
        {
            base.LoadView();

            InitializeView();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            ExtendedLayoutIncludesOpaqueBars = true;
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            InitializeNavigationBar();
            InitializeHandlers();
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            CommonConfig.Logger.Info($"{nameof(ContactViewController)} appeared");

            headerViewOffset.Constant = NavigationController.NavigationBar.Frame.Bottom;
            tableView.ContentInset = new UIEdgeInsets(0f, 0f, 45f + (TabBarController?.TabBar?.Frame.Height ?? 0f), 0f);
            tableView.ScrollIndicatorInsets = new UIEdgeInsets(0f, 0f, 45f + (TabBarController?.TabBar?.Frame.Height ?? 0f), 0f);

            if (refreshDataOnAppear)
            {
                refreshDataOnAppear = false;
                RefreshData();
            }
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            DeinitializeHandlers();
        }

        public override void DidReceiveMemoryWarning()
        {
            CommonConfig.Logger.Warning($"{nameof(ContactViewController)} received memory warning!");

            GC.Collect();
            base.DidReceiveMemoryWarning();
        }

        public override void ViewWillTransitionToSize(CGSize toSize, IUIViewControllerTransitionCoordinator coordinator)
        {
            base.ViewWillTransitionToSize(toSize, coordinator);

            coordinator.AnimateAlongsideTransition(ctx => { },
                ctx =>
                {
                    if (tableView == null)
                        return;

                    headerViewOffset.Constant = NavigationController.NavigationBar.Frame.Bottom;
                    tableView.ContentInset = new UIEdgeInsets(0f, 0f, 45f + (TabBarController?.TabBar?.Frame.Height ?? 0f), 0f);
                    tableView.ScrollIndicatorInsets = new UIEdgeInsets(0f, 0f, 45f + (TabBarController?.TabBar?.Frame.Height ?? 0f), 0f);
                });
        }

        void InitializeView()
        {
            AutomaticallyAdjustsScrollViewInsets = false;

            View.BackgroundColor = Theme.DarkerBlue;

            headerView = new UIView();
            headerView.BackgroundColor = Theme.DarkerBlue;
            headerView.TranslatesAutoresizingMaskIntoConstraints = false;

            tableView = new UITableView(CGRect.Empty, UITableViewStyle.Grouped);
            tableView.ClipsToBounds = false;
            tableView.Source = new DataSource(this, tableView);
            tableView.TranslatesAutoresizingMaskIntoConstraints = false;
            tableView.RowHeight = UITableView.AutomaticDimension;
            tableView.EstimatedRowHeight = 60f;
            tableView.ContentInset = new UIEdgeInsets(0f, 0f, 45f + (TabBarController?.TabBar?.Frame.Height ?? 0f), 0f);
            tableView.ScrollIndicatorInsets = new UIEdgeInsets(0f, 0f, 45f + (TabBarController?.TabBar?.Frame.Height ?? 0f), 0f);
            tableView.AddGestureRecognizer(new UILongPressGestureRecognizer(RowLongPressed)
            {
                MinimumPressDuration = 1f
            });

            View.AddSubview(tableView);
            View.AddSubview(headerView);

            View.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(tableView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, headerView, NSLayoutAttribute.Bottom, 1f, 0f),
                NSLayoutConstraint.Create(tableView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, View, NSLayoutAttribute.Left, 1f, 0f),
                NSLayoutConstraint.Create(tableView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, View, NSLayoutAttribute.Right, 1f, 0f),
                NSLayoutConstraint.Create(tableView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, View, NSLayoutAttribute.Bottom, 1f, 0f),
                headerViewOffset = NSLayoutConstraint.Create(headerView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, View, NSLayoutAttribute.Top, 1f, NavigationController.NavigationBar.Frame.Bottom),
                NSLayoutConstraint.Create(headerView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, View, NSLayoutAttribute.Left, 1f, 0f),
                NSLayoutConstraint.Create(headerView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, View, NSLayoutAttribute.Right, 1f, 0f),
                NSLayoutConstraint.Create(headerView, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, 140f)
            });

            nameLabel = new UILabel();
            nameLabel.Font = Theme.DefaultFont.WithRelativeSize(6f);
            nameLabel.TextColor = Theme.LightGray;
            nameLabel.TextAlignment = UITextAlignment.Center;
            nameLabel.Lines = 1;
            nameLabel.TranslatesAutoresizingMaskIntoConstraints = false;
            headerView.AddSubview(nameLabel);
            headerView.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(nameLabel, NSLayoutAttribute.Top, NSLayoutRelation.Equal, headerView, NSLayoutAttribute.Top, 1f, 10f),
                NSLayoutConstraint.Create(nameLabel, NSLayoutAttribute.Left, NSLayoutRelation.Equal, headerView, NSLayoutAttribute.Left, 1f, 0f),
                NSLayoutConstraint.Create(nameLabel, NSLayoutAttribute.Right, NSLayoutRelation.Equal, headerView, NSLayoutAttribute.Right, 1f, 0f),
                NSLayoutConstraint.Create(nameLabel, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, 25f)
            });

            nameSubLabel = new UILabel();
            nameSubLabel.Font = Theme.DefaultFont.WithRelativeSize(-2f);
            nameSubLabel.TextColor = Theme.LightGray;
            nameSubLabel.TextAlignment = UITextAlignment.Center;
            nameSubLabel.Lines = 1;
            nameSubLabel.TranslatesAutoresizingMaskIntoConstraints = false;
            headerView.AddSubview(nameSubLabel);
            headerView.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(nameSubLabel, NSLayoutAttribute.Top, NSLayoutRelation.Equal, nameLabel, NSLayoutAttribute.Bottom, 1f, 5f),
                NSLayoutConstraint.Create(nameSubLabel, NSLayoutAttribute.Left, NSLayoutRelation.Equal, headerView, NSLayoutAttribute.Left, 1f, 0f),
                NSLayoutConstraint.Create(nameSubLabel, NSLayoutAttribute.Right, NSLayoutRelation.Equal, headerView, NSLayoutAttribute.Right, 1f, 0f),
                NSLayoutConstraint.Create(nameSubLabel, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, 20f)
            });

            var buttonsView = new UIView();
            buttonsView.TranslatesAutoresizingMaskIntoConstraints = false;
            headerView.AddSubview(buttonsView);
            headerView.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(buttonsView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, nameSubLabel, NSLayoutAttribute.Bottom, 1f, 10f),
                NSLayoutConstraint.Create(buttonsView, NSLayoutAttribute.CenterX, NSLayoutRelation.Equal, headerView, NSLayoutAttribute.CenterX, 1f, 0f)
            });

            button1 = new UIButton();
            button1.Enabled = false;
            button1.Alpha = 0f;
            button1.SetImage(UIImage.FromBundle(Path.Combine("icons", "large_email.png")).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate), UIControlState.Normal);
            button1.TintColor = Theme.LightGray;
            button1.TranslatesAutoresizingMaskIntoConstraints = false;
            buttonsView.AddSubview(button1);
            buttonsView.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(button1, NSLayoutAttribute.Top, NSLayoutRelation.Equal, buttonsView, NSLayoutAttribute.Top, 1f, 0f),
                NSLayoutConstraint.Create(button1, NSLayoutAttribute.Left, NSLayoutRelation.Equal, buttonsView, NSLayoutAttribute.Left, 1f, 0f),
                NSLayoutConstraint.Create(button1, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, buttonsView, NSLayoutAttribute.Bottom, 1f, 0f),
                NSLayoutConstraint.Create(button1, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, 60),
                NSLayoutConstraint.Create(button1, NSLayoutAttribute.Width, NSLayoutRelation.Equal, 1f, 40f)
            });

            button2 = new UIButton();
            button2.Enabled = false;
            button2.Alpha = 0f;
            button2.SetImage(UIImage.FromBundle(Path.Combine("icons", "large_mobile.png")).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate), UIControlState.Normal);
            button2.TintColor = Theme.LightGray;
            button2.TranslatesAutoresizingMaskIntoConstraints = false;
            buttonsView.AddSubview(button2);
            buttonsView.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(button2, NSLayoutAttribute.Top, NSLayoutRelation.Equal, buttonsView, NSLayoutAttribute.Top, 1f, 0f),
                NSLayoutConstraint.Create(button2, NSLayoutAttribute.Left, NSLayoutRelation.Equal, button1, NSLayoutAttribute.Right, 1f, 15f),
                NSLayoutConstraint.Create(button2, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, buttonsView, NSLayoutAttribute.Bottom, 1f, 0f),
                NSLayoutConstraint.Create(button2, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, 60f),
                NSLayoutConstraint.Create(button2, NSLayoutAttribute.Width, NSLayoutRelation.Equal, 1f, 40f)
            });

            button3 = new UIButton();
            button3.Enabled = false;
            button3.Alpha = 0f;
            button3.SetImage(UIImage.FromBundle(Path.Combine("icons", "large_sms.png")).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate), UIControlState.Normal);
            button3.TintColor = Theme.LightGray;
            button3.TranslatesAutoresizingMaskIntoConstraints = false;
            buttonsView.AddSubview(button3);
            buttonsView.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(button3, NSLayoutAttribute.Top, NSLayoutRelation.Equal, buttonsView, NSLayoutAttribute.Top, 1f, 0f),
                NSLayoutConstraint.Create(button3, NSLayoutAttribute.Left, NSLayoutRelation.Equal, button2, NSLayoutAttribute.Right, 1f, 15f),
                NSLayoutConstraint.Create(button3, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, buttonsView, NSLayoutAttribute.Bottom, 1f, 0f),
                NSLayoutConstraint.Create(button3, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, 60f),
                NSLayoutConstraint.Create(button3, NSLayoutAttribute.Width, NSLayoutRelation.Equal, 1f, 40f)
            });

            button4 = new UIButton();
            button4.Enabled = false;
            button4.Alpha = 0f;
            button4.SetImage(UIImage.FromBundle(Path.Combine("icons", "large_map.png")).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate), UIControlState.Normal);
            button4.TintColor = Theme.LightGray;
            button4.TranslatesAutoresizingMaskIntoConstraints = false;
            buttonsView.AddSubview(button4);
            buttonsView.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(button4, NSLayoutAttribute.Top, NSLayoutRelation.Equal, buttonsView, NSLayoutAttribute.Top, 1f, 0f),
                NSLayoutConstraint.Create(button4, NSLayoutAttribute.Left, NSLayoutRelation.Equal, button3, NSLayoutAttribute.Right, 1f, 15f),
                NSLayoutConstraint.Create(button4, NSLayoutAttribute.Right, NSLayoutRelation.Equal, buttonsView, NSLayoutAttribute.Right, 1f, 0f),
                NSLayoutConstraint.Create(button4, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, buttonsView, NSLayoutAttribute.Bottom, 1f, 0f),
                NSLayoutConstraint.Create(button4, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, 60f),
                NSLayoutConstraint.Create(button4, NSLayoutAttribute.Width, NSLayoutRelation.Equal, 1f, 40f)
            });


            assignCategoryButton = new UIBarButtonItem();
            assignCategoryButton.Image = UIImage.FromBundle(Path.Combine("icons", "flag.png"));
            assignCategoryButton.Enabled = false;

            fileToButton = new UIBarButtonItem();
            fileToButton.Image = UIImage.FromBundle(Path.Combine("icons", "worktray.png"));
            fileToButton.Enabled = false;

            actionsLinksButton = new UIBarButtonItem();
            actionsLinksButton.Image = UIImage.FromBundle(Path.Combine("icons", "actions.png"));
            actionsLinksButton.Enabled = false;

            toolbar = new UIToolbar();
            toolbar.BarStyle = UIBarStyle.Default;
            toolbar.Items = new[]
            {
                assignCategoryButton,
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                fileToButton,
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                actionsLinksButton
            };
            toolbar.BarTintColor = Theme.Gray;
            toolbar.TranslatesAutoresizingMaskIntoConstraints = false;
            toolbar.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Vertical);
            toolbar.SetContentCompressionResistancePriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Vertical);
            View.AddSubview(toolbar);
            View.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(toolbar, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, 45f),
                NSLayoutConstraint.Create(toolbar, NSLayoutAttribute.Left, NSLayoutRelation.Equal, View, NSLayoutAttribute.Left, 1f, 0f),
                NSLayoutConstraint.Create(toolbar, NSLayoutAttribute.Right, NSLayoutRelation.Equal, View, NSLayoutAttribute.Right, 1f, 0f),
                NSLayoutConstraint.Create(toolbar, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, View, NSLayoutAttribute.Bottom, 1f, -(TabBarController?.TabBar?.Frame.Height ?? 0f))
            });
        }

        void InitializeNavigationBar()
        {
            if (Modal)
            {
                doneButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Done);
                NavigationItem.SetRightBarButtonItem(doneButtonItem, false);
            }
        }

        void InitializeHandlers()
        {
            if (button1 != null)
                button1.TouchUpInside += Button1_TouchUpInside;

            if (button2 != null)
                button2.TouchUpInside += Button2_TouchUpInside;

            if (button3 != null)
                button3.TouchUpInside += Button3_TouchUpInside;

            if (button4 != null)
                button4.TouchUpInside += Button4_TouchUpInside;

            if (assignCategoryButton != null)
                assignCategoryButton.Clicked += AssignCategoryButton_Clicked;

            if (fileToButton != null)
                fileToButton.Clicked += FileToButton_Clicked;

            if (actionsLinksButton != null)
                actionsLinksButton.Clicked += ActionsLinksButton_Clicked;

            if (doneButtonItem != null)
                doneButtonItem.Clicked += DoneButtonItem_Clicked;
        }

        void DeinitializeHandlers()
        {
            if (button1 != null)
                button1.TouchUpInside -= Button1_TouchUpInside;

            if (button2 != null)
                button2.TouchUpInside -= Button2_TouchUpInside;

            if (button3 != null)
                button3.TouchUpInside -= Button3_TouchUpInside;

            if (button4 != null)
                button4.TouchUpInside -= Button4_TouchUpInside;

            if (assignCategoryButton != null)
                assignCategoryButton.Clicked -= AssignCategoryButton_Clicked;

            if (fileToButton != null)
                fileToButton.Clicked -= FileToButton_Clicked;

            if (actionsLinksButton != null)
                actionsLinksButton.Clicked -= ActionsLinksButton_Clicked;

            if (doneButtonItem != null)
                doneButtonItem.Clicked -= DoneButtonItem_Clicked;
        }

        void RowLongPressed(UILongPressGestureRecognizer gr)
        {
            if (gr.State != UIGestureRecognizerState.Began)
                return;

            var location = gr.LocationInView(tableView);
            var indexPath = tableView?.IndexPathForRowAtPoint(location);
            var cell = tableView?.CellAt(indexPath);
            var dataSource = tableView?.Source as DataSource;
            var row = dataSource?.RowAt(indexPath);
            if (cell != null && row != null)
                row.OnLongClicked(this, tableView, cell, indexPath);
        }

        void Button1_TouchUpInside(object sender, EventArgs e)
        {
            var primaryEmail = contact.CommunicationAddresses.FirstOrDefault(ca => ca.Type == CommunicationAddressType.Email && ca.IsPrimary);
            if (primaryEmail == null)
                return;

            var vc = new ComposeDocumentViewController
            {
                DocumentCreationModeFlag = DocumentCreationModeFlag.New,
                PreconfiguredEmailAddresses = new Dictionary<DocumentAddressType, string[]>
                {
                    { DocumentAddressType.To, new [] { primaryEmail.Address } }
                }
            };
            PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
        }

        async void Button2_TouchUpInside(object sender, EventArgs e)
        {
            var formattedNumbers = contact.CommunicationAddresses.Where(ca => (ca.Type == CommunicationAddressType.Mobile || ca.Type == CommunicationAddressType.Phone) && ca.IsPrimary).Select(ca => AddressFormatter.FormatCommunicationAddress(ca)).ToArray();
            if (formattedNumbers.Length == 0)
                return;

            if (formattedNumbers.Length == 1)
            {
                Integration.Call(this, (UIButton)sender, formattedNumbers[0]);
                return;
            }

            var selectedItem = await Dialogs.ShowListDialogAsync(this, Localization.GetString("call"), formattedNumbers, (UIButton)sender);
            if (selectedItem < 0)
                return;

            Integration.Call(this, (UIButton)sender, formattedNumbers[selectedItem]);
        }

        void Button3_TouchUpInside(object sender, EventArgs e)
        {
            var communicationAddresses = contact.CommunicationAddresses.FirstOrDefault(ca => ca.Type == CommunicationAddressType.Mobile && ca.IsPrimary);
            if (communicationAddresses == null)
                return;

            Integration.Text(this, (UIButton)sender, AddressFormatter.FormatCommunicationAddress(communicationAddresses));
        }

        async void Button4_TouchUpInside(object sender, EventArgs e)
        {
            var physicalAddress = contact.PhysicalAddresses.ToArray();
            if (physicalAddress.Length == 0)
                return;

            if (physicalAddress.Length == 1)
            {
                Integration.ShowOnMap(this, (UIButton)sender, physicalAddress[0]);
                return;
            }

            var selectedItem = await Dialogs.ShowListDialogAsync(this, Localization.GetString("show_on_map"), physicalAddress.Select(pa => pa.Type.Name).ToArray(), (UIButton)sender);
            if (selectedItem < 0)
                return;

            Integration.ShowOnMap(this, (UIButton)sender, physicalAddress[selectedItem]);
        }

        void AssignCategoryButton_Clicked(object sender, EventArgs e)
        {
            var vc = new CategoriesListViewController
            {
                BusinessEntityPreview = contactPreview
            };
            PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
        }

        void FileToButton_Clicked(object sender, EventArgs e)
        {
            var eas = UIAlertController.Create(null, null, UIAlertControllerStyle.ActionSheet);

            eas.AddAction(UIAlertAction.Create(Localization.GetString("copy_to_worktray"),
                UIAlertActionStyle.Default,
                a =>
                {
                    var vc = new CopyToWorktrayViewController
                    {
                        BusinessEntities = new List<IBusinessEntity>
                        {
                            contact
                        }
                    };
                    PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
                }));

            eas.AddAction(UIAlertAction.Create(Localization.GetString("copy_to_folder"),
                UIAlertActionStyle.Default,
                a =>
                {
                    var vc = new CopyMoveToFolderListViewController(new List<IBusinessEntity>
                    {
                        contactPreview
                    });
                    PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
                }));

            if (folder?.InternalType == FolderInternalType.FilterView || folder?.InternalType == FolderInternalType.Static || folder?.InternalType == FolderInternalType.Worktray)
                eas.AddAction(UIAlertAction.Create(Localization.GetString("move_to_folder"),
                    UIAlertActionStyle.Default,
                    a =>
                    {
                        var vc = new CopyMoveToFolderListViewController(new List<IBusinessEntity>
                            {
                                contactPreview
                            },
                            folder);
                        PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
                    }));

            if (folder?.InternalType == FolderInternalType.FilterView || folder?.InternalType == FolderInternalType.Static || folder?.InternalType == FolderInternalType.Worktray)
                eas.AddAction(UIAlertAction.Create(Localization.GetString("delete_from_folder"), UIAlertActionStyle.Default, RemoveFromFolder));

            if (ServerConfig.SystemSettings.UserInfo.IsSystemAdministrator || ServerConfig.SystemSettings.ContactsModuleInfo.Permissions.DeleteAllowed)
                eas.AddAction(UIAlertAction.Create(Localization.GetString("delete"), UIAlertActionStyle.Destructive, Delete));

            eas.AddAction(UIAlertAction.Create(Localization.GetString("cancel"), UIAlertActionStyle.Cancel, null));

            if (eas.PopoverPresentationController != null)
                eas.PopoverPresentationController.Delegate = new PopoverPresentationControllerDelegate((UIBarButtonItem)sender);

            PresentViewController(eas, true, null);
        }

        async void ActionsLinksButton_Clicked(object sender, EventArgs e)
        {
            var actionLinksListString = new string[]
            {
                Localization.GetString("actions"),
                Localization.GetString("links")
            };

            var result = await Dialogs.ShowListDialogAsync(this, null, actionLinksListString, actionsLinksButton);

            if (result < 0)
                return;

            UIViewController vc = null;

            switch (result)
            {
                case 0:
                    vc = new ObjectActionsListViewController(contact);
                    break;
                case 1:
                    vc = new ObjectLinksListViewController(contact);
                    break;
            }

            PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
        }

        void DoneButtonItem_Clicked(object sender, EventArgs e)
        {
            DismissViewController(true, null);
        }

        void CommunicationAddressClicked(UITableView tv, UITableViewCell cell, CommunicationAddress ca)
        {
            if (ca.Type == CommunicationAddressType.Email)
            {
                var vc = new ComposeDocumentViewController
                {
                    DocumentCreationModeFlag = DocumentCreationModeFlag.New,
                    PreconfiguredEmailAddresses = new Dictionary<DocumentAddressType, string[]>
	                {
	                    { DocumentAddressType.To, new [] { ca.Address } }
	                }
                };
                PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
            }

            if (ca.Type == CommunicationAddressType.Phone)
                Integration.Call(this, tv, cell, ca.Address);

            if (ca.Type == CommunicationAddressType.Mobile)
                Integration.CallOrText(this, tv, cell, ca.Address);
        }

        void PhysicalAddressClicked(UITableView tv, UITableViewCell cell, PhysicalAddress pa)
        {
            Integration.ShowOnMap(this, tv, cell, pa);
        }

        public void LinkedContactClicked(ContactPreview contactPreview)
        {
            var vc = new ContactViewController();
            vc.Modal = Modal;
            vc.SetData(contactPreview);
            vc.SetRefreshDataOnAppear();
            NavigationController.PushViewController(vc, true);
        }

        public void WebPageClicked(UITableView tableView, UITableViewCell cell, string webPageAddress)
        {
            Integration.OpenUrl(this, tableView, cell, webPageAddress);
        }

        public void CopyToClipboard(UITableView tableView, UITableViewCell cell, string text)
        {
            Integration.CopyToClipboard(this, tableView, cell, text);
        }

        public void SetData(int folderId, int contactId)
        {
            folder = null;
            contactPreview = null;
            contact = null;

            this.folderId = folderId;
            this.contactId = contactId;
        }

        public void SetData(Folder folder, ContactPreview contactPreview)
        {
            folderId = null;
            contactId = null;
            contact = null;

            this.folder = folder;
            this.contactPreview = contactPreview;
        }

        public void SetData(ContactPreview contactPreview)
        {
            folderId = null;
            folder = null;
            contactId = null;
            contact = null;

            this.contactPreview = contactPreview;
        }

        public void SetData(int contactId)
        {
            folderId = null;
            folder = null;
            contactPreview = null;
            contact = null;

            this.contactId = contactId;
        }

        public bool IsShowingContactWithId(int contactId)
        {
            return contactPreview?.Id == contactId || this.contactId == contactId;
        }

        public async void RefreshData()
        {
            cts?.Cancel();
            cts = new CancellationTokenSource();
            var token = cts.Token;

            var folderId = this.folderId;
            var folder = this.folder;
            var contactId = this.contactId;
            var contactPreview = this.contactPreview;

            CommonConfig.Logger.Info("Loading contact...");

            var ds = (DataSource)tableView?.Source;

            try
            {
                ds.StartRefresh();

                if (folderId != null && contactId != null)
                {
                    var swp = await Managers.ContactsManager.GetContactWithPreviewAsync(folderId.Value, contactId.Value);
                    this.contactPreview = swp.ContactPreview;
                    contact = swp.Contact;
                }

                if (folder != null && contactPreview != null)
                    contact = await Managers.ContactsManager.GetContactAsync(folder, contactPreview.Id);

                if (folderId == null && folder == null && contactPreview != null)
                    contact = await Managers.ContactsManager.GetContactAsync(-1, contactPreview.Id);

                if (folderId == null && folder == null && contactPreview == null)
                {
                    var swp = await Managers.ContactsManager.GetContactWithPreviewAsync(-1, contactId.Value);
                    this.contactPreview = swp.ContactPreview;
                    contact = swp.Contact;
                }

                contact.CommunicationAddresses = contact.CommunicationAddresses.OrderBy(ca => ca.Address).ToList();

                if (token.IsCancellationRequested)
                    return;

                if (this.contactPreview.Type == ContactType.Person || this.contactPreview.Type == ContactType.Department)
                {
                    nameLabel.Text = this.contactPreview.Name;
                    nameSubLabel.Text = this.contactPreview.CompanyName;
                }
                else
                {
                    nameLabel.Text = this.contactPreview.Name;
                    nameSubLabel.Text = string.Empty;
                }

                button1.Enabled = contact.CommunicationAddresses.Any(ca => ca.Type == CommunicationAddressType.Email && ca.IsPrimary);
                button2.Enabled = contact.CommunicationAddresses.Any(ca => (ca.Type == CommunicationAddressType.Mobile || ca.Type == CommunicationAddressType.Phone) && ca.IsPrimary);
                button3.Enabled = contact.CommunicationAddresses.Any(ca => ca.Type == CommunicationAddressType.Mobile && ca.IsPrimary);
                button4.Enabled = contact.PhysicalAddresses.Any();

                button1.Alpha = 1f;
                button2.Alpha = 1f;
                button3.Alpha = 1f;
                button4.Alpha = 1f;

                if (assignCategoryButton != null)
                    assignCategoryButton.Enabled = true;

                if (fileToButton != null)
                    fileToButton.Enabled = true;

                if (actionsLinksButton != null)
                    actionsLinksButton.Enabled = true;

                ds.EndRefresh(this.contactPreview, contact);
            }
            catch (Exception ex)
            {
                if (token.IsCancellationRequested)
                    return;

                CommonConfig.Logger.Error($"Could not load contact", ex);

                ds.Clear();

                await Dialogs.ShowErrorDialogAsync(this, ex);

                if (SplitViewController == null)
                    NavigationController.PopViewController(true);
            }
        }

        public void SetRefreshDataOnAppear()
        {
            refreshDataOnAppear = true;
        }

        public void ClearData()
        {
            cts?.Cancel();

            folderId = null;
            folder = null;
            contactId = null;
            contactPreview = null;
            contact = null;

            nameLabel.Text = string.Empty;
            nameSubLabel.Text = string.Empty;

            button1.Alpha = 0f;
            button2.Alpha = 0f;
            button3.Alpha = 0f;
            button4.Alpha = 0f;
            button1.Enabled = false;
            button2.Enabled = false;
            button3.Enabled = false;
            button4.Enabled = false;

            if (assignCategoryButton != null)
                assignCategoryButton.Enabled = false;

            if (fileToButton != null)
                fileToButton.Enabled = false;

            if (actionsLinksButton != null)
                actionsLinksButton.Enabled = false;

            var ds = tableView?.Source as DataSource;
            ds?.Clear();
        }

        #region Actions

        async void RemoveFromFolder(UIAlertAction a)
        {
            var result = await Dialogs.ShowYesNoDialogAsync(this, Localization.GetString("delete_from_folder"), Localization.GetString("confirm_delete_from_folder_contact"));

            if (!result)
                return;

            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("deleting_from_folder___"));

            try
            {
                CommonConfig.Logger.Info($"Attempting to remove contact from folder [contactId={contact.Id}, folderId={folder.Id}]");

                await Managers.CommonActionsManager.RemoveFromFolder(new List<IBusinessEntity>
                    {
                        contact
                    },
                    folder);

                CommonConfig.MessengerHub.Publish(new EntityRemovedFromFolderMessage(this,
                    ObjectType.Contact,
                    folder.Id,
                    new List<int>
                    {
                        contact.Id
                    }));

                dismissAction();

                if (SplitViewController != null && !SplitViewController.Collapsed)
                    ClearData();
                else
                    NavigationController.PopViewController(true);
            }
            catch (Exception ex)
            {
                dismissAction();

                CommonConfig.Logger.Error($"Error while removing contact from folder [contactId={contact.Id}, folderId={folder.Id}]", ex);
                await Dialogs.ShowErrorDialogAsync(this, ex);
            }
        }

        async void Delete(UIAlertAction a)
        {
            var result = await Dialogs.ShowYesNoDialogAsync(this, Localization.GetString("delete"), Localization.GetString("confirm_delete_contact"));

            if (!result)
                return;

            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("deleting___"));

            try
            {
                CommonConfig.Logger.Info($"Attempting to delete contact [contactId={contact.Id}]");

                await Managers.CommonActionsManager.Delete(new List<IBusinessEntity>
                {
                    contact
                });

                CommonConfig.MessengerHub.Publish(new EntityDeletedMessage(this,
                    ObjectType.Contact,
                    new List<int>
                    {
                        contact.Id
                    }));

                dismissAction();

                if (SplitViewController != null && !SplitViewController.Collapsed)
                    ClearData();
                else
                    NavigationController.PopViewController(true);
            }
            catch (Exception ex)
            {
                dismissAction();

                CommonConfig.Logger.Error($"Error while deleting contact [contactId={contact.Id}]", ex);
                await Dialogs.ShowErrorDialogAsync(this, ex);
            }
        }

        #endregion

        class DataSource : UITableViewSource, IDisposable
        {
            ContactViewController viewController;
            UITableView tableView;

            bool empty = true;
            bool loading = true;

            SectionCollection sections = new SectionCollection();

            public DataSource(ContactViewController viewController, UITableView tableView)
            {
                this.viewController = viewController;
                this.tableView = tableView;
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                if (empty)
                    return null;

                if (loading)
                    return tableView.DequeueReusableCell(WaitTableViewCell.Key) as WaitTableViewCell ?? WaitTableViewCell.Create();

                var row = sections[indexPath.Section].Rows[indexPath.Row];
                var cell = tableView.DequeueReusableCell(row.Key) ?? row.CreateCell();
                row.Bind(cell);
                return cell;
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                if (empty)
                    return 0;

                if (loading)
                    return 1;

                return sections[(int)section].Rows.Count;
            }

            public override nint NumberOfSections(UITableView tableView)
            {
                if (empty)
                    return 0;

                if (loading)
                    return 1;

                return sections.Count;
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                var cell = tableView.CellAt(indexPath);
                if (cell.SelectionStyle == UITableViewCellSelectionStyle.None)
                    return;

                sections[indexPath.Section].Rows[indexPath.Row].OnClicked(viewController, tableView, cell, indexPath);

                if (tableView?.IndexPathForSelectedRow != null)
                    tableView.DeselectRow(tableView.IndexPathForSelectedRow, true);
            }

            public void StartRefresh()
            {
                empty = false;
                loading = true;

                tableView.InsertSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
            }

            public void EndRefresh(ContactPreview contactPreview, Contact contact)
            {
                var allSections = new AbstractSection[]
                {
                    new CommunicationAddressSection(),
                    new PhysicalAddressSection(),
                    new LinkedContactSection(),
                    new ExtraSection()
                };

                foreach (var section in allSections)
                {
                    section.ContactPreview = contactPreview;
                    section.Contact = contact;

                    if (!section.Empty)
                    {
                        section.InitializeRows();
                        sections.Add(section);
                    }
                }

                allSections = null;

                if (sections.Count < 1)
                {
                    empty = true;
                    loading = false;

                    tableView.DeleteSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
                }
                else if (sections.Count == 1)
                {
                    empty = false;
                    loading = false;

                    tableView.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
                }
                else if (sections.Count > 1)
                {
                    empty = false;
                    loading = false;

                    tableView.BeginUpdates();
                    tableView.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
                    tableView.InsertSections(NSIndexSet.FromNSRange(new NSRange(1, sections.Count - 1)), UITableViewRowAnimation.Fade);
                    tableView.EndUpdates();
                }
            }

            public void Clear()
            {
                var numberOfSections = NumberOfSections(tableView);

                empty = true;
                loading = true;

                sections.Clear();

                tableView.BeginUpdates();
                tableView.DeleteSections(NSIndexSet.FromNSRange(new NSRange(0, numberOfSections)), UITableViewRowAnimation.Fade);
                tableView.EndUpdates();
            }

            public AbstractRow RowAt(NSIndexPath indexPath)
            {
                return sections[indexPath.Section].Rows[indexPath.Row];
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);

                tableView = null;
                viewController = null;

                sections = null;
            }

            #region Support classes

            public class SectionCollection : List<AbstractSection>
            {
            }

            public abstract class AbstractSection
            {
                WeakReference<ContactPreview> weakContactPreview;
                WeakReference<Contact> weakContact;

                public ContactPreview ContactPreview
                {
                    get
                    {
                        ContactPreview result = null;
                        return (weakContactPreview?.TryGetTarget(out result) ?? false) ? result : null;
                    }
                    set => weakContactPreview = new WeakReference<ContactPreview>(value);
                }

                public Contact Contact
                {
                    get
                    {
                        Contact result = null;
                        return (weakContact?.TryGetTarget(out result) ?? false) ? result : null;
                    }
                    set => weakContact = new WeakReference<Contact>(value);
                }

                public abstract bool Empty { get; }

                public RowCollection Rows { get; } = new RowCollection();

                public abstract void InitializeRows();
            }

            public class CommunicationAddressSection : AbstractSection
            {
                readonly CommunicationAddressType[] supportedSections =
                {
                    CommunicationAddressType.Mobile,
                    CommunicationAddressType.Phone,
                    CommunicationAddressType.Email,
                    CommunicationAddressType.Fax,
                    CommunicationAddressType.IM,
                    CommunicationAddressType.Telex,
                    CommunicationAddressType.Internal
                };


                public override bool Empty { get { return !Contact?.CommunicationAddresses?.Any(ca => supportedSections.Contains(ca.Type)) ?? true; } }

                public override void InitializeRows()
                {
                    if (Empty)
                        return;

                    var cas = Contact.CommunicationAddresses.Where(ca => ca.Type == CommunicationAddressType.Mobile);
                    foreach (var ca in cas)
                        Rows.Add(new CommunicationAddressRow(ContactPreview, Contact, ca));

                    cas = Contact.CommunicationAddresses.Where(ca => ca.Type == CommunicationAddressType.Phone);
                    foreach (var ca in cas)
                        Rows.Add(new CommunicationAddressRow(ContactPreview, Contact, ca));

                    cas = Contact.CommunicationAddresses.Where(ca => ca.Type == CommunicationAddressType.Email);
                    foreach (var ca in cas)
                        Rows.Add(new CommunicationAddressRow(ContactPreview, Contact, ca));

                    cas = Contact.CommunicationAddresses.Where(ca => ca.Type == CommunicationAddressType.Fax);
                    foreach (var ca in cas)
                        Rows.Add(new CommunicationAddressRow(ContactPreview, Contact, ca));

                    cas = Contact.CommunicationAddresses.Where(ca => ca.Type == CommunicationAddressType.IM);
                    foreach (var ca in cas)
                        Rows.Add(new CommunicationAddressRow(ContactPreview, Contact, ca));

                    cas = Contact.CommunicationAddresses.Where(ca => ca.Type == CommunicationAddressType.Telex);
                    foreach (var ca in cas)
                        Rows.Add(new CommunicationAddressRow(ContactPreview, Contact, ca));

                    cas = Contact.CommunicationAddresses.Where(ca => ca.Type == CommunicationAddressType.Internal);
                    foreach (var ca in cas)
                        Rows.Add(new CommunicationAddressRow(ContactPreview, Contact, ca));
                }
            }

            public class PhysicalAddressSection : AbstractSection
            {
                public override bool Empty => !Contact?.PhysicalAddresses?.Any() ?? true;

                public override void InitializeRows()
                {
                    if (Empty)
                        return;

                    var pas = Contact.PhysicalAddresses.ToArray();
                    foreach (var pa in pas)
                        Rows.Add(new PhysicalAddressRow(ContactPreview, Contact, pa));
                }
            }

            public class LinkedContactSection : AbstractSection
            {
                public override bool Empty => Contact?.PrimaryPerson == null && (!Contact?.Children?.Any() ?? true);

                public override void InitializeRows()
                {
                    if (Empty)
                        return;

                    if (Contact.PrimaryPerson != null)
                        Rows.Add(new LinkedContactRow(ContactPreview, Contact, Contact.PrimaryPerson));

                    var cps = Contact.Children.Where(cp => cp.Type == ContactType.Person);
                    foreach (var cp in cps)
                        Rows.Add(new LinkedContactRow(ContactPreview, Contact, cp));

                    cps = Contact.Children.Where(cp => cp.Type == ContactType.Department);
                    foreach (var cp in cps)
                        Rows.Add(new LinkedContactRow(ContactPreview, Contact, cp));

                    cps = Contact.Children.Where(cp => cp.Type == ContactType.Company);
                    foreach (var cp in cps)
                        Rows.Add(new LinkedContactRow(ContactPreview, Contact, cp));
                }
            }

            public class ExtraSection : AbstractSection
            {
                public override bool Empty => string.IsNullOrWhiteSpace(ContactPreview?.Description) && string.IsNullOrWhiteSpace(ContactPreview?.ShortId) && (!Contact?.ResponsibleUsers?.Any() ?? true) && (Contact?.BirthDateTimestamp == -6847804800000 || Contact?.BirthDateTimestamp == -1) && string.IsNullOrWhiteSpace(Contact?.WebPageAddress) && string.IsNullOrWhiteSpace(Contact?.Vat) && string.IsNullOrWhiteSpace(Contact?.Ledger) && string.IsNullOrWhiteSpace(Contact?.Account);

                public override void InitializeRows()
                {
                    if (!string.IsNullOrWhiteSpace(ContactPreview?.Description))
                        Rows.Add(new DescriptionRow(ContactPreview, Contact));
                    if (!string.IsNullOrWhiteSpace(ContactPreview?.ShortId))
                        Rows.Add(new ShortIdRow(ContactPreview, Contact));
                    if (!(!Contact?.ResponsibleUsers?.Any() ?? true))
                        Rows.Add(new ResponsibleUsersRow(ContactPreview, Contact));
                    if (!(Contact?.BirthDateTimestamp == -6847804800000 || Contact?.BirthDateTimestamp == -1))
                        Rows.Add(new BirthdateRow(ContactPreview, Contact));
                    if (!string.IsNullOrWhiteSpace(Contact?.WebPageAddress))
                        Rows.Add(new WebPageRow(ContactPreview, Contact));
                    if (!string.IsNullOrWhiteSpace(Contact?.Vat))
                        Rows.Add(new VatRow(ContactPreview, Contact));
                    if (!string.IsNullOrWhiteSpace(Contact?.Ledger))
                        Rows.Add(new LedgerRow(ContactPreview, Contact));
                    if (!string.IsNullOrWhiteSpace(Contact?.Account))
                        Rows.Add(new AccountRow(ContactPreview, Contact));
                }
            }

            public class RowCollection : List<AbstractRow>
            {
            }

            public abstract class AbstractRow
            {
                WeakReference<ContactPreview> weakContactPreview;
                WeakReference<Contact> weakContact;

                public ContactPreview ContactPreview
                {
                    get
                    {
                        ContactPreview result = null;
                        return (weakContactPreview?.TryGetTarget(out result) ?? false) ? result : null;
                    }
                    set => weakContactPreview = new WeakReference<ContactPreview>(value);
                }

                public Contact Contact
                {
                    get
                    {
                        Contact result = null;
                        return (weakContact?.TryGetTarget(out result) ?? false) ? result : null;
                    }
                    set => weakContact = new WeakReference<Contact>(value);
                }

                protected AbstractRow(ContactPreview contactPreview, Contact contact)
                {
                    ContactPreview = contactPreview;
                    Contact = contact;
                }

                public virtual string Key => ContactInfoTableViewCell.Key;

                public virtual UITableViewCell CreateCell()
                {
                    var cell = ContactInfoTableViewCell.Create();
                    cell.Accessory = UITableViewCellAccessory.None;
                    cell.SelectionStyle = UITableViewCellSelectionStyle.None;
                    return cell;
                }

                public abstract void Bind(UITableViewCell cell);

                public virtual void OnClicked(ContactViewController viewController, UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
                {
                }

                public virtual void OnLongClicked(ContactViewController viewController, UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
                {
                }
            }

            public class DescriptionRow : AbstractRow
            {
                public DescriptionRow(ContactPreview contactPreview, Contact contact)
                    : base(contactPreview, contact)
                {
                }

                public override void Bind(UITableViewCell cell)
                {
                    var cic = (ContactInfoTableViewCell)cell;
                    cic.Initialize(Localization.GetString("description").ToUpper(), ContactPreview.Description, true);
                }

                public override void OnLongClicked(ContactViewController viewController, UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
                {
                    viewController.CopyToClipboard(tableView, cell, ContactPreview.Description);
                }
            }

            public class CommunicationAddressRow : AbstractRow
            {
                readonly WeakReference<CommunicationAddress> weakCommunicationAddress;

                public CommunicationAddressRow(ContactPreview contactPreview, Contact contact, CommunicationAddress communicationAddress)
                    : base(contactPreview, contact)
                {
                    weakCommunicationAddress = new WeakReference<CommunicationAddress>(communicationAddress);
                }

                public override string Key
                {
                    get
                    {
                        CommunicationAddress ca;
                        weakCommunicationAddress.TryGetTarget(out ca);

                        if (string.IsNullOrWhiteSpace(ca.Description))
                            return CommunicationAddressCompactTableViewCell.Key;

                        return CommunicationAddressTableViewCell.Key;
                    }
                }

                public override UITableViewCell CreateCell()
                {
                    CommunicationAddress ca;
                    weakCommunicationAddress.TryGetTarget(out ca);

                    if (string.IsNullOrWhiteSpace(ca.Description))
                        return CommunicationAddressCompactTableViewCell.Create();

                    return CommunicationAddressTableViewCell.Create();
                }

                public override void Bind(UITableViewCell cell)
                {
                    CommunicationAddress ca;
                    weakCommunicationAddress.TryGetTarget(out ca);

                    if (string.IsNullOrWhiteSpace(ca.Description))
                    {
                        var cactcv = (CommunicationAddressCompactTableViewCell)cell;
                        cactcv.Initialize(ca);
                    }
                    else
                    {
                        var catcv = (CommunicationAddressTableViewCell)cell;
                        catcv.Initialize(ca);
                    }
                }

                public override void OnClicked(ContactViewController viewController, UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
                {
                    CommunicationAddress ca;
                    if (!weakCommunicationAddress.TryGetTarget(out ca))
                        return;

                    viewController.CommunicationAddressClicked(tableView, cell, ca);
                }

                public override void OnLongClicked(ContactViewController viewController, UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
                {
                    viewController.CopyToClipboard(tableView, cell, cell.TextLabel.Text);
                }
            }

            public class PhysicalAddressRow : AbstractRow
            {
                readonly WeakReference<PhysicalAddress> weakPhysicalAddress;

                public PhysicalAddressRow(ContactPreview contactPreview, Contact contact, PhysicalAddress physicalAddress)
                    : base(contactPreview, contact)
                {
                    weakPhysicalAddress = new WeakReference<PhysicalAddress>(physicalAddress);
                }

                public override string Key => PhysicalAddressTableViewCell.Key;

                public override UITableViewCell CreateCell()
                {
                    return PhysicalAddressTableViewCell.Create();
                }

                public override void Bind(UITableViewCell cell)
                {
                    PhysicalAddress pa;
                    weakPhysicalAddress.TryGetTarget(out pa);

                    var patvc = (PhysicalAddressTableViewCell)cell;
                    patvc.Initialize(pa);
                }

                public override void OnClicked(ContactViewController viewController, UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
                {
                    PhysicalAddress pa;
                    if (!weakPhysicalAddress.TryGetTarget(out pa))
                        return;

                    viewController.PhysicalAddressClicked(tableView, cell, pa);
                }

                public override void OnLongClicked(ContactViewController viewController, UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
                {
                    viewController.CopyToClipboard(tableView, cell, cell.TextLabel.Text);
                }
            }

            public class LinkedContactRow : AbstractRow
            {
                readonly WeakReference<ContactPreview> weakLinkedContactPreview;

                public LinkedContactRow(ContactPreview contactPreview, Contact contact, ContactPreview linkedContactPreview)
                    : base(contactPreview, contact)
                {
                    weakLinkedContactPreview = new WeakReference<ContactPreview>(linkedContactPreview);
                }

                public override string Key => base.Key + "_LinkedContact";

                public override UITableViewCell CreateCell()
                {
                    var cell = base.CreateCell();
                    cell.SelectionStyle = UITableViewCellSelectionStyle.Default;
                    cell.Accessory = UITableViewCellAccessory.DisclosureIndicator;
                    return cell;
                }

                public override void Bind(UITableViewCell cell)
                {
                    ContactPreview cp;
                    weakLinkedContactPreview.TryGetTarget(out cp);

                    string type;
                    if (Contact?.PrimaryPerson?.Id == cp.Id)
                        type = Localization.GetString("primary_person");
                    else if (cp.Type == ContactType.Person)
                        type = Localization.GetString("person");
                    else if (cp.Type == ContactType.Department)
                        type = Localization.GetString("department");
                    else
                        type = Localization.GetString("company");

                    var cic = (ContactInfoTableViewCell)cell;
                    cic.Initialize(type.ToUpper(), cp.Name);
                }

                public override void OnClicked(ContactViewController viewController, UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
                {
                    ContactPreview cp;
                    if (!weakLinkedContactPreview.TryGetTarget(out cp))
                        return;

                    viewController.LinkedContactClicked(cp);
                }
            }

            public class WebPageRow : AbstractRow
            {
                public WebPageRow(ContactPreview contactPreview, Contact contact)
                    : base(contactPreview, contact)
                {
                }

                public override string Key => base.Key + "_WebPage";

                public override UITableViewCell CreateCell()
                {
                    var cell = base.CreateCell();
                    cell.SelectionStyle = UITableViewCellSelectionStyle.Default;
                    return cell;
                }

                public override void Bind(UITableViewCell cell)
                {
                    var cic = (ContactInfoTableViewCell)cell;
                    cic.Initialize(Localization.GetString("webpage").ToUpper(), new NSAttributedString(Contact.WebPageAddress, foregroundColor: Theme.DarkBlue, underlineStyle: NSUnderlineStyle.Single));
                }

                public override void OnClicked(ContactViewController viewController, UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
                {
                    viewController.WebPageClicked(tableView, cell, Contact.WebPageAddress);
                }

                public override void OnLongClicked(ContactViewController viewController, UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
                {
                    viewController.CopyToClipboard(tableView, cell, Contact.WebPageAddress);
                }
            }

            public class BirthdateRow : AbstractRow
            {
                public BirthdateRow(ContactPreview contactPreview, Contact contact)
                    : base(contactPreview, contact)
                {
                }

                public override void Bind(UITableViewCell cell)
                {
                    var cic = (ContactInfoTableViewCell)cell;
                    cic.Initialize(Localization.GetString("birthdate").ToUpper(), Contact.BirthDateTimestamp.ConvertTimestampMillisecondsToDateTime().ConvertUtcToUserTime().ConvertDateTimeToTimestampMilliseconds().FormatUserTimestampAsLongDateString());
                }
            }

            public class AccountRow : AbstractRow
            {
                public AccountRow(ContactPreview contactPreview, Contact contact)
                    : base(contactPreview, contact)
                {
                }

                public override void Bind(UITableViewCell cell)
                {
                    var cic = (ContactInfoTableViewCell)cell;
                    cic.Initialize(Localization.GetString("account").ToUpper(), Contact.Account);
                }

                public override void OnLongClicked(ContactViewController viewController, UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
                {
                    viewController.CopyToClipboard(tableView, cell, Contact.Account);
                }
            }

            public class LedgerRow : AbstractRow
            {
                public LedgerRow(ContactPreview contactPreview, Contact contact)
                    : base(contactPreview, contact)
                {
                }

                public override void Bind(UITableViewCell cell)
                {
                    var cic = (ContactInfoTableViewCell)cell;
                    cic.Initialize(Localization.GetString("ledger").ToUpper(), Contact.Ledger);
                }

                public override void OnLongClicked(ContactViewController viewController, UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
                {
                    viewController.CopyToClipboard(tableView, cell, Contact.Ledger);
                }
            }

            public class VatRow : AbstractRow
            {
                public VatRow(ContactPreview contactPreview, Contact contact)
                    : base(contactPreview, contact)
                {
                }

                public override void Bind(UITableViewCell cell)
                {
                    var cic = (ContactInfoTableViewCell)cell;
                    cic.Initialize(Localization.GetString("vat").ToUpper(), Contact.Vat);
                }

                public override void OnLongClicked(ContactViewController viewController, UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
                {
                    viewController.CopyToClipboard(tableView, cell, Contact.Vat);
                }
            }

            public class ResponsibleUsersRow : AbstractRow
            {
                public ResponsibleUsersRow(ContactPreview contactPreview, Contact contact)
                    : base(contactPreview, contact)
                {
                }

                public override void Bind(UITableViewCell cell)
                {
                    var cic = (ContactInfoTableViewCell)cell;
                    cic.Initialize(Localization.GetString("responsible_users").ToUpper(), string.Join(", ", Contact.ResponsibleUsers.Values.OrderBy(s => s)));
                }
            }

            public class ShortIdRow : AbstractRow
            {
                public ShortIdRow(ContactPreview contactPreview, Contact contact)
                    : base(contactPreview, contact)
                {
                }

                public override void Bind(UITableViewCell cell)
                {
                    var cic = (ContactInfoTableViewCell)cell;
                    cic.Initialize(Localization.GetString("short_id").ToUpper(), ContactPreview.ShortId);
                }
            }

            #endregion
        }
    }
}