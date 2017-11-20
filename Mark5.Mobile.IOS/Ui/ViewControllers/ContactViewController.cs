using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using CoreGraphics;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Model.HubMessages;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Common.Utilities.Extensions;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.TableViewCells;
using Mark5.Mobile.IOS.Ui.ViewControllers.ComposeDocumentView;
using Mark5.Mobile.IOS.Ui.ViewControllers.FoldersList;
using Mark5.Mobile.IOS.Utilities;
using Mark5.Mobile.IOS.Utilities.Extensions;
using TinyMessenger;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{
    public class ContactViewController : AbstractTableViewController, ISecondaryViewController, IUIViewControllerRestoration
    {
        public bool Empty => folderId == null && folder == null && contactId == null && contactPreview == null && contact == null;

        int? folderId;
        Folder folder;

        int? contactId;
        ContactPreview contactPreview;
        Contact contact;

        bool refreshDataOnAppear;

        UIView headerView;
        UILabel nameLabel;
        UILabel nameSubLabel;
        UIButton button1;
        UIButton button2;
        UIButton button3;
        UIButton button4;

        UIBarButtonItem assignCategoryButton;
        UIBarButtonItem fileToButton;
        UIButton commentsButton;
        BadgeBarButtonItem commentsBadgeButton;
        UIBarButtonItem actionsLinksButton;
        UIBarButtonItem doneButtonItem;
        UIBarButtonItem editButtonItem;

        CancellationTokenSource cts;

        TinyMessageSubscriptionToken contactChangedToken;

        public ContactViewController()
            : base(UITableViewStyle.Grouped)
        {
            HidesBottomBarWhenPushed = true;
        }

        public override void LoadView()
        {
            base.LoadView();

            InitializeView();
            InitializeNavigationBar();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            RestorationIdentifier = nameof(ContactViewController);
            RestorationClass = Class;
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            if (Integration.IsRunningAtLeast(11))
            {
                if (NavigationController != null)
                    NavigationController.NavigationBar.PrefersLargeTitles = true;
                NavigationItem.LargeTitleDisplayMode = UINavigationItemLargeTitleDisplayMode.Never;
            }

            InitializeHandlers();
            SubscribeToMessages();

            if (NavigationController != null)
                NavigationController.ToolbarHidden = false;
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            CommonConfig.Logger.Info("Appeared");

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

            if (NavigationController != null)
                NavigationController.ToolbarHidden = true;
        }

        public override void DidReceiveMemoryWarning()
        {
            CommonConfig.Logger.Warning("Received memory warning!");

            UnsubscribeFromMessages();

            GC.Collect();
            base.DidReceiveMemoryWarning();
        }

        protected override void Recycle()
        {
            base.Recycle();

            doneButtonItem = null;
            fileToButton = null;
            editButtonItem = null;

            TableView.GestureRecognizers.ForEach(TableView.RemoveGestureRecognizer);
            ((DataSource)TableView.Source)?.Clear();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (CommonConfig.Logger.IsDebugEnabled())
                CommonConfig.Logger.Debug("Disposed");
        }

        void InitializeView()
        {
            TableView.Source = new DataSource(this, TableView);
            TableView.RowHeight = UITableView.AutomaticDimension;
            TableView.EstimatedRowHeight = 40f;
            TableView.BackgroundColor = Theme.White;
            TableView.AddGestureRecognizer(new UILongPressGestureRecognizer(RowLongPressed));

            headerView = new UIView(new CGRect(0f, 0f, 0f, 160f))
            {
                BackgroundColor = Theme.White
            };

            nameLabel = new UILabel
            {
                Font = Theme.DefaultFont.WithRelativeSize(6f),
                TextColor = Theme.DarkerBlue,
                TextAlignment = UITextAlignment.Center,
                Lines = 1,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            headerView.AddSubview(nameLabel);
            headerView.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(nameLabel, NSLayoutAttribute.Top, NSLayoutRelation.Equal, headerView, NSLayoutAttribute.Top, 1f, 10f),
                NSLayoutConstraint.Create(nameLabel, NSLayoutAttribute.Left, NSLayoutRelation.Equal, headerView, NSLayoutAttribute.Left, 1f, 0f),
                NSLayoutConstraint.Create(nameLabel, NSLayoutAttribute.Right, NSLayoutRelation.Equal, headerView, NSLayoutAttribute.Right, 1f, 0f),
                NSLayoutConstraint.Create(nameLabel, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, 25f)
            });

            nameSubLabel = new UILabel
            {
                Font = Theme.DefaultFont.WithRelativeSize(-2f),
                TextColor = Theme.DarkerBlue,
                TextAlignment = UITextAlignment.Center,
                Lines = 1,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            headerView.AddSubview(nameSubLabel);
            headerView.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(nameSubLabel, NSLayoutAttribute.Top, NSLayoutRelation.Equal, nameLabel, NSLayoutAttribute.Bottom, 1f, 5f),
                NSLayoutConstraint.Create(nameSubLabel, NSLayoutAttribute.Left, NSLayoutRelation.Equal, headerView, NSLayoutAttribute.Left, 1f, 0f),
                NSLayoutConstraint.Create(nameSubLabel, NSLayoutAttribute.Right, NSLayoutRelation.Equal, headerView, NSLayoutAttribute.Right, 1f, 0f),
                NSLayoutConstraint.Create(nameSubLabel, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, 20f)
            });

            var buttonsView = new UIView
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            headerView.AddSubview(buttonsView);
            headerView.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(buttonsView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, nameSubLabel, NSLayoutAttribute.Bottom, 1f, 10f),
                NSLayoutConstraint.Create(buttonsView, NSLayoutAttribute.CenterX, NSLayoutRelation.Equal, headerView, NSLayoutAttribute.CenterX, 1f, 0f)
            });

            button1 = new UIButton
            {
                Enabled = false,
                Alpha = 0f,
                TintColor = Theme.DarkerBlue,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            button1.SetImage(UIImage.FromBundle(Path.Combine("icons", "large_email.png")).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate), UIControlState.Normal);
            buttonsView.AddSubview(button1);
            buttonsView.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(button1, NSLayoutAttribute.Top, NSLayoutRelation.Equal, buttonsView, NSLayoutAttribute.Top, 1f, 0f),
                NSLayoutConstraint.Create(button1, NSLayoutAttribute.Left, NSLayoutRelation.Equal, buttonsView, NSLayoutAttribute.Left, 1f, 0f),
                NSLayoutConstraint.Create(button1, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, buttonsView, NSLayoutAttribute.Bottom, 1f, 0f),
                NSLayoutConstraint.Create(button1, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, 60),
                NSLayoutConstraint.Create(button1, NSLayoutAttribute.Width, NSLayoutRelation.Equal, 1f, 40f)
            });

            button2 = new UIButton
            {
                Enabled = false,
                Alpha = 0f,
                TintColor = Theme.DarkerBlue,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            button2.SetImage(UIImage.FromBundle(Path.Combine("icons", "large_mobile.png")).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate), UIControlState.Normal);
            buttonsView.AddSubview(button2);
            buttonsView.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(button2, NSLayoutAttribute.Top, NSLayoutRelation.Equal, buttonsView, NSLayoutAttribute.Top, 1f, 0f),
                NSLayoutConstraint.Create(button2, NSLayoutAttribute.Left, NSLayoutRelation.Equal, button1, NSLayoutAttribute.Right, 1f, 15f),
                NSLayoutConstraint.Create(button2, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, buttonsView, NSLayoutAttribute.Bottom, 1f, 0f),
                NSLayoutConstraint.Create(button2, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, 60f),
                NSLayoutConstraint.Create(button2, NSLayoutAttribute.Width, NSLayoutRelation.Equal, 1f, 40f)
            });

            button3 = new UIButton
            {
                Enabled = false,
                Alpha = 0f,
                TintColor = Theme.DarkerBlue,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            button3.SetImage(UIImage.FromBundle(Path.Combine("icons", "large_sms.png")).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate), UIControlState.Normal);
            buttonsView.AddSubview(button3);
            buttonsView.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(button3, NSLayoutAttribute.Top, NSLayoutRelation.Equal, buttonsView, NSLayoutAttribute.Top, 1f, 0f),
                NSLayoutConstraint.Create(button3, NSLayoutAttribute.Left, NSLayoutRelation.Equal, button2, NSLayoutAttribute.Right, 1f, 15f),
                NSLayoutConstraint.Create(button3, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, buttonsView, NSLayoutAttribute.Bottom, 1f, 0f),
                NSLayoutConstraint.Create(button3, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, 60f),
                NSLayoutConstraint.Create(button3, NSLayoutAttribute.Width, NSLayoutRelation.Equal, 1f, 40f)
            });

            button4 = new UIButton
            {
                Enabled = false,
                Alpha = 0f,
                TintColor = Theme.DarkerBlue,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            button4.SetImage(UIImage.FromBundle(Path.Combine("icons", "large_map.png")).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate), UIControlState.Normal);
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

            TableView.TableHeaderView = headerView;

            commentsButton = new UIButton
            {
                Frame = new CGRect(0f, 0f, 25f, 25f),
                Enabled = false
            };
            commentsButton.SetImage(UIImage.FromBundle(Path.Combine("icons", "comments.png")).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate), UIControlState.Normal);

            ToolbarItems = new[]
            {
                assignCategoryButton = new UIBarButtonItem
                {
                    Image = UIImage.FromBundle(Path.Combine("icons", "flag.png")),
                    Enabled = false
                },
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                fileToButton = new UIBarButtonItem
                {
                    Image = UIImage.FromBundle(Path.Combine("icons", "worktray.png")),
                    Enabled = false
                },
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                commentsBadgeButton = new BadgeBarButtonItem(commentsButton)
                {
                    Enabled = false
                },
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                actionsLinksButton = new UIBarButtonItem
                {
                    Image = UIImage.FromBundle(Path.Combine("icons", "actions.png")),
                    Enabled = false
                }
            };
        }

        void InitializeNavigationBar()
        {
            if (PresentingViewController == null)
            {
                if (ServerConfig.SystemSettings.ContactsModuleInfo.Permissions.CreateAllowed || ServerConfig.SystemSettings.ContactsModuleInfo.Permissions.EditAllowed)
                {
                    editButtonItem = new UIBarButtonItem
                    {
                        Image = UIImage.FromBundle(Path.Combine("icons", "pencil")),
                        Enabled = false
                    };
                    NavigationItem.SetRightBarButtonItem(editButtonItem, false);
                }
            }
            else
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

            if (commentsButton != null)
                commentsButton.TouchUpInside += CommentsButton_TouchUpInside;

            if (actionsLinksButton != null)
                actionsLinksButton.Clicked += ActionsLinksButton_Clicked;

            if (doneButtonItem != null)
                doneButtonItem.Clicked += DoneButtonItem_Clicked;

            if (editButtonItem != null)
                editButtonItem.Clicked += EditButtonItem_Clicked;
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

            if (commentsButton != null)
                commentsButton.TouchUpInside -= CommentsButton_TouchUpInside;

            if (actionsLinksButton != null)
                actionsLinksButton.Clicked -= ActionsLinksButton_Clicked;

            if (doneButtonItem != null)
                doneButtonItem.Clicked -= DoneButtonItem_Clicked;

            if (editButtonItem != null)
                editButtonItem.Clicked -= EditButtonItem_Clicked;
        }


        void SubscribeToMessages()
        {
            contactChangedToken = CommonConfig.MessengerHub.Subscribe<EntityChangedMessage>(HandleContactChangedMessage, m => m.ObjectType == ObjectType.Contact && contactPreview?.Id == m.EntityId);
        }

        void UnsubscribeFromMessages()
        {
            contactChangedToken?.Dispose();
        }

        void HandleContactChangedMessage(EntityChangedMessage obj) => RefreshAllOnAppear();

        void RefreshAllOnAppear()
        {
            ((DataSource)TableView.Source)?.Clear();
            contactId = contactPreview.Id;
            contactPreview = null;

            if (SplitViewController == null || SplitViewController.Collapsed)
                refreshDataOnAppear = true;
            else
                RefreshData();
        }

        void RowLongPressed(UILongPressGestureRecognizer gr)
        {
            if (gr.State != UIGestureRecognizerState.Began)
                return;

            var location = gr.LocationInView(TableView);
            var indexPath = TableView?.IndexPathForRowAtPoint(location);

            if (indexPath == null)
                return;

            var cell = TableView?.CellAt(indexPath);
            var dataSource = TableView?.Source as DataSource;
            var row = dataSource?.RowAt(indexPath);
            if (cell != null && row != null)
                row.OnLongClicked(this.Wrap(), TableView, cell, indexPath);
        }

        void Button1_TouchUpInside(object sender, EventArgs e)
        {
            CommonConfig.UsageAnalytics.LogEvent(new ContactFastActionEvent(ContactFastActionChoice.Email));

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
            CommonConfig.UsageAnalytics.LogEvent(new ContactFastActionEvent(ContactFastActionChoice.Call));

            var formattedNumbers = contact.CommunicationAddresses.Where(ca => (ca.Type == CommunicationAddressType.Mobile || ca.Type == CommunicationAddressType.Phone) && ca.IsPrimary)
                                          .Select(ca => AddressFormatter.FormatCommunicationAddress(ca)).ToArray();
            if (formattedNumbers.Length == 0)
                return;

            if (formattedNumbers.Length == 1)
            {
                Integration.Call(this, (UIButton)sender, formattedNumbers[0]);
                return;
            }

            var selectedItem = await Dialogs.ShowListActionSheetAsync(this, formattedNumbers, (UIButton)sender);
            if (selectedItem < 0)
                return;

            Integration.Call(this, (UIButton)sender, formattedNumbers[selectedItem]);
        }

        void Button3_TouchUpInside(object sender, EventArgs e)
        {
            CommonConfig.UsageAnalytics.LogEvent(new ContactFastActionEvent(ContactFastActionChoice.Text));

            var communicationAddresses = contact.CommunicationAddresses.FirstOrDefault(ca => ca.Type == CommunicationAddressType.Mobile && ca.IsPrimary);
            if (communicationAddresses == null)
                return;

            Integration.Text(this, (UIButton)sender, AddressFormatter.FormatCommunicationAddress(communicationAddresses));
        }

        async void Button4_TouchUpInside(object sender, EventArgs e)
        {
            CommonConfig.UsageAnalytics.LogEvent(new ContactFastActionEvent(ContactFastActionChoice.Map));

            var physicalAddress = contact.PhysicalAddresses.ToArray();
            if (physicalAddress.Length == 0)
                return;

            if (physicalAddress.Length == 1)
            {
                Integration.ShowOnMap(this, (UIButton)sender, physicalAddress[0]);
                return;
            }

            var selectedItem = await Dialogs.ShowListActionSheetAsync(this, physicalAddress.Select(pa => pa.Type.Name).ToArray(), (UIButton)sender);
            if (selectedItem < 0)
                return;

            Integration.ShowOnMap(this, (UIButton)sender, physicalAddress[selectedItem]);
        }

        async void EditButtonItem_Clicked(object sender, EventArgs e)
        {
            var listString = new List<string> { };

            if (ServerConfig.SystemSettings.ContactsModuleInfo.Permissions.EditAllowed)
                listString.Add(Localization.GetString("edit_contact"));

            if (ServerConfig.SystemSettings.ContactsModuleInfo.Permissions.CreateAllowed
                && contactPreview.Type == ContactType.Company)
                listString.Add(Localization.GetString("add_department"));
            if (ServerConfig.SystemSettings.ContactsModuleInfo.Permissions.CreateAllowed
                && (contactPreview.Type == ContactType.Company || contactPreview.Type == ContactType.Department))
                listString.Add(Localization.GetString("add_person"));

            var index = await Dialogs.ShowListActionSheetAsync(this, listString.ToArray(), editButtonItem);
            if (index < 0)
                return;

            var selectedString = listString[index];

            if (selectedString == Localization.GetString("edit_contact"))
            {
                var vc = new AddEditContactViewController
                {
                    ContactType = contactPreview.Type,
                    ContactPreview = contactPreview,
                    Contact = contact,
                    CreationModeFlag = ContactCreationModeFlag.Edit
                };
                PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
            }
            else
            {
                var type = selectedString == Localization.GetString("add_person") ? ContactType.Person : ContactType.Department;
                var vc = new AddEditContactViewController
                {
                    ContactType = type,
                    ParentContactPreview = contactPreview,
                    ParentPreselected = true,
                    CreationModeFlag = ContactCreationModeFlag.New,
                };
                PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
            }
        }

        void AssignCategoryButton_Clicked(object sender, EventArgs e)
        {
            var vc = new CategoriesListViewController { BusinessEntityPreview = contactPreview };
            PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
        }

        void FileToButton_Clicked(object sender, EventArgs e)
        {
            var eas = UIAlertController.Create(null, null, UIAlertControllerStyle.ActionSheet);

            eas.AddAction(UIAlertAction.Create(Localization.GetString("copy_to_worktray"),
                                               UIAlertActionStyle.Default,
                                               a =>
            {
                var vc = new CopyToWorktrayViewController { BusinessEntities = new List<IBusinessEntity> { contact } };
                PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
            }));

            eas.AddAction(UIAlertAction.Create(Localization.GetString("copy_to_folder"),
                                               UIAlertActionStyle.Default,
                                               a =>
            {
                var vc = new CopyMoveToFolderListViewController(ModuleType.Contacts, new List<IBusinessEntity> { contactPreview });
                PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
            }));

            if (folder?.InternalType == FolderInternalType.FilterView || folder?.InternalType == FolderInternalType.Static || folder?.InternalType == FolderInternalType.Worktray)
                eas.AddAction(UIAlertAction.Create(Localization.GetString("move_to_folder"),
                                                   UIAlertActionStyle.Default,
                                                   a =>
            {
                var vc = new CopyMoveToFolderListViewController(ModuleType.Contacts, new List<IBusinessEntity> { contactPreview }, folder);
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

        void CommentsButton_TouchUpInside(object sender, EventArgs e)
        {
            var vc = new CommentsListViewController { Entity = contact };
            PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
        }

        async void ActionsLinksButton_Clicked(object sender, EventArgs e)
        {
            var actionLinksListString = new[]
            {
                Localization.GetString("actions"),
                Localization.GetString("links")
            };

            var result = await Dialogs.ShowListActionSheetAsync(this, actionLinksListString, actionsLinksButton);
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

        void DoneButtonItem_Clicked(object sender, EventArgs e) => DismissViewController(true, null);

        async void CommunicationAddressClicked(UITableView tv, UITableViewCell cell, CommunicationAddress ca)
        {
            if (ca.Type == CommunicationAddressType.Email)
            {
                CommonConfig.UsageAnalytics.LogEvent(new ContactClickEmailEvent());

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
            {
                CommonConfig.UsageAnalytics.LogEvent(new ContactCallNumberEvent());
                Integration.Call(this, tv, cell, ca.Address);
            }

            if (ca.Type == CommunicationAddressType.Mobile)
            {
                var choices = new string[] { Localization.GetString("call"), Localization.GetString("send_text") };
                var result = await Dialogs.ShowListActionSheetAsync(this, choices, tv, cell);

                if (result == 0)
                {
                    CommonConfig.UsageAnalytics.LogEvent(new ContactCallNumberEvent());
                    Integration.Call(this, tv, cell, ca.Address);
                }
                if (result == 1)
                {
                    CommonConfig.UsageAnalytics.LogEvent(new ContactSendTextEvent());
                    Integration.Text(this, tv, cell, ca.Address);
                }
            }

        }

        public void LinkedContactClicked(ContactPreview contactPreview)
        {
            CommonConfig.UsageAnalytics.LogEvent(new ContactNavigateSubContactEvent());

            var vc = new ContactViewController();
            vc.SetData(contactPreview);
            vc.SetRefreshDataOnAppear();
            PresentViewController(new NavigationController(vc), true, null);
        }

        void PhysicalAddressClicked(UITableView tv, UITableViewCell cell, PhysicalAddress pa)
        {
            CommonConfig.UsageAnalytics.LogEvent(new ContactClickPhysicalAddressEvent());
            Integration.ShowOnMap(this, tv, cell, pa);
        }

        public void WebPageClicked(UITableView tableView, UITableViewCell cell, string webPageAddress) => Integration.OpenUrl(this, tableView, cell, webPageAddress);
        public void CopyToClipboard(UITableView tableView, UITableViewCell cell, string text) => Integration.CopyToClipboard(this, tableView, cell, text);

        public void SetData(int folderId, int contactId)
        {
            CommonConfig.UsageAnalytics.LogEvent(new OpenContactEvent());

            folder = null;
            contactPreview = null;
            contact = null;

            this.folderId = folderId;
            this.contactId = contactId;
        }

        public void SetData(Folder folder, ContactPreview contactPreview)
        {
            CommonConfig.UsageAnalytics.LogEvent(new OpenContactEvent());

            folderId = null;
            contactId = null;
            contact = null;

            this.folder = folder;
            this.contactPreview = contactPreview;
        }

        public void SetData(ContactPreview contactPreview)
        {
            CommonConfig.UsageAnalytics.LogEvent(new OpenContactEvent());

            folderId = null;
            folder = null;
            contactId = null;
            contact = null;

            this.contactPreview = contactPreview;
        }

        public void SetData(int contactId)
        {
            CommonConfig.UsageAnalytics.LogEvent(new OpenContactEvent());

            folderId = null;
            folder = null;
            contactPreview = null;
            contact = null;

            this.contactId = contactId;
        }

        public bool IsShowingContactWithId(int contactId) => contactPreview?.Id == contactId || this.contactId == contactId;

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

            var ds = (DataSource)TableView.Source;

            try
            {
                ds.StartRefresh();

                if ((folderId != null || folder != null) && contactId != null)
                {
                    var swp = await Managers.ContactsManager.GetContactWithPreviewAsync(folder?.Id ?? folderId, contactId.Value);

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

                if (this.contactPreview.Type == ContactType.Person)
                {
                    nameLabel.Text = this.contactPreview.Name;
                    if (!string.IsNullOrEmpty(this.contactPreview.CompanyName) && !string.IsNullOrEmpty(contact.Position))
                    {
                        nameSubLabel.Text = $"{contact.Position} @ {this.contactPreview.CompanyName}";
                    }
                    else
                    {
                        string subtitle = string.Empty;
                        subtitle += contact.Position;
                        subtitle += this.contactPreview.CompanyName;

                        nameSubLabel.Text = subtitle;
                    }

                }
                else if (this.contactPreview.Type == ContactType.Department)
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

                if (commentsBadgeButton != null)
                {
                    commentsBadgeButton.BadgeValue = contact?.Comments?.Count.ToString();
                    commentsBadgeButton.Enabled = contact != null;
                }

                if (commentsButton != null)
                    commentsButton.Enabled = contact != null;

                if (actionsLinksButton != null)
                    actionsLinksButton.Enabled = true;

                if (editButtonItem != null)
                {
                    editButtonItem.Enabled = true;
                    NavigationItem.SetRightBarButtonItem(editButtonItem, false);
                }

                if (doneButtonItem != null)
                    NavigationItem.SetRightBarButtonItem(doneButtonItem, false);

                ds.EndRefresh(this.contactPreview, contact);
            }
            catch (Exception ex)
            {
                if (token.IsCancellationRequested)
                    return;

                CommonConfig.Logger.Error($"Could not load contact", ex);

                ds.Clear();

                await Dialogs.ShowErrorAlertAsync(this, ex);

                if (SplitViewController == null || SplitViewController.Collapsed)
                {
                    if (PresentingViewController == null)
                        NavigationController?.PopViewController(true);
                    else
                        DismissViewController(true, null);
                }
            }
        }

        public void SetRefreshDataOnAppear() => refreshDataOnAppear = true;

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

            if (commentsBadgeButton != null)
            {
                commentsBadgeButton.SetBadgeValue("0", false);
                commentsBadgeButton.Enabled = false;
            }

            if (commentsButton != null)
                commentsButton.Enabled = false;

            if (actionsLinksButton != null)
                actionsLinksButton.Enabled = false;

            NavigationItem.SetRightBarButtonItem(null, false);

            ((DataSource)TableView.Source)?.Clear();
        }

        async void RemoveFromFolder(UIAlertAction a)
        {
            var d = new PopoverPresentationControllerDelegate(fileToButton);
            var result = await Dialogs.ShowDestructiveActionSheetAsync(this, Localization.GetString("delete_from_folder"), d);
            if (!result)
                return;

            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("deleting_from_folder___"));

            try
            {
                CommonConfig.Logger.Info($"Attempting to remove contact from folder [contactId={contact.Id}, folderId={folder.Id}]");

                await Managers.CommonActionsManager.RemoveFromFolder(new List<IBusinessEntity> { contact }, folder);

                CommonConfig.MessengerHub.Publish(new EntityRemovedFromFolderMessage(this, ObjectType.Contact, folder.Id, new List<int> { contact.Id }));

                dismissAction();

                if (SplitViewController != null && !SplitViewController.Collapsed)
                    ClearData();
                else
                    NavigationController?.PopViewController(true);
            }
            catch (Exception ex)
            {
                dismissAction();

                CommonConfig.Logger.Error($"Error while removing contact from folder [contactId={contact.Id}, folderId={folder.Id}]", ex);
                await Dialogs.ShowErrorAlertAsync(this, ex);
            }
        }

        async void Delete(UIAlertAction a)
        {
            var d = new PopoverPresentationControllerDelegate(fileToButton);
            var result = await Dialogs.ShowDestructiveActionSheetAsync(this, Localization.GetString("delete"), d);
            if (!result)
                return;

            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("deleting___"));

            try
            {
                CommonConfig.Logger.Info($"Attempting to delete contact [contactId={contact.Id}]");

                await Managers.CommonActionsManager.Delete(new List<IBusinessEntity> { contact });

                dismissAction();

                if (SplitViewController != null && !SplitViewController.Collapsed)
                    ClearData();
                else
                    NavigationController?.PopViewController(true);
            }
            catch (Exception ex)
            {
                dismissAction();

                CommonConfig.Logger.Error($"Error while deleting contact [contactId={contact.Id}]", ex);
                await Dialogs.ShowErrorAlertAsync(this, ex);
            }
        }

        void UpdateTitle(bool show)
        {
            var title = show ? nameLabel.Text : null;
            NavigationItem.Title = title;
        }

        class DataSource : UITableViewSource
        {
            readonly WeakReference<ContactViewController> viewControllerWeakReference;
            readonly WeakReference<UITableView> tableViewWeakReference;

            bool empty = true;
            bool loading = true;

            readonly SectionCollection sections = new SectionCollection();

            public DataSource(ContactViewController viewController, UITableView tableView)
            {
                viewControllerWeakReference = viewController.Wrap();
                tableViewWeakReference = tableView.Wrap();
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                if (empty)
                    return null;

                if (loading)
                    return tableView.DequeueReusableCell(WaitTableViewCell.DefaultId) as WaitTableViewCell ?? new WaitTableViewCell();

                var row = sections[indexPath.Section].Rows[indexPath.Row];
                var cell = tableView.DequeueReusableCell(row.Id) ?? row.CreateCell();
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

                sections[indexPath.Section].Rows[indexPath.Row].OnClicked(viewControllerWeakReference, tableView, cell, indexPath);

                if (tableView?.IndexPathForSelectedRow != null)
                    tableView.DeselectRow(tableView.IndexPathForSelectedRow, true);
            }

            public override void Scrolled(UIScrollView scrollView) => ScrollChanged(scrollView);
            public override void DraggingStarted(UIScrollView scrollView) => ScrollChanged(scrollView);
            public override void DraggingEnded(UIScrollView scrollView, bool willDecelerate) => ScrollChanged(scrollView);
            public override void DecelerationEnded(UIScrollView scrollView) => ScrollChanged(scrollView);

            void ScrollChanged(UIScrollView scrollView)
            {
                if (Integration.IsRunningAtLeast(11))
                {
                    var offset = scrollView.ContentOffset.Y;
                    var inset = -scrollView.SafeAreaInsets.Top;
                    var show = offset > inset + 30;

                    viewControllerWeakReference.Unwrap()?.UpdateTitle(show);
                }
            }

            public void StartRefresh()
            {
                empty = false;
                loading = true;

                tableViewWeakReference.Unwrap()?.InsertSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
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

                    tableViewWeakReference.Unwrap()?.DeleteSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
                }
                else if (sections.Count == 1)
                {
                    empty = false;
                    loading = false;

                    tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
                }
                else if (sections.Count > 1)
                {
                    empty = false;
                    loading = false;

                    tableViewWeakReference.Unwrap()?.BeginUpdates();
                    tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
                    tableViewWeakReference.Unwrap()?.InsertSections(NSIndexSet.FromNSRange(new NSRange(1, sections.Count - 1)), UITableViewRowAnimation.Fade);
                    tableViewWeakReference.Unwrap()?.EndUpdates();
                }
            }

            public void Clear()
            {
                var numberOfSections = tableViewWeakReference.Unwrap()?.NumberOfSections() ?? 0;

                empty = true;
                loading = true;

                sections.Clear();

                tableViewWeakReference.Unwrap()?.BeginUpdates();
                tableViewWeakReference.Unwrap()?.DeleteSections(NSIndexSet.FromNSRange(new NSRange(0, numberOfSections)), UITableViewRowAnimation.Fade);
                tableViewWeakReference.Unwrap()?.EndUpdates();
            }

            public AbstractRow RowAt(NSIndexPath indexPath) => sections[indexPath.Section].Rows[indexPath.Row];

            public class SectionCollection : List<AbstractSection>
            {
            }

            public abstract class AbstractSection
            {
                WeakReference<ContactPreview> weakContactPreview;
                WeakReference<Contact> weakContact;

                public ContactPreview ContactPreview
                {
                    get => weakContactPreview.Unwrap();
                    set => weakContactPreview = value.Wrap();
                }

                public Contact Contact
                {
                    get => weakContact.Unwrap();
                    set => weakContact = value.Wrap();
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

                    var cps = Contact.Children.Where(cp => cp.Type == ContactType.Person).OrderBy(cp => cp.Name);
                    foreach (var cp in cps)
                        Rows.Add(new LinkedContactRow(ContactPreview, Contact, cp));

                    cps = Contact.Children.Where(cp => cp.Type == ContactType.Department).OrderBy(cp => cp.Name);
                    foreach (var cp in cps)
                        Rows.Add(new LinkedContactRow(ContactPreview, Contact, cp));

                    cps = Contact.Children.Where(cp => cp.Type == ContactType.Company).OrderBy(cp => cp.Name);
                    foreach (var cp in cps)
                        Rows.Add(new LinkedContactRow(ContactPreview, Contact, cp));
                }
            }

            public class ExtraSection : AbstractSection
            {
                public override bool Empty => string.IsNullOrWhiteSpace(ContactPreview?.Description)
                                                    && string.IsNullOrWhiteSpace(ContactPreview?.ShortId) && (!Contact?.ResponsibleUsers?.Any() ?? true)
                                                    && (Contact?.BirthDateTimestamp == -1)
                                                    && string.IsNullOrWhiteSpace(Contact?.WebPageAddress) && string.IsNullOrWhiteSpace(Contact?.Vat) && string.IsNullOrWhiteSpace(Contact?.Ledger) && string.IsNullOrWhiteSpace(Contact?.Account);

                public override void InitializeRows()
                {
                    if (!string.IsNullOrWhiteSpace(ContactPreview?.Description))
                        Rows.Add(new DescriptionRow(ContactPreview, Contact));
                    if (!string.IsNullOrWhiteSpace(ContactPreview?.ShortId))
                        Rows.Add(new ShortIdRow(ContactPreview, Contact));
                    if (!(!Contact?.ResponsibleUsers?.Any() ?? true))
                        Rows.Add(new ResponsibleUsersRow(ContactPreview, Contact));
                    if (Contact?.BirthDateTimestamp != -1)
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
                    get => weakContactPreview.Unwrap();
                    set => weakContactPreview = value.Wrap();
                }

                public Contact Contact
                {
                    get => weakContact.Unwrap();
                    set => weakContact = value.Wrap();
                }

                protected AbstractRow(ContactPreview contactPreview, Contact contact)
                {
                    ContactPreview = contactPreview;
                    Contact = contact;
                }

                public virtual string Id => ContactInfoTableViewCell.DefaultId;
                public virtual UITableViewCell CreateCell() => new ContactInfoTableViewCell();
                public abstract void Bind(UITableViewCell cell);
                public virtual void OnClicked(WeakReference<ContactViewController> viewControllerWeakReference, UITableView tableView, UITableViewCell cell, NSIndexPath indexPath) { }
                public virtual void OnLongClicked(WeakReference<ContactViewController> viewControllerWeakReference, UITableView tableView, UITableViewCell cell, NSIndexPath indexPath) { }
            }

            public class DescriptionRow : AbstractRow
            {
                public DescriptionRow(ContactPreview contactPreview, Contact contact)
                    : base(contactPreview, contact)
                {
                }

                public override string Id { get => base.Id + "_Description"; }

                public override void Bind(UITableViewCell cell)
                {
                    var cic = (ContactInfoTableViewCell)cell;
                    cic.Accessory = UITableViewCellAccessory.None;
                    cic.SelectionStyle = UITableViewCellSelectionStyle.Default;
                    cic.Initialize(Localization.GetString("description").ToUpper(), ContactPreview.Description, true);
                }

                public override void OnLongClicked(WeakReference<ContactViewController> viewControllerWeakReference, UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
                {
                    viewControllerWeakReference.Unwrap()?.CopyToClipboard(tableView, cell, ContactPreview.Description);
                }
            }

            public class CommunicationAddressRow : AbstractRow
            {
                readonly WeakReference<CommunicationAddress> weakCommunicationAddress;

                public CommunicationAddressRow(ContactPreview contactPreview, Contact contact, CommunicationAddress communicationAddress)
                    : base(contactPreview, contact)
                {
                    weakCommunicationAddress = communicationAddress.Wrap();
                }

                public override string Id
                {
                    get
                    {
                        if (string.IsNullOrWhiteSpace(weakCommunicationAddress.Unwrap()?.Description))
                            return CommunicationAddressTableViewCell.CompactId;

                        return CommunicationAddressTableViewCell.DefaultId;
                    }
                }

                public override UITableViewCell CreateCell()
                {
                    if (string.IsNullOrWhiteSpace(weakCommunicationAddress.Unwrap()?.Description))
                        return new CommunicationAddressTableViewCell(CommunicationAddressTableViewCell.CompactId);

                    return new CommunicationAddressTableViewCell(CommunicationAddressTableViewCell.DefaultId);
                }

                public override void Bind(UITableViewCell cell)
                {
                    var ca = weakCommunicationAddress.Unwrap();

                    var catcv = (CommunicationAddressTableViewCell)cell;
                    catcv.Initialize(ca);
                }

                public override void OnClicked(WeakReference<ContactViewController> viewControllerWeakReference, UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
                {
                    viewControllerWeakReference.Unwrap()?.CommunicationAddressClicked(tableView, cell, weakCommunicationAddress.Unwrap());
                }

                public override void OnLongClicked(WeakReference<ContactViewController> viewControllerWeakReference, UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
                {
                    viewControllerWeakReference.Unwrap()?.CopyToClipboard(tableView, cell, cell.TextLabel.Text);
                }
            }

            public class PhysicalAddressRow : AbstractRow
            {
                readonly WeakReference<PhysicalAddress> weakPhysicalAddress;

                public PhysicalAddressRow(ContactPreview contactPreview, Contact contact, PhysicalAddress physicalAddress)
                    : base(contactPreview, contact)
                {
                    weakPhysicalAddress = physicalAddress.Wrap();
                }

                public override string Id => PhysicalAddressTableViewCell.DefaultId;

                public override UITableViewCell CreateCell() => new PhysicalAddressTableViewCell();

                public override void Bind(UITableViewCell cell)
                {
                    var patvc = (PhysicalAddressTableViewCell)cell;
                    patvc.Initialize(weakPhysicalAddress.Unwrap());
                }

                public override void OnClicked(WeakReference<ContactViewController> viewControllerWeakReference, UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
                {
                    viewControllerWeakReference.Unwrap()?.PhysicalAddressClicked(tableView, cell, weakPhysicalAddress.Unwrap());
                }

                public override void OnLongClicked(WeakReference<ContactViewController> viewControllerWeakReference, UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
                {
                    viewControllerWeakReference.Unwrap()?.CopyToClipboard(tableView, cell, cell.TextLabel.Text);
                }
            }

            public class LinkedContactRow : AbstractRow
            {
                readonly WeakReference<ContactPreview> weakLinkedContactPreview;

                public LinkedContactRow(ContactPreview contactPreview, Contact contact, ContactPreview linkedContactPreview)
                    : base(contactPreview, contact)
                {
                    weakLinkedContactPreview = linkedContactPreview.Wrap();
                }

                public override string Id => base.Id + "_LinkedContact";

                public override void Bind(UITableViewCell cell)
                {
                    var cp = weakLinkedContactPreview.Unwrap();

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
                    cell.Accessory = UITableViewCellAccessory.DisclosureIndicator;
                    cell.SelectionStyle = UITableViewCellSelectionStyle.Default;
                    cic.Initialize(type.ToUpper(), cp.Name);
                }

                public override void OnClicked(WeakReference<ContactViewController> viewControllerWeakReference, UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
                {
                    viewControllerWeakReference?.Unwrap().LinkedContactClicked(weakLinkedContactPreview.Unwrap());
                }
            }

            public class WebPageRow : AbstractRow
            {
                public WebPageRow(ContactPreview contactPreview, Contact contact)
                    : base(contactPreview, contact)
                {
                }

                public override string Id => base.Id + "_WebPage";

                public override void Bind(UITableViewCell cell)
                {
                    var cic = (ContactInfoTableViewCell)cell;
                    cic.Accessory = UITableViewCellAccessory.DisclosureIndicator;
                    cic.SelectionStyle = UITableViewCellSelectionStyle.Default;
                    cic.Initialize(Localization.GetString("webpage").ToUpper(), Contact.WebPageAddress);
                }

                public override void OnClicked(WeakReference<ContactViewController> viewControllerWeakReference, UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
                {
                    viewControllerWeakReference.Unwrap()?.WebPageClicked(tableView, cell, Contact.WebPageAddress);
                }

                public override void OnLongClicked(WeakReference<ContactViewController> viewControllerWeakReference, UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
                {
                    viewControllerWeakReference.Unwrap()?.CopyToClipboard(tableView, cell, Contact.WebPageAddress);
                }
            }

            public class BirthdateRow : AbstractRow
            {
                public BirthdateRow(ContactPreview contactPreview, Contact contact)
                    : base(contactPreview, contact)
                {
                }

                public override string Id => base.Id + "_Birthdate";

                public override void Bind(UITableViewCell cell)
                {
                    var cic = (ContactInfoTableViewCell)cell;
                    cic.Accessory = UITableViewCellAccessory.None;
                    cic.SelectionStyle = UITableViewCellSelectionStyle.None;
                    cic.Initialize(Localization.GetString("birthdate").ToUpper(), Contact.BirthDateTimestamp.ConvertTimestampMillisecondsToDateTime()
                                   .ConvertUtcToServerTime()
                                   .ConvertDateTimeToTimestampMilliseconds()
                                   .FormatUserTimestampAsLongDateString());
                }
            }

            public class AccountRow : AbstractRow
            {
                public AccountRow(ContactPreview contactPreview, Contact contact)
                    : base(contactPreview, contact)
                {
                }

                public override string Id => base.Id + "_Account";

                public override void Bind(UITableViewCell cell)
                {
                    var cic = (ContactInfoTableViewCell)cell;
                    cic.Accessory = UITableViewCellAccessory.None;
                    cic.SelectionStyle = UITableViewCellSelectionStyle.Default;
                    cic.Initialize(Localization.GetString("account").ToUpper(), Contact.Account);
                }

                public override void OnLongClicked(WeakReference<ContactViewController> viewControllerWeakReference, UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
                {
                    viewControllerWeakReference.Unwrap()?.CopyToClipboard(tableView, cell, Contact.Account);
                }
            }

            public class LedgerRow : AbstractRow
            {
                public LedgerRow(ContactPreview contactPreview, Contact contact)
                    : base(contactPreview, contact)
                {
                }

                public override string Id => base.Id + "_Ledger";

                public override void Bind(UITableViewCell cell)
                {
                    var cic = (ContactInfoTableViewCell)cell;
                    cic.Accessory = UITableViewCellAccessory.None;
                    cic.SelectionStyle = UITableViewCellSelectionStyle.Default;
                    cic.Initialize(Localization.GetString("ledger").ToUpper(), Contact.Ledger);
                }

                public override void OnLongClicked(WeakReference<ContactViewController> viewControllerWeakReference, UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
                {
                    viewControllerWeakReference.Unwrap()?.CopyToClipboard(tableView, cell, Contact.Ledger);
                }
            }

            public class VatRow : AbstractRow
            {
                public VatRow(ContactPreview contactPreview, Contact contact)
                    : base(contactPreview, contact)
                {
                }

                public override string Id => base.Id + "_Vat";

                public override void Bind(UITableViewCell cell)
                {
                    var cic = (ContactInfoTableViewCell)cell;
                    cic.Accessory = UITableViewCellAccessory.None;
                    cic.SelectionStyle = UITableViewCellSelectionStyle.Default;
                    cic.Initialize(Localization.GetString("vat").ToUpper(), Contact.Vat);
                }

                public override void OnLongClicked(WeakReference<ContactViewController> viewControllerWeakReference, UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
                {
                    viewControllerWeakReference.Unwrap()?.CopyToClipboard(tableView, cell, Contact.Vat);
                }
            }

            public class ResponsibleUsersRow : AbstractRow
            {
                public ResponsibleUsersRow(ContactPreview contactPreview, Contact contact)
                    : base(contactPreview, contact)
                {
                }

                public override string Id => base.Id + "_ResponsibleUsers";

                public override void Bind(UITableViewCell cell)
                {
                    var cic = (ContactInfoTableViewCell)cell;
                    cic.Accessory = UITableViewCellAccessory.None;
                    cic.SelectionStyle = UITableViewCellSelectionStyle.None;
                    cic.Initialize(Localization.GetString("responsible_users").ToUpper(), string.Join(", ", Contact.ResponsibleUsers.Values.OrderBy(s => s)));
                }
            }

            public class ShortIdRow : AbstractRow
            {
                public ShortIdRow(ContactPreview contactPreview, Contact contact)
                    : base(contactPreview, contact)
                {
                }

                public override string Id => base.Id + "_ShortId";

                public override void Bind(UITableViewCell cell)
                {
                    var cic = (ContactInfoTableViewCell)cell;
                    cic.Accessory = UITableViewCellAccessory.None;
                    cic.SelectionStyle = UITableViewCellSelectionStyle.None;
                    cic.Initialize(Localization.GetString("short_id").ToUpper(), ContactPreview.ShortId);
                }
            }
        }

        public override void EncodeRestorableState(NSCoder coder)
        {
            base.EncodeRestorableState(coder);

            coder.Encode(PresentingViewController != null, "doNotRestore");

            if (folderId.HasValue)
                coder.Encode(folderId.Value, "folderId");
            if (folder != null)
                coder.Encode(Serializer.SerializeToByteArray(folder.ShallowCopy()), "folder");
            if (contactId.HasValue)
                coder.Encode(contactId.Value, "contactId");
            if (contactPreview != null)
                coder.Encode(Serializer.SerializeToByteArray(contactPreview), "contactPreview");
            if (contact != null)
                coder.Encode(Serializer.SerializeToByteArray(contact), "contact");
        }

        public override void DecodeRestorableState(NSCoder coder)
        {
            base.DecodeRestorableState(coder);

            if (coder.ContainsKey("folderId"))
                folderId = coder.DecodeInt("folderId");
            if (folder != null)
                folder = Serializer.DeserializeFromByteArray<Folder>(coder.DecodeBytes("folder"));
            if (coder.ContainsKey("contactId"))
                contactId = coder.DecodeInt("contactId");
            if (coder.ContainsKey("contactPreview"))
                contactPreview = Serializer.DeserializeFromByteArray<ContactPreview>(coder.DecodeBytes("contactPreview"));
            if (coder.ContainsKey("contact"))
                contact = Serializer.DeserializeFromByteArray<Contact>(coder.DecodeBytes("contact"));

            refreshDataOnAppear = true;
        }

        [Export("viewControllerWithRestorationIdentifierPath:coder:")]
        public static UIViewController Restore(string[] identifierComponents, NSCoder coder)
        {
            if (coder.DecodeBool("doNotRestore"))
                return null;

            return new ContactViewController();
        }
    }
}